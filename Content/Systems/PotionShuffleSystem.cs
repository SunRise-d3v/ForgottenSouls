namespace ForgottenSouls.Content.Systems;

/*internal class PotionShuffleSystem : ModSystem
{
    private static readonly Dictionary<int, int> Map = [];

    internal static bool TryGetBuff(int itemType, out int buffType) => Map.TryGetValue(itemType, out buffType);

    public override void ClearWorld() => Map.Clear();

    public override void OnWorldLoad()
    {
        if (Map.Count == 0)
            Generate();
    }

    public override void SaveWorldData(TagCompound tag)
    {
        List<string> items = [];
        List<string> buffs = [];

        foreach ((int item, int buff) in Map)
        {
            if (ItemID.Search.TryGetName(item, out string itemName) && BuffID.Search.TryGetName(buff, out string buffName))
            {
                items.Add(itemName);
                buffs.Add(buffName);
            }
        }

        tag["PotionItems"] = items;
        tag["PotionBuffs"] = buffs;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        Map.Clear();

        IList<string> items = tag.GetList<string>("PotionItems");
        IList<string> buffs = tag.GetList<string>("PotionBuffs");

        for (int i = 0; i < items.Count && i < buffs.Count; i++)
        {
            if (ItemID.Search.TryGetId(items[i], out int item)
                && BuffID.Search.TryGetId(buffs[i], out int buff)
                && IsShufflableBuff(buff))
            {
                Map[item] = buff;
            }
        }

        if (Map.Count == 0)
            Generate();
    }

    private static bool IsShufflableBuff(int buffType)
    {
        return buffType > 0
            && !Main.debuff[buffType]
            && !Main.buffNoTimeDisplay[buffType]
            && !BuffID.Sets.TimeLeftDoesNotDecrease[buffType]
            && BuffID.Sets.BasicMountData[buffType] is null
            && !Main.vanityPet[buffType]
            && !Main.lightPet[buffType];
    }

    private static bool IsShufflablePotion(Item item)
    => item.consumable
       && item.useStyle == ItemUseStyleID.DrinkLiquid
       && item.buffType > 0
       && item.buffTime > 0
       && IsShufflableBuff(item.buffType)
       && !ItemID.Sets.IsFood[item.type];

    private static void Generate()
    {
        Map.Clear();

        List<int> potions = [];

        foreach ((int type, Item sample) in ContentSamples.ItemsByType)
        {
            if (type > 0 && IsShufflablePotion(sample))
                potions.Add(type);
        }

        if (potions.Count < 2)
            return;

        List<int> shuffled = [.. potions.OrderBy(_ => Main.rand.Next())];

        for (int i = 0; i < shuffled.Count; i++)
        {
            int itemType = shuffled[i];
            int buffType = ContentSamples.ItemsByType[shuffled[(i + 1) % shuffled.Count]].buffType;

            if (IsShufflableBuff(buffType))
                Map[itemType] = buffType;
        }
    }

    internal static IEnumerable<int> GetBuffs()
        => Map.Values.Distinct();
}

internal class PotionShufflePlayer : ModPlayer
{
    public override void PreUpdate()
    {
        for (int i = 0; i < Player.inventory.Length; i++)
            Remap(Player.inventory[i]);

        Remap(Main.mouseItem);
    }

    public override void PostUpdateBuffs()
    {
        for (int i = 0; i < Player.buffType.Length; i++)
        {
            int buffType = Player.buffType[i];

            if (buffType <= 0)
                continue;

            foreach (int shuffledBuff in PotionShuffleSystem.GetBuffs())
            {
                if (buffType != shuffledBuff)
                    continue;

                if (Player.buffTime[i] > 0)
                {
                    if (Main.GameUpdateCount % 60 == 0)
                    {
                        Main.NewText(
                            $"Buff: {buffType}, Time: {Player.buffTime[i]}");
                    }
                }

                break;
            }
        }
    }

    private static void Remap(Item item)
    {
        if (!item.IsAir &&
            PotionShuffleSystem.TryGetBuff(item.type, out int buff))
        {
            item.buffType = buff;
        }
    }
}*/