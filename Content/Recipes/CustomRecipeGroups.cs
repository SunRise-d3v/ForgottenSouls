namespace ForgottenSouls.Content.Recipes;

internal sealed class CustomRecipeGroups : ModSystem
{
    public const string EvilSwordGroup = "EvilSwordGroup";

    public override void AddRecipeGroups()
    {
        RecipeGroup.RegisterGroup(EvilSwordGroup,
            new RecipeGroup(
                () => Language.GetTextValue("Mods.ForgottenSouls.RecipeGroups.EvilSword"),
                ItemID.LightsBane,
                ItemID.BloodButcherer
            )
        );
    }
}