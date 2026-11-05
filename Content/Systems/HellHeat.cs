namespace DisasterMod.Content.Systems;

internal sealed class HellHeat : ModSystem
{
    private const string FilterKey = "DisasterMod:HellHeat";

    private const float MaxIntensity = 0.5f;
    private const float FadeSpeed = 0.04f;

    private static float _intensity;
    private static bool _filterActive;

    public override void Load()
    {
        if (Main.dedServ)
            return;

        Asset<Effect> shader = Mod.Assets.Request<Effect>(
            "Assets/Shaders/.xnb/HellHeatDistortion",
            AssetRequestMode.ImmediateLoad
        );

        ScreenShaderData shaderData = new(
            shader, "HellHeatPass"
        );

        Filters.Scene[FilterKey] = new Filter(
            shaderData, EffectPriority.Medium
        );
    }

    public override void PostUpdateEverything()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        bool inHell =
            !Main.gameMenu &&
            Main.LocalPlayer.active &&
            Main.LocalPlayer.ZoneUnderworldHeight;

        float targetIntensity = inHell ? MaxIntensity : 0f;

        _intensity = MathHelper.Lerp(
            _intensity,
            targetIntensity,
            FadeSpeed
        );

        if (Math.Abs(_intensity - targetIntensity) < 0.001f)
            _intensity = targetIntensity;

        if (_intensity > 0f)
        {
            if (!Filters.Scene[FilterKey].IsActive())
                Filters.Scene.Activate(FilterKey);

            Filters.Scene[FilterKey].GetShader().UseIntensity(_intensity);
        }
        else if (Filters.Scene[FilterKey].IsActive())
        {
            Filters.Scene.Deactivate(FilterKey);
        }
    }

    public override void OnWorldUnload()
    {
        _intensity = 0f;

        if (Main.netMode != NetmodeID.Server && _filterActive)
        {
            Filters.Scene.Deactivate(FilterKey);
            _filterActive = false;
        }
    }

    public override void Unload()
    {
        _intensity = 0f;

        if (!Main.dedServ)
            Filters.Scene.Deactivate(FilterKey);
    }
}