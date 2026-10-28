namespace ForgottenSouls.Content.Globals;

internal sealed class ChestDebuffSystem : ModSystem
{
    public static readonly HashSet<Point16> Handled = [];

    public override void ClearWorld() => Handled.Clear();

    public override void SaveWorldData(TagCompound tag)
    {
        List<int> data = [];
        foreach (Point16 p in Handled)
        {
            data.Add(p.X);
            data.Add(p.Y);
        }
        tag["handled"] = data;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        Handled.Clear();
        IList<int> data = tag.GetList<int>("handled");
        for (int i = 0; i + 1 < data.Count; i += 2)
            Handled.Add(new Point16(data[i], data[i + 1]));
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(Handled.Count);
        foreach (Point16 p in Handled)
        {
            writer.Write(p.X);
            writer.Write(p.Y);
        }
    }

    public override void NetReceive(BinaryReader reader)
    {
        Handled.Clear();
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
            Handled.Add(new Point16(reader.ReadInt16(), reader.ReadInt16()));
    }

    public static void Mark(Point16 chest)
    {
        if (!Handled.Add(chest))
            return;

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            ModPacket packet = ModContent.GetInstance<ForgottenSouls>().GetPacket();
            packet.Write((byte)4);
            packet.Write(chest.X);
            packet.Write(chest.Y);
            packet.Send();
        }
    }
}

internal sealed class ChestPlacedTracker : GlobalTile
{
    public override void PlaceInWorld(int i, int j, int type, Item item)
    {
        if (!Main.tileContainer[type])
            return;

        Tile tile = Main.tile[i, j];
        int left = i - tile.TileFrameX / 18 % 2;
        int top = j - tile.TileFrameY / 18 % 2;

        ChestDebuffSystem.Mark(new Point16(left, top));
    }
}

internal sealed class ChestDebuffPlayer : ModPlayer
{
    private const float Chance = 0.5f;

    private static readonly (int Buff, int Seconds)[] Debuffs =
    [
        (BuffID.Poisoned, 10),
        (BuffID.Darkness, 15),
        (BuffID.Cursed, 5),
        (BuffID.OnFire, 6),
        (BuffID.Slow, 10),
        (BuffID.Weak, 15),
        (BuffID.Bleeding, 15),
        (BuffID.Confused, 8),
        (BuffID.Silenced, 6),
        (BuffID.Blackout, 6)
    ];

    private int lastChest = -1;

    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        if (Player.chest >= 0 && Player.chest != lastChest)
        {
            Chest chest = Main.chest[Player.chest];
            Point16 key = new(chest.x, chest.y);

            if (!ChestDebuffSystem.Handled.Contains(key))
            {
                ChestDebuffSystem.Mark(key);

                if (Main.rand.NextFloat() < Chance)
                {
                    var (buff, seconds) = Debuffs[Main.rand.Next(Debuffs.Length)];
                    Player.AddBuff(buff, seconds * 60);
                }
            }
        }

        lastChest = Player.chest;
    }
}