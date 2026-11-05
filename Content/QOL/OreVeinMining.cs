namespace DisasterMod.Content.QOL;

internal sealed class OreListPlayer : ModPlayer
{
    public readonly HashSet<int> White = [];
    public readonly HashSet<int> Black = [];

    private static readonly Color ChatColor = new(255, 240, 20);

    private bool _middleWasDown;

    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        bool down = Main.mouseMiddle;
        bool pressed = down && !_middleWasDown;
        _middleWasDown = down;

        if (!pressed || !Main.hasFocus || Main.gameMenu || Main.mapFullscreen || Player.mouseInterface || Main.playerInventory && Main.mouseItem?.IsAir == false)
            return;

        int x = (int)(Main.MouseWorld.X / 16f);
        int y = (int)(Main.MouseWorld.Y / 16f);

        if (!WorldGen.InWorld(x, y, 2))
            return;

        Tile tile = Main.tile[x, y];

        if (!tile.HasTile || !TileID.Sets.Ore[tile.TileType])
            return;

        Cycle(tile.TileType, x, y);
    }

    private void Cycle(int type, int x, int y)
    {
        string name = OreName(type, x, y);
        string text;

        if (White.Contains(type))
        {
            White.Remove(type);
            Black.Add(type);
            text = Language.GetTextValue("Mods.DisasterMod.OreList.Blacklisted", name);
        }
        else
        {
            Black.Remove(type);
            White.Add(type);
            text = Language.GetTextValue("Mods.DisasterMod.OreList.Whitelisted", name);
        }

        Main.NewText(text, ChatColor);
    }

    private static string OreName(int type, int x, int y)
    {
        try
        {
            string name = Lang.GetMapObjectName(MapHelper.TileToLookup(type, 0));

            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }
        catch
        {

        }

        return Language.GetTextValue("Mods.DisasterMod.OreList.UnknownOre");
    }

    private static string Key(int type)
    {
        if (type < TileID.Count)
            return type.ToString();

        return ModContent.GetModTile(type)?.FullName;
    }

    private static int Parse(string key)
    {
        if (int.TryParse(key, out int id))
            return id;

        return ModContent.TryFind(key, out ModTile tile) ? tile.Type : -1;
    }

    public override void SaveData(TagCompound tag)
    {
        tag["oreWhite"] = White.Select(Key).Where(k => k != null).ToList();
        tag["oreBlack"] = Black.Select(Key).Where(k => k != null).ToList();
    }

    public override void LoadData(TagCompound tag)
    {
        White.Clear();
        Black.Clear();

        foreach (string key in tag.GetList<string>("oreWhite"))
        {
            int type = Parse(key);

            if (type >= 0)
                White.Add(type);
        }

        foreach (string key in tag.GetList<string>("oreBlack"))
        {
            int type = Parse(key);

            if (type >= 0)
                Black.Add(type);
        }
    }
}

internal sealed class OreVeinTile : GlobalTile
{
    private const int MaxTiles = 400;

    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (fail || effectOnly || Main.dedServ || OreVeinMining.Busy)
            return;

        if (!TileID.Sets.Ore[type])
            return;

        Player player = Main.LocalPlayer;

        if (!player.active || player.itemAnimation <= 0 || player.HeldItem.pick <= 0)
            return;

        if (Math.Abs(Player.tileTargetX - i) > 1 || Math.Abs(Player.tileTargetY - j) > 1)
            return;

        OreListPlayer lists = player.GetModPlayer<OreListPlayer>();

        if (!lists.White.Contains(type) || lists.Black.Contains(type))
            return;

        OreVeinMining.Enqueue(CollectVein(i, j, type));
    }

    private static List<Point> CollectVein(int i, int j, int type)
    {
        List<Point> result = [];
        HashSet<Point> seen = [new Point(i, j)];
        Queue<Point> open = new();
        open.Enqueue(new Point(i, j));

        while (open.Count > 0 && result.Count < MaxTiles)
        {
            Point p = open.Dequeue();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    Point n = new(p.X + dx, p.Y + dy);

                    if (!seen.Add(n) || !WorldGen.InWorld(n.X, n.Y, 2))
                        continue;

                    Tile tile = Main.tile[n.X, n.Y];

                    if (!tile.HasTile || tile.TileType != type)
                        continue;

                    result.Add(n);
                    open.Enqueue(n);

                    if (result.Count >= MaxTiles)
                        return result;
                }
            }
        }

        return result;
    }
}

internal sealed class OreVeinMining : ModSystem
{
    private const int PerTick = 3;

    private static readonly Queue<Point> Queue = new();

    internal static bool Busy { get; private set; }

    internal static void Enqueue(IEnumerable<Point> points)
    {
        foreach (Point p in points)
            Queue.Enqueue(p);
    }

    public override void PostUpdateEverything()
    {
        if (Main.dedServ || Queue.Count == 0)
            return;

        Busy = true;

        for (int n = 0; n < PerTick && Queue.Count > 0; n++)
        {
            Point p = Queue.Dequeue();
            Tile tile = Main.tile[p.X, p.Y];

            if (!tile.HasTile || !TileID.Sets.Ore[tile.TileType])
                continue;

            WorldGen.KillTile(p.X, p.Y, false, false, false);

            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, p.X, p.Y, 0f);
        }

        Busy = false;
    }

    public override void OnWorldUnload()
        => Queue.Clear();
}