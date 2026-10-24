namespace ForgottenSouls.Content;

internal class HardmodeBlizzard : ModPlayer
{
    private bool forcing;
    private bool savedRaining;
    private int savedRainTime;
    private float savedMaxRaining;
    private float savedWindTarget;

    public override void PostUpdate()
    {
        if (Main.dedServ || Player.whoAmI != Main.myPlayer)
            return;

        bool wantBlizzard = Main.hardMode && Player.ZoneSnow;

        if (wantBlizzard)
        {
            if (!forcing)
            {
                savedRaining = Main.raining;
                savedRainTime = (int)Main.rainTime;
                savedMaxRaining = Main.maxRaining;
                savedWindTarget = Main.windSpeedTarget;
                forcing = true;
            }

            Main.raining = true;
            Main.rainTime = Math.Max(Main.rainTime, 600);
            Main.maxRaining = 0.95f;

            float dir = Main.windSpeedTarget < 0f ? -1f : 1f;
            Main.windSpeedTarget = 0.9f * dir;
        }
        else if (forcing)
        {
            forcing = false;
            Main.raining = savedRaining;
            Main.rainTime = savedRainTime;
            Main.maxRaining = savedMaxRaining;
            Main.windSpeedTarget = savedWindTarget;
        }
    }
}