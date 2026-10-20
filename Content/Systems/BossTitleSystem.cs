namespace ForgottenSouls.Content.Systems;

internal class BossTitleSystem : ModSystem
{
    private static string bossName = "";
    private static string bossSubtitle = "";

    private static float displayTimer = 0f;
    private const float DisplayTime = 180f;
    private static Color titleColor = Color.White;
    private static Color shadowColor = Color.Black;

    public static void ShowBossTitle(string name, string subtitle, Color? mainColor = null, Color? shadow = null)
    {
        bossName = name;
        bossSubtitle = subtitle;
        displayTimer = DisplayTime;
        titleColor = mainColor ?? Color.Lerp(Color.Red, Color.OrangeRed, 0.5f);
        shadowColor = shadow ?? Color.Black;
    }

    public override void PostUpdateEverything()
    {
        if (displayTimer > 0)
            displayTimer--;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
        if (index != -1)
        {
            layers.Insert(index, new LegacyGameInterfaceLayer(
                "ForgottenSouls: Boss Title",
                delegate
                {
                    if (displayTimer > 0)
                        DrawBossTitle();

                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }

    private static void DrawBossTitle()
    {
        float progress = displayTimer / DisplayTime;
        float baseScale = 1.7f;
        float opacity = MathHelper.Clamp(progress * 3f, 0f, 1f);

        Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
        string mainText = bossName.ToUpper();
        string subtitleText = bossSubtitle;

        DynamicSpriteFont font = Terraria.GameContent.FontAssets.DeathText.Value;
        float subtitleScale = baseScale * 0.6f;

        Vector2 mainSize = font.MeasureString(mainText) * baseScale;
        Vector2 subtitleSize = font.MeasureString(subtitleText) * subtitleScale;

        Vector2 mainPos = screenCenter - mainSize * 0.5f;
        Vector2 subtitlePos = mainPos - new Vector2(0, subtitleSize.Y + 5f);

        int appearPerTick = 3;
        int maxLetters = (int)((DisplayTime - displayTimer) / appearPerTick);

        if (!string.IsNullOrEmpty(subtitleText))
        {
            Color finalColor = titleColor * opacity * 0.6f;
            Color finalShadow = shadowColor * opacity * 0.3f;

            Vector2 pos = subtitlePos;
            Vector2 size = font.MeasureString(subtitleText);

            pos.X = screenCenter.X - size.X * 0.5f * subtitleScale;

            for (int k = 0; k < 4; k++)
            {
                Vector2 offset = Vector2.UnitX.RotatedBy(k * MathHelper.PiOver2) * 2f;
                Main.spriteBatch.DrawString(
                    font,
                    subtitleText,
                    pos + offset,
                    finalShadow,
                    0f,
                    Vector2.Zero,
                    subtitleScale,
                    SpriteEffects.None,
                    0f
                );
            }

            Main.spriteBatch.DrawString(
                font,
                subtitleText,
                pos,
                finalColor,
                0f,
                Vector2.Zero,
                subtitleScale,
                SpriteEffects.None,
                0f
            );
        }

        for (int i = 0; i < mainText.Length; i++)
        {
            string letterStr = mainText[i].ToString();
            _ = font.MeasureString(letterStr);
            Vector2 letterPos = mainPos;

            for (int j = 0; j < i; j++)
                letterPos.X += font.MeasureString(mainText[j].ToString()).X * baseScale;

            float letterOpacity = MathHelper.Clamp((maxLetters - i) / 3f, 0f, 1f);
            float letterScale = baseScale * (0.8f + 0.2f * letterOpacity);

            Color finalColor = titleColor * opacity * letterOpacity;
            Color finalShadow = shadowColor * opacity * letterOpacity * 0.5f;

            for (int k = 0; k < 4; k++)
            {
                Vector2 offset = Vector2.UnitX.RotatedBy(k * MathHelper.PiOver2) * 2f;
                Main.spriteBatch.DrawString(
                    font,
                    letterStr,
                    letterPos + offset,
                    finalShadow,
                    0f,
                    Vector2.Zero,
                    letterScale,
                    SpriteEffects.None,
                    0f
                );
            }

            Main.spriteBatch.DrawString(
                font,
                letterStr,
                letterPos,
                finalColor,
                0f,
                Vector2.Zero,
                letterScale,
                SpriteEffects.None,
                0f
            );
        }
    }
}