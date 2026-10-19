namespace NoCompromises.Content;

internal class StartingInventory : ModPlayer
{
    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        if (mediumCoreDeath)
            yield break;
        else
        {
            if (Main.masterMode == false)
            {
                Item starterBag = new();
                starterBag.SetDefaults(ModContent.ItemType<StarterBag>(), false);
                yield return starterBag;
            }
            else
                yield break;
        }
    }

    public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath)
        => itemsByMod["Terraria"].Clear();
}