namespace ForgottenSouls.Content.Recipes;

internal sealed record CustomRecipe(int Result)
{
    public int ResultStack { get; init; } = 1;
    public (int Type, int Stack)[] Items { get; init; } = [];
    public (string Group, int Stack)[] Groups { get; init; } = [];
    public int[] Tiles { get; init; } = [];
}
internal sealed partial class CustomRecipes : ModSystem
{
    public override void AddRecipes()
    {
        foreach (CustomRecipe def in Recipes)
        {
            Recipe recipe = Recipe.Create(def.Result, def.ResultStack);

            foreach (var (type, stack) in def.Items)
                recipe.AddIngredient(type, stack);

            foreach (var (group, stack) in def.Groups)
                recipe.AddRecipeGroup(group, stack);

            foreach (int tile in def.Tiles)
                recipe.AddTile(tile);

            recipe.Register();
        }
    }

    public override void PostAddRecipes()
    {
        foreach (Recipe recipe in Main.recipe)
        {
            if (recipe.Mod != Mod && RecipeRemovals.Results.Contains(recipe.createItem.type))
                recipe.DisableRecipe();
        }
    }
}