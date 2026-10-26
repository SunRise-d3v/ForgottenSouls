namespace ForgottenSouls.Content.Globals;

internal sealed class MimicChest : ModSystem
{
    private static readonly HashSet<Point16> SafeChests = [];
    internal static bool MarkSafe(Point16 origin) => SafeChests.Add(origin);

    public override void ClearWorld() => SafeChests.Clear();

    public override void SaveWorldData(TagCompound tag)
    {
        if (SafeChests.Count > 0)
            tag["SafeChests"] = SafeChests.Select(static p => (p.X << 16) | p.Y).ToList();
    }

    public override void LoadWorldData(TagCompound tag)
    {
        SafeChests.Clear();

        foreach (int packed in tag.GetList<int>("SafeChests"))
            SafeChests.Add(new Point16(packed >> 16, packed & 0xFFFF));
    }

    internal static Point16 GetOrigin(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        return new Point16(i - tile.TileFrameX % 36 / 18, j - tile.TileFrameY % 36 / 18);
    }

    internal static bool IsChestTile(int type) => type == TileID.Containers || type == TileID.Containers2;

    internal static bool IsShadowChest(Tile tile)
    {
        if (tile.TileType != TileID.Containers)
            return false;

        int style = tile.TileFrameX / 36;
        return style == 3 || style == 4;
    }


    internal static bool IsTrapChest(Tile tile)
        => tile.TileType == TileID.FakeContainers || tile.TileType == TileID.FakeContainers2;
}

internal sealed class ChestPlacementTracker : GlobalTile
{
    public override void PlaceInWorld(int i, int j, int type, Item item)
    {
        if (MimicChest.IsChestTile(type))
            MimicChest.MarkSafe(MimicChest.GetOrigin(i, j));
    }
}

internal sealed class MimicChestPlayer : ModPlayer
{
    private const float MimicChance = 0.05f;

    private int _lastChest = -1;

    public override void PostUpdate()
    {
        int current = Player.chest;

        if (current >= 0 && current != _lastChest)
            OnChestOpened(current);

        _lastChest = Player.chest;
    }

    private void OnChestOpened(int index)
    {
        if (Main.netMode != NetmodeID.SinglePlayer)
            return;

        Chest chest = Main.chest[index];
        if (chest == null)
            return;

        Tile tile = Main.tile[chest.x, chest.y];
        if (!tile.HasTile || !MimicChest.IsChestTile(tile.TileType))
            return;

        if (MimicChest.IsShadowChest(tile) || MimicChest.IsTrapChest(tile))
            return;

        if (!MimicChest.MarkSafe(new Point16(chest.x, chest.y)))
            return;

        if (Main.rand.NextFloat() >= MimicChance)
            return;

        ConvertToMimic(chest);
    }

    private void ConvertToMimic(Chest chest)
    {
        int x = chest.x;
        int y = chest.y;
        var source = Player.GetSource_TileInteraction(x, y);

        Player.chest = -1;

        Rectangle area = new(x * 16, y * 16, 32, 32);

        for (int slot = 0; slot < chest.item.Length; slot++)
        {
            Item content = chest.item[slot];

            if (content.IsAir)
                continue;

            //Item.NewItem(source, area, content.Clone());
            content.TurnToAir();
        }

        WorldGen.KillTile(x, y, false, false, true);

        SpawnPerimeterDust(area);

        int index = NPC.NewNPC(source, x * 16 + 16, y * 16 + 32, NPCID.Mimic);

        if (index >= 0 && index < Main.maxNPCs)
        {
            Main.npc[index].velocity.Y = -4f;
            Main.npc[index].netUpdate = true;
        }
    }

    private static void SpawnPerimeterDust(Rectangle area)
    {
        const int points = 28;
        Vector2 center = area.Center.ToVector2();

        for (int i = 0; i < points; i++)
        {
            float along = i / (float)points * 4f;
            int side = (int)along;
            float f = along - side;

            Vector2 position = side switch
            {
                0 => new Vector2(area.Left + f * area.Width, area.Top),
                1 => new Vector2(area.Right, area.Top + f * area.Height),
                2 => new Vector2(area.Right - f * area.Width, area.Bottom),
                _ => new Vector2(area.Left, area.Bottom - f * area.Height),
            };

            Vector2 outward = (position - center).SafeNormalize(Vector2.UnitY);

            Dust dust = Dust.NewDustPerfect(position, DustID.Smoke, outward * 1.5f, 100, default, 1.6f);
            dust.noGravity = true;
        }
    }
}