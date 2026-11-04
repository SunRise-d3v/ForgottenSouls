namespace DisasterMod;

internal sealed class DisasterConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(3f)]
    [Range(1f, 15f)]
    [Increment(1f)]
    [Slider]
    public float RespawnSeconds;
}