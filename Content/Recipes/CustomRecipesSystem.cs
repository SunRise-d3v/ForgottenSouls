namespace ForgottenSouls.Content;

internal record CustomRecipe(int Result, (int Type, int Stack)[] Items, (string Group, int Stack)[] Groups, int Tile);
internal partial class CustomRecipes : ModSystem
{
    public override void AddRecipes()
    {
        foreach (CustomRecipe def in Recipes)
        {
            Recipe recipe = Recipe.Create(def.Result);

            foreach (var (type, stack) in def.Items)
                recipe.AddIngredient(type, stack);

            foreach (var (group, stack) in def.Groups)
                recipe.AddRecipeGroup(group, stack);

            recipe.AddTile(def.Tile);
            recipe.Register();
        }
    }

    public override void PostAddRecipes()
    {
        HashSet<int> results = [.. Recipes.Select(r => r.Result)];

        foreach (Recipe recipe in Main.recipe)
        {
            if (recipe.Mod != Mod && results.Contains(recipe.createItem.type))
                recipe.DisableRecipe();
        }
    }
}