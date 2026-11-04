namespace ForgottenSouls.Content.Items;

internal static class DrawingRecipeCondition
{
    public static Condition Unlocked(int itemID)
    {
        return new Condition(
            $"Mods.ForgottenSouls.Conditions.DrawingUnlocked",
            () => Main.LocalPlayer.GetModPlayer<DrawingPlayer>().IsUnlocked(itemID)
        );
    }
}