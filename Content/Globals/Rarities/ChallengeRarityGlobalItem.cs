namespace ForgottenSouls.Content.Globals;

public class ChallengeRarityGlobalItem : GlobalItem
{
    private const int Padding = 8;

    private static Asset<Effect> Shader;

    public override void Load()
    {
        if (Main.dedServ)
            return;

        Shader = Mod.Assets.Request<Effect>(
            "Assets/Shaders/.xnb/ChallengeRarity",
            AssetRequestMode.ImmediateLoad
        );
    }

    public override void Unload()
    {
        Shader = null;
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if (item.rare != ModContent.RarityType<ChallengeRarity>() ||
            line.Name != "ItemName")
            return true;

        if (Shader == null)
            return true;

        RarityShaderRenderer.Draw(line, Shader.Value);

        return false;
    }
}