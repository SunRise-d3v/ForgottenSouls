
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using ForgottenSouls.Content.Rarities;

namespace ForgottenSouls.Content.GlobalItems;

public class BloodyRarityGlobalItem : GlobalItem
{
    private static readonly Color BrightRed = new(255, 35, 35);
    private static readonly Color DarkRed = new(65, 0, 15);

    public override bool PreDrawTooltipLine(
        Item item,
        DrawableTooltipLine line,
        ref int yOffset)
    {
        if (item.rare != ModContent.RarityType<BloodyRarity>() ||
            line.Name != "ItemName")
            return true;

        SpriteBatch spriteBatch = Main.spriteBatch;

        float time = Main.GlobalTimeWrappedHourly;

        float x = line.X;
        float width = line.Font.MeasureString(line.Text).X;

        // Рисуем буквы с градиентом.
        for (int i = 0; i < line.Text.Length; i++)
        {
            string character = line.Text[i].ToString();

            float progress = width > 0f ? x - line.X : 0f;
            progress /= width > 0f ? width : 1f;

            float wave = (float)System.Math.Sin(time * 1.5f + progress * 5f);
            float gradient = MathHelper.Clamp(progress + wave * 0.08f, 0f, 1f);

            Color color = Color.Lerp(BrightRed, DarkRed, gradient);

            Vector2 position = new(x, line.Y);

            // Небольшая тёмная тень.
            Utils.DrawBorderString(
                spriteBatch,
                character,
                position + new Vector2(1f, 1f),
                Color.DarkRed * 0.8f
            );

            Utils.DrawBorderString(
                spriteBatch,
                character,
                position,
                color
            );

            x += line.Font.MeasureString(character).X;
        }

        return false;
    }
}
