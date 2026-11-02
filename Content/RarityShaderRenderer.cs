namespace ForgottenSouls.Content.Rarities;

internal static class RarityShaderRenderer
{
    internal static bool Draw(
        DrawableTooltipLine line,
        Effect shader)
    {
        SpriteBatch spriteBatch = Main.spriteBatch;

        spriteBatch.End();

        try
        {
            shader.Parameters["uTime"].SetValue(
                (float)Main.GlobalTimeWrappedHourly
            );

            shader.Parameters["uIntensity"].SetValue(1.5f);

            spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                shader,
                Main.UIScaleMatrix
            );

            Utils.DrawBorderString(
                spriteBatch,
                line.Text,
                new Vector2(line.X, line.Y),
                Color.White
            );

            spriteBatch.End();
        }
        finally
        {
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                Main.UIScaleMatrix
            );
        }

        return false;
    }
}