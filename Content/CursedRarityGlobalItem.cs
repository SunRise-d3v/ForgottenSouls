using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

using ForgottenSouls.Content.Rarities;
using ForgottenSouls.Content.Systems;

namespace ForgottenSouls.Content.GlobalItems;

public class CursedRarityGlobalItem : GlobalItem
{
    private static readonly Color BrightColor = new(255, 35, 65);
    private static readonly Color DarkColor = new(75, 0, 45);

    public override bool PreDrawTooltipLine(
        Item item,
        DrawableTooltipLine line,
        ref int yOffset)
    {
        if (item.rare != ModContent.RarityType<CursedRarity>() ||
            line.Name != "ItemName")
            return true;

        if (CursedRaritySystem.Shader == null)
            return true;

        SpriteBatch spriteBatch = Main.spriteBatch;

        Effect shader = CursedRaritySystem.Shader.Value;

        shader.Parameters["uTime"].SetValue(
            (float)Main.GlobalTimeWrappedHourly
        );

        shader.Parameters["uIntensity"].SetValue(1f);

        float width = line.Font.MeasureString(line.Text).X;
        float x = line.X;

        spriteBatch.End();

        spriteBatch.Begin(
            SpriteSortMode.Immediate,
            BlendState.AlphaBlend,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise,
            shader,
            Main.UIScaleMatrix
        );

        for (int i = 0; i < line.Text.Length; i++)
        {
            string character = line.Text[i].ToString();

            float progress = width > 0f
                ? (x - line.X) / width
                : 0f;

            Color color = Color.Lerp(
                BrightColor,
                DarkColor,
                progress
            );

            Utils.DrawBorderString(
                spriteBatch,
                character,
                new Vector2(x, line.Y),
                color
            );

            x += line.Font.MeasureString(character).X;
        }

        spriteBatch.End();

        // Возвращаем стандартный SpriteBatch Terraria.
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise,
            null,
            Main.UIScaleMatrix
        );

        return false;
    }
}