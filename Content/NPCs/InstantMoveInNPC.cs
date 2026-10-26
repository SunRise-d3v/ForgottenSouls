namespace ForgottenSouls.Content.Mechanics;

internal sealed class InstantMoveInNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => lateInstantiation && entity.townNPC;

    private int lastHomeX = -2;
    private int lastHomeY = -2;

    public override void PostAI(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (lastHomeX == -2)
        {
            lastHomeX = npc.homeTileX;
            lastHomeY = npc.homeTileY;
            return;
        }

        if (npc.homeTileX > 0 && npc.homeTileY > 0 && (npc.homeTileX != lastHomeX || npc.homeTileY != lastHomeY))
            MoveInstantly(npc);

        lastHomeX = npc.homeTileX;
        lastHomeY = npc.homeTileY;
    }

    private static void MoveInstantly(NPC npc)
    {
        Vector2 oldCenter = npc.Center;

        npc.position = new Vector2(npc.homeTileX * 16f + 8f - npc.width / 2f, npc.homeTileY * 16f - npc.height);
        npc.velocity = Vector2.Zero;

        for (int i = 0; i < 4 && Collision.SolidCollision(npc.position, npc.width, npc.height); i++)
            npc.position.Y -= 16f;

        npc.netUpdate = true;
        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);

        if (!Main.dedServ)
        {
            SpawnPuff(oldCenter);
            SpawnPuff(npc.Center);
        }
    }

    private static void SpawnPuff(Vector2 center)
    {
        for (int i = 0; i < 15; i++)
        {
            Dust d = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(12f, 16f), DustID.Smoke, Main.rand.NextVector2Circular(2f, 2f), 100, default, 1.3f);
            d.noGravity = true;
        }
    }
}