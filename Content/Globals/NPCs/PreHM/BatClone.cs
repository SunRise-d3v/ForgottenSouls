namespace ForgottenSouls.Content.NPCs;

internal sealed class BatClone : GlobalNPC
{
    private const int CloneCooldownTicks = 120;
    private const int MaxBats = 20;
    private const float BlindChance = 0.20f;
    private const int BlindTicks = 3 * 60;

    private int _cloneCooldown;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => Pool.Bats.Contains(npc.type);

    public override void PostAI(NPC npc)
    {
        if (_cloneCooldown > 0)
            _cloneCooldown--;
    }

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
    {
        if (!Sadomasochist.Active)
            return;

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

            if (npc.active && Pool.Bats.Contains(npc.type))
                count++;
        }

        return count;
    }
}