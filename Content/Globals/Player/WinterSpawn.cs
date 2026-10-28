using System;
using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ForgottenSouls.Content;

/// <summary>Moves the world spawn point into the largest snow biome on the surface.</summary>
internal class WinterSpawnSystem : ModSystem
{
    private const int FreeHeight = 4;      // free tiles required above the spawn ground
    private const int MaxGapInRun = 3;     // columns without snow that still count as the same biome

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        // Vanilla "Spawn Point" resets the spawn to the world center, so our pass goes right after it.
        // Later passes (the Guide, houses, etc.) then use the new spawn position.
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
        // Find the longest stretch of surface columns whose ground is snow or ice
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
            return; // no snow biome on the surface: keep the vanilla spawn

        // Take the snow column closest to the biome center that has room above it
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

    // First solid tile from the top is the real surface (trees and plants are not solid)
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

    // The player must fit above the ground and the spot must not be underwater
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