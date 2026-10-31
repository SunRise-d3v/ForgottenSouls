namespace ForgottenSouls.Content.Recipes;

internal sealed partial class CustomRecipes
{
    private static readonly CustomRecipe[] Recipes =
    [
        new(ItemID.NightsEdge,
            [(ModContent.ItemType<DeviatingEnergy>(), 5),
            (ItemID.Muramasa, 1),
            (ItemID.BladeofGrass, 1),
            (ItemID.FieryGreatsword, 1)],
            [(CustomRecipeGroups.EvilSwordGroup, 1)],
            TileID.DemonAltar)
    ];
}