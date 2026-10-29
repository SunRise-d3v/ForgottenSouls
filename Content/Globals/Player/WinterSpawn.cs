namespace ForgottenSouls.Content;

internal class WinterSpawnSystem : ModSystem
{
    private const int FreeHeight = 4;
    private const int MaxGapInRun = 3;

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        int index = tasks.FindIndex(pass => pass.Name == "Spawn Point");

        if (index == -1)
            index = tasks.FindIndex(pass => pass.Name == "Final Cleanup");

        if (index == -1)
            index = tasks.Count - 1;
        else
            index++;

        tasks.Insert(index, new PassLegacy("Moving Spawn To Winter", MoveSpawnToSnow));
    }

    private static void MoveSpawnToSnow(GenerationProgress progress, GameConfiguration configuration)
    {
        int bestStart = -1, bestEnd = -1;
        int runStart = -1, lastSnow = -1;

        for (int x = 40; x < Main.maxTilesX - 40; x++)
        {
            bool snow = IsSnowGround(x);

            if (snow)
            {
                if (runStart == -1)
                    runStart = x;

                lastSnow = x;
            }

            bool runEnds = runStart != -1 && (x - lastSnow > MaxGapInRun || x == Main.maxTilesX - 41);

            if (!runEnds)
                continue;

            if (lastSnow - runStart > bestEnd - bestStart)
            {
                bestStart = runStart;
                bestEnd = lastSnow;
            }

            runStart = -1;
        }

        if (bestStart == -1)
            return;

        int center = (bestStart + bestEnd) / 2;

        for (int offset = 0; offset <= (bestEnd - bestStart) / 2; offset++)
        {
            foreach (int x in new[] { center - offset, center + offset })
            {
                int groundY = FindGround(x);

                if (groundY < 0 || !IsSnowGround(x) || !HasFreeSpace(x, groundY))
                    continue;

                Main.spawnTileX = x;
                Main.spawnTileY = groundY;
                return;
            }
        }
    }

    private static bool IsSnowGround(int x)
    {
        int groundY = FindGround(x);

        if (groundY < 0)
            return false;

        ushort type = Main.tile[x, groundY].TileType;
        return type == TileID.SnowBlock || type == TileID.IceBlock || type == TileID.Slush;
    }

    private static int FindGround(int x)
    {
        int limit = (int)Main.worldSurface + 50;

        for (int y = 20; y < limit; y++)
        {
            Tile tile = Main.tile[x, y];

            if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                return y;
        }

        return -1;
    }

    private static bool HasFreeSpace(int x, int groundY)
    {
        for (int y = groundY - FreeHeight; y < groundY; y++)
        {
            Tile tile = Main.tile[x, y];

            if (tile.HasTile && Main.tileSolid[tile.TileType])
                return false;

            if (tile.LiquidAmount > 0)
                return false;
        }

        return true;
    }
}