namespace ForgottenSouls.Content.ModPlayers;

internal sealed class Properties : ModPlayer
{
#if true
    public override void PreUpdateMovement()
    {
        if (!Sadomasochist.Active)
            return;

        Player.moveSpeed += 0.125f;
    }
#endif
}