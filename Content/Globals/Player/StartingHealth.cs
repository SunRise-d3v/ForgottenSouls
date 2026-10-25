namespace ForgottenSouls.Content;

internal class StartingHealth : ModPlayer
{
    public const int HealthPenalty = 50;

    public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
    {
        float progress = MathHelper.Clamp((Player.statLifeMax - 100f) / 300f, 0f, 1f);
        int penalty = (int)Math.Round(HealthPenalty * (1f - progress));

        health = new StatModifier(1f, 1f, -penalty);
        mana = StatModifier.Default;
    }
}