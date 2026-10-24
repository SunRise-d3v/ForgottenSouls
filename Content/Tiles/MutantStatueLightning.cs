namespace ForgottenSouls.Content;

internal sealed class MutantStatueLightning : GlobalItem
{
    private const int LightningDamage = 60;
    private const float StartHeight = 100f;
    private const float StartSpeed = 12f;

    public override bool OnPickup(Item item, Player player)
    {
        if (item.type == ModContent.ItemType<Masochist>() && player.whoAmI == Main.myPlayer)
            StrikePlayer(player);

        return true;
    }

    private static void StrikePlayer(Player player)
    {
        Vector2 start = player.Center - new Vector2(0f, StartHeight);

        Projectile.NewProjectile(
            player.GetSource_Misc("MutantStatueLightning"),
            start,
            Vector2.UnitY * StartSpeed,
            ModContent.ProjectileType<RainLightning>(),
            LightningDamage,
            0f,
            player.whoAmI,
            ai0: MathHelper.PiOver2,
            ai1: Main.rand.Next(100));
    }
}