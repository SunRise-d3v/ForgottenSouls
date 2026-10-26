namespace ForgottenSouls.Content.NPCs;

internal sealed class WeddingCouple : GlobalNPC
{
    private const float PairSpawnDistance = 50f;

    private const float SpeedBonus = 0.6f;     
    private const float HealthBonus = 0.99f;     
    private const float RageScale = 1.175f; 
    private const float RedTint = 0.35f;   
    private const int ZombiesMin = 3;
    private const int ZombiesMax = 7;
    private const int SummonIntervalMin = 20 * 60;
    private const int SummonIntervalMax = 30 * 60;
    private const int MaxZombiesInWorld = 30;    
    private const int defenceScale = 5;

    private int _partner = -1;
    private bool _enraged;
    private int _summonTimer;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC npc, bool lateInstantiation)
        => npc.type == NPCID.TheGroom || npc.type == NPCID.TheBride;

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (source is not EntitySource_SpawnNPC || Main.netMode == NetmodeID.MultiplayerClient)
            return;

        int partnerType = npc.type == NPCID.TheGroom ? NPCID.TheBride : NPCID.TheGroom;
        int side = Main.rand.NextBool() ? 1 : -1;

        int index = NPC.NewNPC(
            npc.GetSource_FromAI(),
            (int)npc.Center.X + side * (int)PairSpawnDistance,
            (int)npc.Bottom.Y,
            partnerType);

        if (index < 0 || index >= Main.maxNPCs)
            return;

        _partner = index;
        Main.npc[index].GetGlobalNPC<WeddingCouple>()._partner = npc.whoAmI;
    }

    public override void OnKill(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || _partner < 0)
            return;

        NPC partner = Main.npc[_partner];

        if (!partner.active || (partner.type != NPCID.TheGroom && partner.type != NPCID.TheBride) || partner.whoAmI == npc.whoAmI)
            return;

        partner.GetGlobalNPC<WeddingCouple>().Enrage(partner);
    }

    private void Enrage(NPC npc)
    {
        if (_enraged)
            return;

        _enraged = true;

        int bonus = (int)(npc.lifeMax * HealthBonus);
        npc.lifeMax += bonus;
        npc.life = Math.Min(npc.life + bonus, npc.lifeMax);

        Vector2 bottom = npc.Bottom;
        npc.scale = RageScale;
        npc.width = (int)(npc.width * RageScale);
        npc.height = (int)(npc.height * RageScale);
        npc.defense = (int)(npc.defense + defenceScale);
        npc.Bottom = bottom;

        SummonZombies(npc);
        _summonTimer = Main.rand.Next(SummonIntervalMin, SummonIntervalMax + 1);

        if (!Main.dedServ)
        {
            SoundEngine.PlaySound(SoundID.Roar, npc.Center);

            for (int i = 0; i < 30; i++)
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
        }

        npc.netUpdate = true;
    }

    public override void PostAI(NPC npc)
    {
        if (!_enraged || Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (npc.velocity.Y == 0f && npc.velocity.X != 0f)
        {
            Vector2 extra = Collision.TileCollision(npc.position, new Vector2(npc.velocity.X * SpeedBonus, 0f), npc.width, npc.height);
            npc.position.X += extra.X;
        }

        if (--_summonTimer > 0)
            return;

        _summonTimer = Main.rand.Next(SummonIntervalMin, SummonIntervalMax + 1);
        SummonZombies(npc);
    }

    private static void SummonZombies(NPC npc)
    {
        int count = Main.rand.Next(ZombiesMin, ZombiesMax + 1);   // 3-7

        for (int i = 0; i < count; i++)
        {
            if (NPC.CountNPCS(NPCID.Zombie) >= MaxZombiesInWorld)
                break;

            Vector2 position = npc.Center + new Vector2(Main.rand.NextFloat(-150f, 150f), 0f);

            int index = NPC.NewNPC(npc.GetSource_FromAI(), (int)position.X, (int)npc.Bottom.Y, NPCID.Zombie);

            if (index >= 0 && index < Main.maxNPCs && !Main.dedServ)
            {
                for (int d = 0; d < 6; d++)
                    Dust.NewDust(Main.npc[index].position, Main.npc[index].width, Main.npc[index].height, DustID.Blood, 0f, -1f);
            }
        }
    }

    public override void DrawEffects(NPC npc, ref Color drawColor)
    {
        if (_enraged)
            drawColor = Color.Lerp(drawColor, Color.Red, RedTint);
    }
}