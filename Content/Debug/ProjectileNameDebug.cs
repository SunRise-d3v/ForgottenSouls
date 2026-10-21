namespace ForgottenSouls.Debug;

internal sealed class ProjectileNameDebug : GlobalProjectile
{
#if DEBUG
    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (projectile.ModProjectile != null)
            Main.NewText($"{projectile.ModProjectile.Mod.Name}/{projectile.ModProjectile.Name}");
    }
#endif
}