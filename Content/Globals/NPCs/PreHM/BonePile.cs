using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ForgottenSouls.Content.NPCs;

/// <summary>
/// Куча костей: неподвижный безобидный NPC на месте смерти скелета.
/// Если за 6 секунд её не уничтожить, из неё встаёт точная копия убитого скелета.
/// Спрайт: Content/NPCs/BonePile.png (один кадр).
/// </summary>
internal class BonePile : ModNPC
{
    public override string Texture => Helper.TextureLoad("NPCs", "BonePile");

    private const int ReviveTicks = 6 * 60;   // 6 секунд

    // ai[0] хранит netID убитого скелета (чтобы воскресить его точную копию), ai[1] это таймер
    private int OriginalNetId => (int)NPC.ai[0];

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 1;

        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers { Hide = true });
    }

    public override void SetDefaults()
    {
        NPC.width = 36;       // подгоните под спрайт
        NPC.height = 28;
        NPC.lifeMax = 100;
        NPC.damage = 0;       // безобидна
        NPC.defense = 0;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.dontCountMe = true;   // не занимает место в лимите спавна других мобов
        NPC.value = 0f;
        NPC.HitSound = SoundID.NPCHit2;
        NPC.DeathSound = SoundID.NPCDeath2;
    }

    public override void AI()
    {
        // Полностью неподвижна, только садится на землю
        NPC.velocity.X = 0f;
        NPC.velocity.Y = Math.Min(NPC.velocity.Y + 0.3f, 10f);

        if (++NPC.ai[1] < ReviveTicks)
            return;

        Revive();
    }

    private void Revive()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        int netId = OriginalNetId;

        // Вариант скелета (например, бронированный или с луком) определяется netID, а не базовым типом
        NPC sample = ContentSamples.NpcsByNetId[netId];

        int index = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Bottom.Y, sample.type);

        if (index >= 0 && index < Main.maxNPCs)
        {
            NPC copy = Main.npc[index];

            copy.SetDefaults(netId);
            copy.GetGlobalNPC<SecondLifeNPC>().Revived = true;   // второй жизни больше не будет
            copy.netUpdate = true;
        }

        BoneDust(24);

        // Куча исчезает без выпадения предметов
        NPC.active = false;
        NPC.netUpdate = true;
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        BoneDust(NPC.life > 0 ? 4 : 14);
    }

    private void BoneDust(int count)
    {
        if (Main.dedServ)
            return;

        for (int i = 0; i < count; i++)
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Bone, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -0.5f));
    }
}

/// <summary>Скелеты оставляют после смерти кучу костей (один раз), см. BonePile.</summary>
internal class SecondLifeNPC : GlobalNPC
{
    // Не считаются скелетами (боссы и особые враги, которые вы исключили, плюс торговец-скелет)
    private static readonly HashSet<int> Excluded =
    [
        NPCID.DungeonGuardian,
        NPCID.Tim,
        NPCID.RuneWizard,
        NPCID.SkeletonMerchant,
        NPCID.SkeletronHand,
        NPCID.SkeletronHead,
        NPCID.SkeletronPrime
    ];

    // Скелеты, в названии которых нет слова Skeleton, и скелеты-маги подземелья
    private static readonly HashSet<int> ExtraSkeletons =
    [
        NPCID.UndeadMiner,
        NPCID.UndeadViking,
        NPCID.Necromancer,
        NPCID.NecromancerArmored,
        NPCID.RaggedCaster,
        NPCID.RaggedCasterOpenCoat,
        NPCID.DiabolistRed,
        NPCID.DiabolistWhite,
        NPCID.DarkCaster,
    ];

    // true у скелета, который уже вставал из кучи
    internal bool Revived;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => IsSkeletonType(npc.type);

    public override void OnKill(NPC npc)
    {
        // Кучу создаёт сервер (или одиночная игра)
        if (Revived || Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (npc.boss || npc.friendly || npc.townNPC)
            return;

        // В ai[0] кучи передаём netID, чтобы она знала, кого воскрешать
        NPC.NewNPC(
            npc.GetSource_Death(),
            (int)npc.Center.X,
            (int)npc.Bottom.Y,
            ModContent.NPCType<BonePile>(),
            0,
            npc.netID);
    }

    private static bool IsSkeletonType(int type)
    {
        if (Excluded.Contains(type))
            return false;

        if (ExtraSkeletons.Contains(type))
            return true;

        // NPC из модов в словаре NPCID.Search отсутствуют (их типы начинаются с NPCID.Count), их не считаем скелетами
        if (type >= NPCID.Count || !NPCID.Search.TryGetName(type, out string name))
            return false;

        // Скелетрон и его части содержат слово Skeleton, но это боссы
        if (name.Contains("Skeletron"))
            return false;

        return name.Contains("Skeleton") || name.Contains("AngryBones");
    }
}