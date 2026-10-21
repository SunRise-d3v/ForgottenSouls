namespace ForgottenSouls.Content.WorldGeneration;

internal sealed class House : ModSystem
{
#if false
    private const int MinDistance = 30;
    private const int MaxDistance = 45;

    private const int HouseWidth = 9;
    private const int HouseHeight = 6;

    public override void PostWorldGen()
    {
        int spawnX = Main.spawnTileX;
        int spawnY = Main.spawnTileY;

        int direction = -1;

        int statueType = -1;

        /*foreach (ModTile modTile in TileLoader.tile)
        {
            if (modTile == null)
                continue;

            if (modTile.Name == "MutantStatue")
            {
                statueType = modTile.Type;
                break;
            }
        }*/

        if (statueType != -1)
        {
            for (int x = 10; x < Main.maxTilesX - 10; x++)
            {
                for (int y = 10; y < Main.maxTilesY - 10; y++)
                {
                    Tile tile = Main.tile[x, y];

                    if (!tile.HasTile ||
                        tile.TileType != statueType)
                        continue;

                    if (x < spawnX)
                        direction = 1;
                    else
                        direction = -1;

                    goto StatueFound;
                }
            }
        }

    StatueFound:

        int distance = WorldGen.genRand.Next(
            MinDistance,
            MaxDistance + 1);

        int houseX = spawnX + direction * distance;

        if (!WorldGen.InWorld(houseX, spawnY, HouseWidth + 5))
            return;

        int groundY = spawnY;

        for (int y = spawnY - 10; y < spawnY + 20; y++)
        {
            if (!WorldGen.InWorld(houseX, y, HouseWidth))
                continue;

            Tile tile = Main.tile[houseX, y];

            if (tile.HasTile &&
                Main.tileSolid[tile.TileType] &&
                !Main.tileSolidTop[tile.TileType])
            {
                groundY = y;
                break;
            }
        }

        if (!WorldGen.InWorld(
            houseX - HouseWidth / 2,
            groundY - HouseHeight - 2,
            HouseWidth))
            return;

        BuildHouse(
            houseX - HouseWidth / 2,
            groundY - HouseHeight,
            groundY);
    }

    private static void BuildHouse(
        int left,
        int top,
        int groundY)
    {
        int right = left + HouseWidth - 1;

        for (int x = left; x <= right; x++)
        {
            for (int y = top; y < groundY; y++)
            {
                WorldGen.KillTile(x, y, noItem: true);

                Main.tile[x, y].ClearTile();
                Main.tile[x, y].WallType = WallID.Wood;
            }
        }

        for (int x = left; x <= right; x++)
        {
            WorldGen.PlaceTile(
                x, top, TileID.WoodBlock,
                mute: true, forced: true);

            WorldGen.PlaceTile(
                x, groundY - 1, TileID.WoodBlock,
                mute: true, forced: true);
        }

        for (int y = top; y < groundY; y++)
        {
            WorldGen.PlaceTile(
                left, y, TileID.WoodBlock,
                mute: true, forced: true);

            WorldGen.PlaceTile(
                right, y, TileID.WoodBlock,
                mute: true, forced: true);
        }

        WorldGen.KillTile(left, groundY - 2, noItem: true);
        WorldGen.KillTile(left, groundY - 3, noItem: true);

        WorldGen.PlaceTile(
            left + HouseWidth / 2,
            top + 2,
            TileID.Torches,
            mute: true,
            forced: true);

        WorldGen.RangeFrame(
            left - 1,
            top - 1,
            right + 1,
            groundY);
    }
#endif
}