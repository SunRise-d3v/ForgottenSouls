namespace ForgottenSouls.Content.NPCs;

internal sealed class BonePile : ModNPC
{
    public override string Texture => Helper.TextureLoadNPC("BonePile");

    private const int ReviveTicks = 6 * 60;

    private int OriginalNetId => (int)NPC.ai[0];

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 1;

        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers { Hide = true });
    }

    public override void SetDefaults()
    {
        NPC.width = 36;
        NPC.height = 28;
        NPC.lifeMax = 100;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.dontCountMe = true;
        NPC.value = 0f;
        NPC.HitSound = SoundID.NPCHit2;
        NPC.DeathSound = SoundID.NPCDeath2;
    }

    public override void AI()
    {
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

        NPC sample = ContentSamples.NpcsByNetId[netId];

        int index = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Bottom.Y, sample.type);

        if (index >= 0 && index < Main.maxNPCs)
        {
            NPC copy = Main.npc[index];

            copy.SetDefaults(netId);
            copy.GetGlobalNPC<SecondLifeNPC>().Revived = true;
            copy.netUpdate = true;
        }

        BoneDust(24);

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

internal sealed class SecondLifeNPC : GlobalNPC
{
    internal bool Revived;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => IsSkeletonType(npc.type);

    public override void OnKill(NPC npc)
    {
        if (!Sadomasochist.Active)
            return;

        if (Revived || Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (npc.boss || npc.friendly || npc.townNPC)
            return;

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
        if (Pool.Excluded.Contains(type))
            return false;

        if (Pool.ExtraSkeletons.Contains(type))
            return true;

        if (type >= NPCID.Count || !NPCID.Search.TryGetName(type, out string name))
            return false;

        if (name.Contains("Skeletron"))
            return false;

        return name.Contains("Skeleton") || name.Contains("AngryBones");
    }
}