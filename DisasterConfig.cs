namespace DisasterMod;

internal sealed class DisasterConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(3f)]
    [Range(1f, 15f)]
    [Increment(1f)]
    [Slider]
    public float RespawnSeconds;

    [DefaultValue(true)]
    public bool EnableDeathEffect;

    [DefaultValue(true)]
    public bool EnableEnterWorldEffect;

    [DefaultValue(75)]
    [Range(1, 300)]
    [Increment(5)]
    [Slider]
    public int SleepSpeed;
}