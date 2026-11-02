using System;
using System.Collections.Generic;

using FargowiltasSouls.Content.Bosses.TrojanSquirrel;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CrossMod;

/// <summary>Когда игрок призывает Троянскую белку, вокруг него появляется арена: деревянная рамка 100x100 блоков.</summary>
internal class TrojanArenaTrigger : GlobalNPC
{
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => entity.type == ModContent.NPCType<TrojanSquirrel>();

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        // Арена строится вокруг ближайшего игрока (того, кто призвал белку)
        Player player = Main.player[Player.FindClosest(npc.Center, 0, 0)];

        if (player.active && !player.dead)
            TrojanArenaSystem.Build(player.Center.ToTileCoordinates());
    }
}

internal class TrojanArenaSystem : ModSystem
{
    private const int Size = 100;          // сторона квадрата в блоках
    private const int Thickness = 2;       // толщина стены в блоках
    private const int WorldMargin = 50;    // насколько арена должна держаться от края мира
    private const int SyncChunk = 25;      // кусками такого размера отправляем изменения клиентам
    private const int AbsentTicks = 60;    // сколько белки не должно быть в мире, чтобы арена исчезла

    // Блоки арены, которые мы поставили: только их мы потом убираем (настоящий ландшафт не трогаем)
    private static readonly HashSet<Point> Placed = [];

    private int _absent;

    /// <summary>Арена в координатах блоков. null, если арены сейчас нет.</summary>
    public static Rectangle? Area { get; private set; }

    public static bool Contains(Vector2 worldPosition)
        => Area is Rectangle area && area.Contains(worldPosition.ToTileCoordinates());

    // ---------- Постройка ----------

    internal static void Build(Point center)
    {
        if (Area != null)
            return;

        int left = Math.Clamp(center.X - Size / 2, WorldMargin, Main.maxTilesX - Size - WorldMargin);
        int top = Math.Clamp(center.Y - Size / 2, WorldMargin, Main.maxTilesY - Size - WorldMargin);

        Area = new Rectangle(left, top, Size, Size);

        // Верхняя и нижняя стены на всю ширину
        for (int x = left; x < left + Size; x++)
        {
            for (int t = 0; t < Thickness; t++)
            {
                Place(x, top + t);
                Place(x, top + Size - 1 - t);
            }
        }

        // Боковые стены между ними
        for (int y = top + Thickness; y < top + Size - Thickness; y++)
        {
            for (int t = 0; t < Thickness; t++)
            {
                Place(left + t, y);
                Place(left + Size - 1 - t, y);
            }
        }

        SyncArea(left, top);
    }

    private static void Place(int x, int y)
    {
        if (!WorldGen.InWorld(x, y, 5))
            return;

        Tile tile = Main.tile[x, y];

        // Уже есть сплошной блок: он сам служит стеной, его не трогаем
        if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
            return;

        tile.HasTile = true;
        tile.TileType = TileID.WoodBlock;
        tile.IsHalfBlock = false;
        tile.Slope = SlopeType.Solid;

        Placed.Add(new Point(x, y));
        WorldGen.SquareTileFrame(x, y, false);
    }

    private static void SyncArea(int left, int top)
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        for (int x = left; x < left + Size; x += SyncChunk)
        {
            for (int y = top; y < top + Size; y += SyncChunk)
                NetMessage.SendTileSquare(-1, x, y, SyncChunk, SyncChunk);
        }
    }

    // ---------- Удаление ----------

    public override void PostUpdateWorld()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || Placed.Count == 0)
            return;

        // Пока белка жива, арена стоит
        if (NPC.AnyNPCs(ModContent.NPCType<TrojanSquirrel>()))
        {
            _absent = 0;
            return;
        }

        if (++_absent >= AbsentTicks)
            RemoveArena();
    }

    private static void RemoveArena()
    {
        foreach (Point p in Placed)
        {
            Tile tile = Main.tile[p.X, p.Y];

            // Убираем только наши деревянные блоки (если игрок уже сломал их или заменил, пропускаем)
            if (tile.HasTile && tile.TileType == TileID.WoodBlock)
                tile.ClearTile();

            WorldGen.SquareTileFrame(p.X, p.Y, false);
        }

        if (Area is Rectangle area)
            SyncArea(area.X, area.Y);

        Placed.Clear();
        Area = null;
    }

    // ---------- Сохранение: блоки остаются в файле мира, поэтому после загрузки их нужно убрать ----------

    public override void ClearWorld()
    {
        Placed.Clear();
        Area = null;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (Placed.Count == 0)
            return;

        List<int> packed = [];

        foreach (Point p in Placed)
        {
            packed.Add(p.X);
            packed.Add(p.Y);
        }

        tag["ArenaTiles"] = packed;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        Placed.Clear();

        IList<int> packed = tag.GetList<int>("ArenaTiles");

        // После загрузки белки в мире нет, поэтому PostUpdateWorld сам уберёт остатки арены
        for (int i = 0; i + 1 < packed.Count; i += 2)
            Placed.Add(new Point(packed[i], packed[i + 1]));
    }
}

/// <summary>Пока игрок внутри арены, на нём висит «Творческий шок». Значок этого баффа скрыт.</summary>
internal class TrojanArenaPlayer : ModPlayer
{
    public bool Shocked;

    public override void PostUpdate()
    {
        Shocked = TrojanArenaSystem.Area != null && TrojanArenaSystem.Contains(Player.Center);

        // Продлеваем бафф, пока игрок внутри. Выйдет (или умрёт), через пару тиков бафф пропадёт сам
        if (Shocked && !Player.dead)
            Player.AddBuff(BuffID.NoBuilding, 60);
    }
}