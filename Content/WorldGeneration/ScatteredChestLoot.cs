namespace DisasterMod.Content.Globals;

internal sealed class ScatteredChestLootSystem : ModSystem
{
    private readonly record struct Junk(int Type, int Weight);

    private static readonly Junk[] JunkTable =
    [
        new(ItemID.Cobweb, 7),
        new(ItemID.Rope, 3),
        new(ItemID.Chain, 2),
        new(ItemID.Book, 2),
    ];

    private readonly record struct Accessory(int Type, int Weight);

    private static readonly Accessory[] SurfaceAccessories =
    [
        new(ItemID.Aglet, 6),
        new(ItemID.AnkletoftheWind, 4),
        new(ItemID.Shackle, 6),
        new(ItemID.Compass, 5),
        new(ItemID.DepthMeter, 5),
        new(ItemID.Radar, 4),
        new(ItemID.CloudinaBottle, 3),
        new(ItemID.HermesBoots, 3),
        new(ItemID.LuckyHorseshoe, 2),
        new(ItemID.ShinyRedBalloon, 2),
        // Новые
        new(ItemID.Stopwatch, 4),
        new(ItemID.WeatherRadio, 4),
        new(ItemID.Sextant, 3),
        new(ItemID.GoldWatch, 4),
        new(ItemID.FishermansGuide, 3),
        new(ItemID.Flipper, 3),
        new(ItemID.DivingHelmet, 2),
        new(ItemID.FlowerBoots, 2),
        new(ItemID.ClimbingClaws, 3),
        new(ItemID.ShoeSpikes, 3),
    ];

    private static readonly Accessory[] UndergroundAccessories =
    [
        new(ItemID.BandofRegeneration, 5),
        new(ItemID.BandofStarpower, 4),
        new(ItemID.FeralClaws, 4),
        new(ItemID.CobaltShield, 4),
        new(ItemID.MagicQuiver, 3),
        new(ItemID.WaterWalkingBoots, 3),
        new(ItemID.ClimbingClaws, 4),
        new(ItemID.ShoeSpikes, 4),
        new(ItemID.CloudinaBottle, 3),
        new(ItemID.HermesBoots, 3),
        new(ItemID.LuckyHorseshoe, 2),
        // Новые
        new(ItemID.MetalDetector, 3),
        new(ItemID.TallyCounter, 3),
        new(ItemID.Radar, 4),
        new(ItemID.DepthMeter, 4),
        new(ItemID.Compass, 4),
        new(ItemID.Aglet, 4),
        new(ItemID.AnkletoftheWind, 3),
        new(ItemID.Shackle, 4),
        new(ItemID.Flipper, 3),
        new(ItemID.DivingHelmet, 2),
        new(ItemID.TreasureMagnet, 2),
        new(ItemID.ShinyRedBalloon, 2),
        new(ItemID.PanicNecklace, 1),
    ];

    private static readonly Accessory[] HellAccessories =
    [
        new(ItemID.LavaCharm, 4),
        new(ItemID.ObsidianRose, 4),
        new(ItemID.MagmaStone, 3),
        new(ItemID.ObsidianSkull, 4),
        new(ItemID.ObsidianShield, 2),
        new(ItemID.FlameWakerBoots, 1),

        new(ItemID.ObsidianHorseshoe, 2),
        new(ItemID.BandofRegeneration, 3),
        new(ItemID.BandofStarpower, 3),
        new(ItemID.HermesBoots, 2),
        new(ItemID.CloudinaBottle, 2),
        new(ItemID.LuckyHorseshoe, 2),
        new(ItemID.Shackle, 3),
        new(ItemID.Aglet, 3),
        new(ItemID.TreasureMagnet, 2),
        new(ItemID.MetalDetector, 2),
    ];

    private const int AccessoryChance = 2;

    private static readonly int TotalWeight = Sum();

    private const int MinJunk = 4;
    private const int MaxJunk = 10;

    private const int SplitChance = 3;

    private const int Columns = 10;

    private static int Sum()
    {
        int total = 0;

        foreach (Junk j in JunkTable)
            total += j.Weight;

        return total;
    }

