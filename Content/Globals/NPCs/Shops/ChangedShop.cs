namespace NoCompromises.Content.NPCs.Shops;

internal abstract class ChangedShop : GlobalNPC
{
    protected abstract int NpcType { get; }

    public sealed override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
    {
        if (npc.type != NpcType)
            return;

        ModifyShop(npc, shopName, items);
    }

    protected abstract void ModifyShop(NPC npc, string shopName, Item[] items);

    protected static void AddItem(Item[] items, int itemID, int stack = 20, int priceInGold = 1)
    {
        int slot = FindFreeSlot(items);
        if (slot == -1)
            return;

        items[slot] = new Item();
        items[slot].SetDefaults(itemID);
        items[slot].stack = stack;
        items[slot].shopCustomPrice = Item.buyPrice(0, priceInGold);
    }

    private static int FindFreeSlot(Item[] items)
    {
        for (int i = 0; i < items.Length; i++)
            if (items[i] == null || items[i].IsAir)
                return i;
        return -1;
    }
}