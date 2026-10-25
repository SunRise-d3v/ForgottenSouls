namespace ForgottenSouls.Content;

internal sealed class RegenerationNerf : ModPlayer
{
    private const float Reduction = 0.10f;
    private const int VanillaBonus = 4;   

    public override void UpdateLifeRegen()
    {
        if (!Player.HasBuff(BuffID.Regeneration) || Player.lifeRegen <= 0)
            return;

        if (Main.rand.NextFloat() < VanillaBonus * Reduction)
            Player.lifeRegen -= 1;
    }
}