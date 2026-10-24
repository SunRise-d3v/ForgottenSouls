namespace ForgottenSouls.Content.Globals;

internal class InstantDrowningPlayer : ModPlayer
{
    public override void PostUpdate()
    {
        if (Player.dead || Player.breath > 0)
            return;

        Player.KillMe(PlayerDeathReason.ByOther(1), Player.statLifeMax2 + 1000, 0);
    }
}