namespace ForgottenSouls.Content.Tiles;

internal sealed class MutantStatueLightning : GlobalItem
{
#if false
    private const int LightningDamage = 60;
    private const float StartHeight = 100f;   
    private const float StartSpeed = 12f;
    private const int SearchRadius = 10;     

    private static readonly string[] StatueTiles = ["MutantStatueGift", "MutantStatue"];

    public override bool OnPickup(Item item, Player player)
    {
        if (item.type == ModContent.ItemType<Masochist>() && player.whoAmI == Main.myPlayer)
            StrikeStatue(player);

        return true;
    }

    private void StrikeStatue(Player player)
    {
        if (!TryFindStatue(player, out Vector2 statueCenter))
            return;

        Vector2 start = statueCenter - new Vector2(0f, StartHeight);

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

    private bool TryFindStatue(Player player, out Vector2 center)
    {
        center = default;

        List<int> types = [];

        foreach (string name in StatueTiles)
        {
            if (ModContent.TryFind("FargowiltasSouls", name, out ModTile tile))
                types.Add(tile.Type);
            else
                Mod.Logger.Warn($"MutantStatueLightning: тайл FargowiltasSouls/{name} не найден");
        }

        Point origin = player.Center.ToTileCoordinates();
        float best = float.MaxValue;

        for (int x = origin.X - SearchRadius; x <= origin.X + SearchRadius; x++)
        {
            for (int y = origin.Y - SearchRadius; y <= origin.Y + SearchRadius; y++)
            {
                Tile tile = Framing.GetTileSafely(x, y);

                if (!tile.HasTile || !types.Contains(tile.TileType))
                    continue;

                int left = x - tile.TileFrameX % 54 / 18;
                int top = y - tile.TileFrameY % 54 / 18;
                Vector2 candidate = new((left + 1.5f) * 16f, (top + 1.5f) * 16f);

                float distance = Vector2.DistanceSquared(candidate, player.Center);

                if (distance < best)
                {
                    best = distance;
                    center = candidate;
                }
            }
        }

        return best < float.MaxValue;
    }
#endif
}