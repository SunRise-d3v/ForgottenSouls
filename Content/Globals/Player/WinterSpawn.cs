namespace ForgottenSouls.Content;

internal class WinterSpawn : ModSystem
{
#if false
    private const int EdgeBlend = 15;     
    private const int SnowDepth = 60;     
    private const int RockLayerExtra = 20; 
    private const int BreakTolerance = 6;  
    private const int TreeSpacingMin = 7;
    private const int TreeSpacingMax = 14;

    private static readonly HashSet<int> ForestGround =
    [
        TileID.Grass,
        TileID.Dirt,
        TileID.Stone,
        TileID.ClayBlock
    ];

    private static readonly HashSet<int> Vegetation =
    [
        TileID.Trees,
        TileID.Plants,
        TileID.Plants2,
        TileID.Saplings,
        TileID.Sunflower,
        TileID.Vines
    ];

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        int index = tasks.FindIndex(pass => pass.Name == "Spawn Point");

        if (index == -1)
            index = tasks.FindIndex(pass => pass.Name == "Final Cleanup");

        if (index == -1)
            index = tasks.Count - 1;

        tasks.Insert(index, new PassLegacy("Creating Winter Spawn", GenerateWinterSpawn));
    }

    private static void GenerateWinterSpawn(GenerationProgress progress, GameConfiguration configuration)
    {
        int centerX = Main.maxTilesX / 2;

        int left = FindBiomeEdge(centerX, -1);
        int right = FindBiomeEdge(centerX, 1);
        int bottomLimit = (int)Main.rockLayer + RockLayerExtra;
        int nextTree = left + 8;

        for (int x = left; x <= right; x++)
        {
            int groundY = FindGround(x);

            if (groundY < 0)
                continue;

            ClearVegetation(x, groundY);

            int edge = Math.Min(x - left, right - x);

            if (edge < EdgeBlend && WorldGen.genRand.NextDouble() > edge / (double)EdgeBlend)
                continue;

            ConvertColumn(x, groundY, bottomLimit);

            if (x >= nextTree && edge > 5)
            {
                TryGrowTree(x, groundY);
                nextTree = x + WorldGen.genRand.Next(TreeSpacingMin, TreeSpacingMax);
            }
        }

        int spawnGround = FindGround(centerX);

        if (spawnGround >= 0)
        {
            Main.spawnTileX = centerX;
            Main.spawnTileY = spawnGround;
        }
    }

    private static int FindBiomeEdge(int startX, int direction)
    {
        int lastForest = startX;
        int broken = 0;
        int maxDistance = Main.maxTilesX / 3;

        for (int step = 1; step <= maxDistance; step++)
        {
            int x = startX + direction * step;

            if (x < 15 || x > Main.maxTilesX - 15)
                break;

            int groundY = FindGround(x);

            if (groundY < 0)
            {
                if (++broken > BreakTolerance)
                    break;

                continue;
            }

            if (Main.tile[x, groundY - 1].LiquidAmount > 0)
                continue;

            if (ForestGround.Contains(Main.tile[x, groundY].TileType))
            {
                lastForest = x;
                broken = 0;
            }
            else if (++broken > BreakTolerance)
            {
                break;
            }
        }

        return lastForest;
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

    private static void ClearVegetation(int x, int groundY)
    {
        for (int y = Math.Max(20, groundY - 45); y < groundY; y++)
        {
            Tile tile = Main.tile[x, y];

            if (tile.HasTile && Vegetation.Contains(tile.TileType))
                WorldGen.KillTile(x, y, false, false, true);
        }
    }

    private static void ConvertColumn(int x, int groundY, int bottomLimit)
    {
        int bottom = Math.Min(bottomLimit, Main.maxTilesY - 10);

        for (int y = groundY; y <= bottom; y++)
        {
            Tile tile = Main.tile[x, y];
            int depth = y - groundY;

            if (tile.HasTile)
            {
                ushort type = tile.TileType;

                if (type == TileID.Grass || type == TileID.Dirt || type == TileID.ClayBlock)
                    tile.TileType = depth < SnowDepth ? TileID.SnowBlock : TileID.IceBlock;
                else if (type == TileID.Stone)
                    tile.TileType = TileID.IceBlock;
            }

            ConvertWall(tile);
            WorldGen.SquareTileFrame(x, y, false);
        }
    }

    private static void ConvertWall(Tile tile)
    {
        switch (tile.WallType)
        {
            case WallID.DirtUnsafe:
            case WallID.DirtUnsafe1:
            case WallID.DirtUnsafe2:
            case WallID.DirtUnsafe3:
            case WallID.DirtUnsafe4:
                tile.WallType = WallID.SnowWallUnsafe;
                break;

            case WallID.Stone:
                tile.WallType = WallID.IceUnsafe;
                break;
        }
    }

    private static void TryGrowTree(int x, int groundY)
    {
        if (!WorldGen.InWorld(x, groundY, 20))
            return;

        WorldGen.PlaceTile(x, groundY - 1, TileID.Saplings, true);
        WorldGen.GrowTree(x, groundY - 1);
    }
#endif
}