namespace ForgottenSouls.Content;

internal class CustomRecipeGroups : ModSystem
{
    public const string EvilSwordGroup = "EvilSwordGroup";

    public override void AddRecipeGroups()
    {
        RecipeGroup.RegisterGroup(EvilSwordGroup,
            new RecipeGroup(() => "Any evil sword", ItemID.LightsBane, ItemID.BloodButcherer));
    }
}
