namespace DisasterMod.Content.Items;

internal sealed class GunRecoil : GlobalItem
{
    private const float BaseRecoil = 2.2f;

    private const float VerticalFactor = 0.35f;

    private const float MaxSpeed = 9f;

    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return lateInstantiation && entity.useAmmo == AmmoID.Bullet && entity.damage > 0;
    }

	public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.whoAmI != Main.myPlayer)
            return true;

        if (player.grapCount > 0 || player.mount.Active)
            return true;

        Vector2 dir = velocity.SafeNormalize(Vector2.Zero);

        if (dir == Vector2.Zero)
            return true;

        float speedFactor = MathHelper.Clamp(item.useTime / 20f, 0.25f, 1.6f);
        float strength = BaseRecoil * speedFactor;

        Vector2 push = new(-dir.X * strength, -dir.Y * strength * VerticalFactor);

        if (player.velocity.Y == 0f)
            push.Y = 0f;

        Vector2 newVelocity = player.velocity + push;

        newVelocity.X = MathHelper.Clamp(newVelocity.X, -MaxSpeed, MaxSpeed);

        player.velocity = newVelocity;

        return true;
    }
}