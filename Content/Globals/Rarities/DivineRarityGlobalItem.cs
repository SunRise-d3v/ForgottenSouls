namespace ForgottenSouls.Content.Globals;

public class DivineRarityGlobalItem : GlobalItem
{
    public override bool PreDrawTooltipLine(
    Item item,
    DrawableTooltipLine line,
    ref int yOffset)
    {
        if (item.rare != ModContent.RarityType<DivineRarity>() ||
            line.Name != "ItemName")
            return true;

        if (DivineRaritySystem.Shader == null)
            return true;

        RarityShaderRenderer.Draw(line, DivineRaritySystem.Shader.Value);

        return false;
    }
}