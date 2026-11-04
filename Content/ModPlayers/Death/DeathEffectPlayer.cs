namespace DisasterMod.Content.ModPlayers;

internal sealed class DeathEffectPlayer : ModPlayer
{
    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        DeathEffect.Start();
    }

    public override void OnRespawn()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        DeathEffect.Reveal();
    }

    public override void OnEnterWorld()
        => DeathEffect.Reveal();
}