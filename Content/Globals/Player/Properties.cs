namespace NoCompromises.Content.Global;

internal sealed class Properties : ModPlayer
{
#if true
    public override void PreUpdateMovement()
        => Player.moveSpeed += 0.144f;

    public override void PostUpdateRunSpeeds()
        => Player.maxRunSpeed *= 1.06f;

    /*public override void ResetEffects()
        => Player.pickSpeed = 0.155f;*/
#endif
}