namespace ForgottenSouls.Content.Recipes;

internal sealed partial class CustomRecipes
{
    private static readonly CustomRecipe[] Recipes =
    [
        new(ItemID.NightsEdge,
            [(ItemID.Muramasa, 1),
            (ItemID.BladeofGrass, 1),
            (ItemID.FieryGreatsword, 1)],
            [(CustomRecipeGroups.EvilSwordGroup, 1)],
            TileID.DemonAltar)
    ];
}