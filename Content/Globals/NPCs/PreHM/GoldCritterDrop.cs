namespace ForgottenSouls.Content.Globals;

internal sealed class GoldCritterDrop : GlobalNPC
{
    private static readonly HashSet<int> GoldCritters =
    [
        NPCID.GoldBird,
        NPCID.GoldBunny,
        NPCID.GoldButterfly,
        NPCID.GoldDragonfly,
        NPCID.GoldFrog,
        NPCID.GoldGoldfish,
        NPCID.GoldGrasshopper,
        NPCID.GoldLadyBug,
        NPCID.GoldMouse,
        NPCID.GoldSeahorse,
        NPCID.SquirrelGold,
        NPCID.GoldWorm,
    ];

    public override void OnKill(NPC npc)
    {
        if (!GoldCritters.Contains(npc.type))
            return;

        Item.NewItem(npc.GetSource_Loot(), npc.getRect(), ItemID.GoldCoin, Main.rand.Next(10, 15 + 1));
    }
}