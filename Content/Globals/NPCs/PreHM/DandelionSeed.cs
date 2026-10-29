namespace ForgottenSouls.Content.Projectiles;

internal sealed class DandelionSeed : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    private int _homingTimer;

    private const int HomingDuration = 45;
    private const float HomingSpeed = 5f;
    private const float TurnSpeed = 0.15f;

    public override void AI(Projectile projectile)
    {
        if (!Sadomasochist.Active)
            return;

        if (projectile.type != ProjectileID.DandelionSeed)
            return;

        if (_homingTimer >= HomingDuration)
            return;

        Player target = Main.player[Player.FindClosest(
            projectile.position,
            projectile.width,
            projectile.height)];

        if (target.active && !target.dead)
        {
            Vector2 direction = target.Center - projectile.Center;

            if (direction != Vector2.Zero)
            {
                direction.Normalize();

                Vector2 desiredVelocity = direction * HomingSpeed;

                projectile.velocity = Vector2.Lerp(
                    projectile.velocity,
                    desiredVelocity,
                    TurnSpeed);
            }
        }

        _homingTimer++;
    }
}