    public override void PostWorldGen()
    {
        foreach (Chest chest in Main.chest)
        {
            if (chest == null || !WorldGen.InWorld(chest.x, chest.y, 2))
                continue;

            Tile tile = Main.tile[chest.x, chest.y];

            if (!tile.HasTile || !Main.tileContainer[tile.TileType])
                continue;

            Scatter(chest);
        }
    }

    private static void Scatter(Chest chest)
    {
        UnifiedRandom rand = WorldGen.genRand;

        List<Item> loot = [];

        for (int i = 0; i < Chest.maxItems; i++)
        {
            if (chest.item[i] != null && !chest.item[i].IsAir)
                loot.Add(chest.item[i]);

            chest.item[i] = new Item();
        }

        List<Item> pieces = [];

        foreach (Item item in loot)
        {
            if (item.stack > 1 && item.maxStack > 1 && rand.NextBool(SplitChance) && pieces.Count + loot.Count < Chest.maxItems / 2 - 2)
            {
                int parts = System.Math.Min(rand.Next(2, 4), item.stack);
                int left = item.stack;

                for (int p = 0; p < parts; p++)
                {
                    Item piece = item.Clone();
                    int amount = p == parts - 1 ? left : rand.Next(1, left - (parts - p - 1) + 1);
                    piece.stack = amount;
                    left -= amount;
                    pieces.Add(piece);
                }
            }
            else
            {
                pieces.Add(item);
            }
        }

        int parity = rand.Next(2);
        List<int> chess = [];
        List<int> other = [];

        for (int i = 0; i < Chest.maxItems; i++)
        {
            int row = i / Columns;
            int col = i % Columns;

            if ((row + col) % 2 == parity)
                chess.Add(i);
            else
                other.Add(i);
        }

        foreach (Item item in pieces)
        {
            if (chess.Count == 0 && other.Count == 0)
                break;

            Place(chest, chess.Count > 0 ? chess : other, item, rand);
        }

        if (rand.NextBool(AccessoryChance))
        {
            int count = rand.NextBool(AccessoryChance * 3) ? 2 : 1;

            for (int n = 0; n < count && (chess.Count > 0 || other.Count > 0); n++)
            {
                Item acc = new();
                acc.SetDefaults(PickAccessory(chest, rand));
                acc.Prefix(-1);

                Place(chest, chess.Count > 0 ? chess : other, acc, rand);
            }
        }

        bool underground = chest.y > Main.worldSurface;
        int junkCount = underground ? rand.Next(MinJunk, MaxJunk + 1) : rand.Next(MinJunk - 2, MaxJunk - 3);

        for (int n = 0; n < junkCount && chess.Count > 0; n++)
        {
            int type = underground && rand.NextBool(3) ? ItemID.Cobweb : PickJunk(rand);

            Item item = new();
            item.SetDefaults(type);
            item.stack = 1;

            Place(chest, chess, item, rand);
        }
    }

    private static void Place(Chest chest, List<int> slots, Item item, UnifiedRandom rand)
    {
        int pick = rand.Next(slots.Count);
        chest.item[slots[pick]] = item;
        slots.RemoveAt(pick);
    }

    private static int PickJunk(UnifiedRandom rand)
    {
        int roll = rand.Next(TotalWeight);

        foreach (Junk j in JunkTable)
        {
            roll -= j.Weight;

            if (roll < 0)
                return j.Type;
        }

        return ItemID.Cobweb;
    }

    private static int PickAccessory(Chest chest, UnifiedRandom rand)
    {
        Accessory[] table;

        if (chest.y >= Main.UnderworldLayer)
            table = HellAccessories;
        else if (chest.y > Main.worldSurface)
            table = UndergroundAccessories;
        else
            table = SurfaceAccessories;

        int total = 0;

        foreach (Accessory a in table)
            total += a.Weight;

        int roll = rand.Next(total);

        foreach (Accessory a in table)
        {
            roll -= a.Weight;

            if (roll < 0)
                return a.Type;
        }

        return table[0].Type;
    }
}