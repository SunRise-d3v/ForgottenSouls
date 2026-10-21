namespace ForgottenSouls.Content.Tiles;

internal sealed class LifeCrystalTileExplosion : GlobalTile
{
    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (type != TileID.Heart || fail || effectOnly)
            return;

        noItem = true;

        Tile tile = Main.tile[i, j];

        if (tile.TileFrameX != 0 || tile.TileFrameY != 0)
            return;

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Vector2 spawnPosition = new(i * 16 + 16, j * 16 + 16);

            Projectile.NewProjectile(
                new EntitySource_TileBreak(i, j),
                spawnPosition,
                Vector2.Zero,
                ProjectileID.LifeCrystalBoulder,
                40,
                6f,
                Main.myPlayer
            );
        }
    }
}

internal sealed class LifeCrystalBoulderDropBlocker : GlobalProjectile
{
    [Obsolete]
    public override void OnKill(Projectile projectile, int timeLeft)
    {
        if (projectile.type == ProjectileID.LifeCrystalBoulder)
        {
            foreach (Item item in Main.item)
            {
                if (item != null && item.active && item.type == ItemID.LifeCrystal && item.timeSinceItemSpawned == 0)
                {
                    if (Vector2.Distance(projectile.Center, item.Center) < 50f)
                    {
                        item.TurnToAir();
                    }
                }
            }
        }
    }
}