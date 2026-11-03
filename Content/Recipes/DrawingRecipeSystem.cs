using ForgottenSouls.Content.Players;

namespace ForgottenSouls.Content.Systems;

internal sealed class DrawingRecipeSystem : ModSystem
{
    public override void PostAddRecipes()
    {
        foreach (Recipe recipe in Main.recipe)
        {
            int itemID = recipe.createItem.type;

            if (!DrawingPlayer.IsDrawingRequired(itemID))
                continue;

            recipe.AddCondition(DrawingRecipeCondition.Unlocked(itemID));
        }
    }
}