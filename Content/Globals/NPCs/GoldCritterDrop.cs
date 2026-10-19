namespace ForgottenSouls.Content.NPCs;

internal class GoldCritterDrop : GlobalNPC
{
    public override void OnKill(NPC npc)
    {
        if (npc.type == NPCID.GoldBird ||
            npc.type == NPCID.GoldBunny ||
            npc.type == NPCID.GoldButterfly ||
            npc.type == NPCID.GoldDragonfly ||
            npc.type == NPCID.GoldFrog ||
            npc.type == NPCID.GoldGoldfish ||
            npc.type == NPCID.GoldGrasshopper ||
            npc.type == NPCID.GoldLadyBug ||
            npc.type == NPCID.GoldMouse ||
            npc.type == NPCID.GoldSeahorse ||
            npc.type == NPCID.SquirrelGold ||
            npc.type == NPCID.GoldWorm)
        {
            int amount = Main.rand.Next(10, 16);
            Item.NewItem(npc.GetSource_Loot(), npc.getRect(), ItemID.GoldCoin, amount);
        }
    }
}