namespace ForgottenSouls.Content.Globals;

internal class BatClone : GlobalNPC
{
    private const int CloneCooldownTicks = 120;
    private const int MaxBats = 20;
    private const float BlindChance = 0.20f;
    private const int BlindTicks = 3 * 60;

    private static readonly HashSet<int> Bats =
    [
        NPCID.CaveBat,
        NPCID.GiantBat,
        NPCID.JungleBat,
        NPCID.IceBat,
        NPCID.Hellbat,
        NPCID.Lavabat,
        NPCID.SporeBat,
        NPCID.VampireBat,
        NPCID.IlluminantBat,
        NPCID.GiantFlyingFox
    ];

    private int _cloneCooldown;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => Bats.Contains(npc.type);

    public override void PostAI(NPC npc)
    {
        if (_cloneCooldown > 0)
            _cloneCooldown--;
    }

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
    {
        if (Main.rand.NextFloat() < BlindChance)
            target.AddBuff(BuffID.Obstructed, BlindTicks);

        if (_cloneCooldown > 0 || Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (CountBats() >= MaxBats)
            return;

        _cloneCooldown = CloneCooldownTicks;

        int index = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Bottom.Y, npc.type);

        if (index < 0 || index >= Main.maxNPCs)
            return;

        NPC clone = Main.npc[index];
        clone.SetDefaults(npc.netID);
        clone.life = npc.life;
        clone.Center = npc.Center;
        clone.velocity = Main.rand.NextVector2Circular(4f, 4f);
        clone.netUpdate = true;

        if (!Main.dedServ)
        {
            for (int i = 0; i < 10; i++)
                Dust.NewDust(clone.position, clone.width, clone.height, DustID.Smoke, 0f, -1f);
        }
    }

    private static int CountBats()
    {
        int count = 0;

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];

            if (npc.active && Bats.Contains(npc.type))
                count++;
        }

        return count;
    }
}