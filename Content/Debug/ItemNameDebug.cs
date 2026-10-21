namespace ForgottenSouls.Debug;

internal sealed class ItemNameDebug : GlobalItem
{
#if DEBUG
    public override bool? UseItem(Item item, Player player)
    {
        if (player.whoAmI == Main.myPlayer)
            PrintName(item);

        return null;
    }

    public override void RightClick(Item item, Player player)
    {
        if (player.whoAmI == Main.myPlayer)
            PrintName(item);
    }

    private static void PrintName(Item item)
    {
        string text = item.ModItem != null
            ? $"{item.ModItem.Mod.Name}/{item.ModItem.Name}"
            : $"Terraria/{ItemID.Search.GetName(item.type)}";

        Main.NewText(text, Color.Yellow);
    }
#endif
}