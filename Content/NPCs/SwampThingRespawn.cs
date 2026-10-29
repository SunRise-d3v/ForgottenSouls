namespace ForgottenSouls.Content.NPCs;

internal sealed class SwampThingRespawn : GlobalNPC
{
    private const int MaxRespawns = 1;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => entity.type == NPCID.SwampThing;

    private int respawns;

    public override void OnKill(NPC npc)
    {
        if (!Sadomasochist.Active)
            return;

        if (Main.netMode == NetmodeID.MultiplayerClient || respawns >= MaxRespawns)
            return;

        int index = NPC.NewNPC(
            npc.GetSource_Death(),
            (int)npc.Center.X,
            (int)npc.Bottom.Y,
            NPCID.SwampThing);

        if (index < 0 || index >= Main.maxNPCs)
            return;

        NPC reborn = Main.npc[index];
        reborn.target = npc.target;
        reborn.GetGlobalNPC<SwampThingRespawn>().respawns = respawns + 1;
        reborn.netUpdate = true;

        for (int i = 0; i < 20; i++)
            Dust.NewDust(reborn.position, reborn.width, reborn.height, DustID.GreenTorch, 0f, -2f, 100, default, 1.4f);
    }
}