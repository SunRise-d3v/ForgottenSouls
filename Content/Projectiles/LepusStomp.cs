namespace ForgottenSouls.Content.Projectiles;

internal sealed class LepusStomp : ModProjectile
{
    public override string Texture => Helper.TextureLoad("NPCs/Lepus", "LepusStomp");
    public override void SetDefaults()
    {
        Projectile.CloneDefaults(683);

        AIType = ProjectileID.DD2OgreSmash;
        Projectile.width = 100;
        Projectile.height = 20;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.timeLeft = 500;
    }
}