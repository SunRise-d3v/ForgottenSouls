
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using Terraria.ModLoader;

namespace ForgottenSouls.Content.Systems;

internal class CursedRaritySystem : ModSystem
{
    internal static Asset<Effect> Shader;

    public override void Load()
    {
        if (Main.dedServ)
            return;

        Shader = Mod.Assets.Request<Effect>(
            "Assets/Shaders/.xnb/CursedRarity",
            AssetRequestMode.ImmediateLoad
        );
    }

    public override void Unload()
    {
        Shader = null;
    }
}