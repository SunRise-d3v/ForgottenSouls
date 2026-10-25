namespace ForgottenSouls.Content;

internal sealed class MutantStatueLightning : GlobalItem
{
    private const int LightningDamage = 60;
    private const float StartHeight = 100f;       // на сколько выше статуи начинается молния
    private const float StartSpeed = 12f;
    private const int SearchRadius = 10;          // в каких пределах от игрока ищем статую (в блоках)

    // Внутренние имена тайлов статуи во Fargo: с подарком и пустая (после того как подарок забрали)
    private static readonly string[] StatueTiles = ["MutantStatueGift", "MutantStatue"];

    public override bool OnPickup(Item item, Player player)
    {
        if (item.type == ModContent.ItemType<Masochist>() && player.whoAmI == Main.myPlayer)
            StrikeStatue(player);

        return true;
    }

    private void StrikeStatue(Player player)
    {
        // Молния бьёт только если подарок подобрали у статуи (а не получили, например, с босса Мутанта)
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

    /// <summary>Ищет ближайшую статую Мутанта (3x3 тайла) рядом с игроком и возвращает её центр.</summary>
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

                // Верхний левый тайл статуи 3x3 по кадру текущего тайла, затем её центр
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
}