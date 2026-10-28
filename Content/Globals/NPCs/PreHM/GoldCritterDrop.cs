namespace ForgottenSouls.Content.Globals;

internal sealed class GoldCritterDrop : GlobalNPC
{
    public override void OnKill(NPC npc)
    {
        if (!Pool.GoldCritters.Contains(npc.type))
            return;

        Item.NewItem(npc.GetSource_Loot(), npc.getRect(), ItemID.GoldCoin, Main.rand.Next(10, 15 + 1));
    }
}