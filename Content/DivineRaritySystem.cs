using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria;
using Terraria.ModLoader;

namespace ForgottenSouls.Content.Systems;

internal class DivineRaritySystem : ModSystem
{
    internal static Asset<Effect> Shader;

    internal static RenderTarget2D RenderTarget;

    internal static void EnsureRenderTarget(int width, int height)
    {
        GraphicsDevice device = Main.graphics.GraphicsDevice;

        if (RenderTarget != null &&
            !RenderTarget.IsDisposed &&
            RenderTarget.Width == width &&
            RenderTarget.Height == height)
            return;

        RenderTarget2D oldTarget = RenderTarget;

        RenderTarget = new RenderTarget2D(
            device,
            width,
            height,
            false,
            SurfaceFormat.Color,
            DepthFormat.None
        );

        if (oldTarget != null)
        {
            Main.QueueMainThreadAction(() =>
            {
                if (!oldTarget.IsDisposed)
                    oldTarget.Dispose();
            });
        }
    }

    public override void Load()
    {
        if (Main.dedServ)
            return;

        Shader = Mod.Assets.Request<Effect>(
            "Assets/Shaders/.xnb/DivineRarity",
            AssetRequestMode.ImmediateLoad
        );
    }

    public override void Unload()
    {
        Shader = null;

        RenderTarget2D target = RenderTarget;
        RenderTarget = null;

        if (target != null)
        {
            Main.QueueMainThreadAction(() =>
            {
                if (!target.IsDisposed)
                    target.Dispose();
            });
        }
    }
}