namespace ForgottenSouls.Content.ModPlayers;

internal sealed class DrawingPlayer : ModPlayer
{
    private readonly HashSet<int> _unlockedItems = [];

    private static readonly HashSet<int> DrawingRequiredItems = [];

    public static void RequireDrawing(int itemID)
    {
        DrawingRequiredItems.Add(itemID);
    }

    public static bool IsDrawingRequired(int itemID)
    {
        return DrawingRequiredItems.Contains(itemID);
    }

    public bool IsUnlocked(int itemID)
    {
        return _unlockedItems.Contains(itemID);
    }

    public void Unlock(int itemID)
    {
        _unlockedItems.Add(itemID);
    }

    public override void SaveData(TagCompound tag)
    {
        tag["UnlockedItems"] = _unlockedItems.ToList();
    }

    public override void LoadData(TagCompound tag)
    {
        _unlockedItems.Clear();

        foreach (int itemID in tag.GetList<int>("UnlockedItems"))
            _unlockedItems.Add(itemID);
    }
}