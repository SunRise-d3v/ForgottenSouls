namespace ForgottenSouls.Content.Biome;

internal class SolarStrike : ModPlayer
{
#if false
    private int sunStrikeTimer = 0;
    private int nextStrikeDelay = 0;

    public override void Initialize()
    {
        ResetTimer();
    }

    public override void PostUpdate()
    {
        if (Player.ZoneDesert && Main.dayTime && !Main.raining && !Player.HasBuff(BuffID.ObsidianSkin))
        {
            sunStrikeTimer++;

            if (sunStrikeTimer >= nextStrikeDelay * 60)
            {
                int damage = Main.rand.Next(25, 55);
                Player.Hurt(PlayerDeathReason.ByCustomReason($"{Player.name} был поражён солнечным ударом."), damage, 0);

                ResetTimer();
            }
        }
        else
            sunStrikeTimer = 0;
    }

    private void ResetTimer()
    {
        sunStrikeTimer = 0;
        nextStrikeDelay = Main.rand.Next(30, 61);
    }
#endif
}