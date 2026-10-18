namespace NoCompromises.Content.System;

internal sealed class FastSleep : ModSystem
{
#if true
    private const double TIMESPPED = 75.0;

    public override void PostUpdateWorld()
    {
        foreach (Player player in Main.player)
        {
            if (player.active && player.sleeping.isSleeping)
            {
                Main.time += TIMESPPED;

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

                break;
            }
        }
    }
#endif
}