namespace ForgottenSouls.Content.Projectiles;

internal sealed class FallingStarKill : GlobalProjectile
{
    private const int MinDamage = 666;

    private bool _natural;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.type == ProjectileID.FallingStar;

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (!Sadomasochist.Active)
            return;

        if (source is EntitySource_ItemUse or EntitySource_ItemUse_WithAmmo)
            return;

        _natural = true;
        projectile.friendly = false;
        projectile.hostile = true;
        projectile.damage = System.Math.Max(projectile.damage, MinDamage);
    }

    public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
    {
        if (!_natural || target.dead)
            return;

        target.KillMe(
            PlayerDeathReason.ByProjectile(-1, projectile.whoAmI),
            target.statLifeMax2 + 1000,
            info.HitDirection);
    }
}