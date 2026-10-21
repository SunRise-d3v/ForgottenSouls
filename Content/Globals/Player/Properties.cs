namespace ForgottenSouls.Content;

internal sealed class Properties : ModPlayer
{
#if true
    public override void PreUpdateMovement()
        => Player.moveSpeed += 0.144f;

    // public override void PostUpdateRunSpeeds()
    // => Player.maxRunSpeed *= 1.06f;

    public override void PreUpdateBuffs()
    {
        Player.AddBuff(ModContent.BuffType<ObserverBuff>(), 60);
    }
#endif
}