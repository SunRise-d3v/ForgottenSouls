namespace ForgottenSouls.Content.NPCs;

internal sealed class SkeletonArcherMultishot : GlobalProjectile
{
    private const float SpreadDegrees = 8f;

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !projectile.hostile)
            return;

        if (source is not EntitySource_Parent { Entity: NPC npc } || npc.type != NPCID.SkeletonArcher)
            return;

        int extra = Main.rand.Next(1, 4) - 1;
        if (extra <= 0)
            return;

        float[] angles = extra == 1
            ? [(Main.rand.NextBool() ? 1f : -1f) * SpreadDegrees * 0.6f]
            : [-SpreadDegrees, SpreadDegrees];

        IEntitySource extraSource = projectile.GetSource_FromThis();

        foreach (float angle in angles)
        {
            Projectile.NewProjectile(
                extraSource,
                projectile.Center,
                projectile.velocity.RotatedBy(MathHelper.ToRadians(angle)),
                projectile.type,
                projectile.damage,
                projectile.knockBack,
                projectile.owner,
                projectile.ai[0],
                projectile.ai[1]);
        }
    }
}