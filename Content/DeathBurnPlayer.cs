namespace DisasterMod.Content;

internal sealed class DeathBurnPlayer : ModPlayer
{
    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        DeathBurnSystem.Start();
    }

    public override void OnRespawn()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        DeathBurnSystem.Reveal();
    }

    public override void OnEnterWorld()
        => DeathBurnSystem.Reveal();
}