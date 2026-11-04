namespace ForgottenSouls.Content.Globals;

internal static class BlueprintAccess
{
    public static IEnumerable<int> All => DrawingPlayer.DrawingRequiredItems;

    public static bool IsActivated(Player player, int itemType)
    {
        return player.GetModPlayer<DrawingPlayer>().IsUnlocked(itemType);
    }

    public static List<int> Available(Player player)
    {
        return [.. All
            .Distinct()
            .Where(type => !IsActivated(player, type))];
    }
}