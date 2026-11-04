namespace ForgottenSouls.Content.Globals;

internal static class BlueprintAccess
{
    public static IEnumerable<int> All =>
        ModDrawing.AllModDrawings.Select(drawing => drawing.Type);

    public static bool IsActivated(Player player, ModDrawing drawing)
    {
        return player.GetModPlayer<DrawingPlayer>()
            .IsUnlocked(drawing.UnlockItem);
    }

    public static List<int> Available(Player player)
    {
        return [.. ModDrawing.AllModDrawings
            .Where(drawing => !IsActivated(player, drawing))
            .Select(drawing => drawing.Type).Distinct()];
    }
}