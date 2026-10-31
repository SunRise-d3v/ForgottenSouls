namespace ForgottenSouls.Content.Recipes;

internal sealed partial class CustomRecipes
{
    private static readonly CustomRecipe[] Recipes =
    [
        new(ItemID.NightsEdge)
        {
            Items =
            [
                (ModContent.ItemType<DeviatingEnergy>(), 5),
                (ItemID.Muramasa, 1),
                (ItemID.BladeofGrass, 1),
                (ItemID.FieryGreatsword, 1)
            ],
            Groups = [(CustomRecipeGroups.EvilSwordGroup, 1)],
            Tiles = [TileID.DemonAltar]
        },
    ];
}