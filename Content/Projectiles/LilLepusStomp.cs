namespace ForgottenSouls.Content.Projectiles;

internal class LilLepusStomp : ModProjectile
{
    public override string Texture => Helper.TextureLoad("NPCs/Lepus", "LilLepusStomp");
    
    public override void SetDefaults()
    {
        Projectile.CloneDefaults(683);
        AIType = ProjectileID.DD2OgreSmash;
        Projectile.width = 70;
        Projectile.height = 20;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.timeLeft = 400;
    }
}