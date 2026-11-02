using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

using ForgottenSouls.Content.Rarities;
using ForgottenSouls.Content.Systems;

namespace ForgottenSouls.Content.GlobalItems;

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

    public override bool PreDrawTooltipLine(
    Item item,
    DrawableTooltipLine line,
    ref int yOffset)
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