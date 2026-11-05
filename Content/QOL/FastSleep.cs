namespace DisasterMod.Content.QOL;

internal sealed class FastSleep : ModSystem
{
    private const int RampUpTicks = 200;
    private const int RampDownTicks = 45;

    private double _current;

    public override void OnWorldLoad() => _current = 0;
    public override void OnWorldUnload() => _current = 0;

    public override void PostUpdateWorld()
    {
        double target = ModContent.GetInstance<DisasterConfig>().SleepSpeed;
        bool anySleeping = false;

        foreach (Player player in Main.player)
        {
            if (player.active && player.sleeping.isSleeping)
            {
                anySleeping = true;
                break;
            }
        }

        if (anySleeping)
            _current = Math.Min(target, _current + target / RampUpTicks);
        else
            _current = Math.Max(0, _current - target / RampDownTicks);

        if (_current <= 0)
            return;

        Main.time += _current;

        if (Main.dayTime && Main.time >= Main.dayLength)
        {
            Main.time = 0;
            Main.dayTime = false;
        }
        else if (!Main.dayTime && Main.time >= Main.nightLength)
        {
            Main.time = 0;
            Main.dayTime = true;
        }
    }
}