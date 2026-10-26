namespace ForgottenSouls.Content.Biomes;

internal sealed class SolarStrikeShader : ModSystem
{
#if false
    public const string FilterName = "ForgottenSouls:SolarStrike";

    public override void Load()
    {
        if (Main.dedServ)
            return;

        Asset<Effect> effect = Mod.Assets.Request<Effect>("Assets/Effects/SolarStrike", AssetRequestMode.ImmediateLoad);

        Filters.Scene[FilterName] = new Filter(
            new ScreenShaderData(effect, "SolarStrikePass")
                .UseColor(1f, 0.82f, 0.55f)         
                .UseSecondaryColor(1f, 0.97f, 0.85f), 
            EffectPriority.VeryHigh);

        Filters.Scene[FilterName].Load();
    }
#endif
}

internal sealed class SolarStrike : ModPlayer
{
#if false
    private const int WarningTicks = 3 * 60;      
    private const float WarningIntensity = 0.35f;  
    private const float FlashDecay = 1f / 150f;   

    private int sunStrikeTimer = 0;
    private int nextStrikeDelay = 0;
    private float flash;  
    private float warning; 

    public override void Initialize()
    {
        ResetTimer();
    }

    public override void PostUpdate()
    {
        if (Player.ZoneDesert && Main.dayTime && !Main.raining && !Player.HasBuff(BuffID.ObsidianSkin))
        {
            sunStrikeTimer++;

            int remaining = nextStrikeDelay * 60 - sunStrikeTimer;
            warning = remaining < WarningTicks ? (1f - remaining / (float)WarningTicks) * WarningIntensity : 0f;

            if (sunStrikeTimer >= nextStrikeDelay * 60)
            {
                int damage = Main.rand.Next(25, 55);
                Player.Hurt(PlayerDeathReason.ByCustomReason($"{Player.name} был поражён солнечным ударом."), damage, 0);

                flash = 1f;
                ResetTimer();
            }
        }
        else
        {
            sunStrikeTimer = 0;
            warning = 0f;
        }

        UpdateShader();
    }

    private void UpdateShader()
    {
        if (Main.dedServ || Player.whoAmI != Main.myPlayer)
            return;

        flash = Math.Max(0f, flash - FlashDecay);
        float intensity = Math.Max(flash, warning);

        if (intensity > 0.001f)
        {
            if (!Filters.Scene[SolarStrikeShader.FilterName].IsActive())
                Filters.Scene.Activate(SolarStrikeShader.FilterName);

            Filters.Scene[SolarStrikeShader.FilterName].GetShader().UseIntensity(intensity);
        }
        else if (Filters.Scene[SolarStrikeShader.FilterName].IsActive())
        {
            Filters.Scene.Deactivate(SolarStrikeShader.FilterName);
        }
    }

    private void ResetTimer()
    {
        sunStrikeTimer = 0;
        nextStrikeDelay = Main.rand.Next(30, 61);
    }
#endif
}