namespace ForgottenSouls.Content.Globals;

internal sealed class FlightEssenceDrop : GlobalNPC
{
    public override void OnKill(NPC npc)
    {
        if (!Main.hardMode || Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (npc.type != NPCID.Harpy &&
            npc.type != NPCID.AngryNimbus)
            return;

        if (Main.rand.NextBool(5))
        {
            Item.NewItem(
                npc.GetSource_Loot(),
                npc.getRect(),
                ItemID.SoulofFlight,
                Main.rand.Next(1, 3)
            );
        }
    }
}