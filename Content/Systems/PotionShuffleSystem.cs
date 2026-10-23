namespace ForgottenSouls.Content.Systems;

internal class PotionShuffleSystem : ModSystem
{
    private static readonly Dictionary<int, int> Map = [];

    private static bool _ready;

    internal static bool TryGetBuff(int itemType, out int buffType)
    {
        EnsureGenerated();
        return Map.TryGetValue(itemType, out buffType);
    }

    public override void ClearWorld()
    {
        Map.Clear();
        _ready = false;
    }

    private static void EnsureGenerated()
    {
        if (_ready || Main.gameMenu)
            return;

        _ready = true;

        if (Map.Count == 0)
            Generate();
    }

    public override void SaveWorldData(TagCompound tag)
    {
        var pairs = Map
            .Where(p => ItemID.Search.TryGetName(p.Key, out _) && BuffID.Search.TryGetName(p.Value, out _))
            .ToList();

        tag["PotionItems"] = pairs.Select(p => ItemID.Search.GetName(p.Key)).ToList();
        tag["PotionBuffs"] = pairs.Select(p => BuffID.Search.GetName(p.Value)).ToList();
    }

    public override void LoadWorldData(TagCompound tag)
    {
        Map.Clear();

        IList<string> items = tag.GetList<string>("PotionItems");
        IList<string> buffs = tag.GetList<string>("PotionBuffs");

        for (int i = 0; i < Math.Min(items.Count, buffs.Count); i++)
        {
            if (ItemID.Search.TryGetId(items[i], out int item)
                && BuffID.Search.TryGetId(buffs[i], out int buff)
                && IsShufflableBuff(buff))
            {
                Map[item] = buff;
            }
        }
    }

    private static bool IsShufflableBuff(int buffType)
        => buffType > 0
           && !Main.debuff[buffType]
           && !Main.buffNoTimeDisplay[buffType]
           && !BuffID.Sets.TimeLeftDoesNotDecrease[buffType]
           && BuffID.Sets.BasicMountData[buffType] is null
           && !Main.vanityPet[buffType]
           && !Main.lightPet[buffType];

    private static bool IsShufflablePotion(Item item)
        => item.consumable
           && item.useStyle == ItemUseStyleID.DrinkLiquid
           && item.buffTime > 0
           && IsShufflableBuff(item.buffType)
           && !ItemID.Sets.IsFood[item.type];

    private static void Generate()
    {
        Map.Clear();

        List<int> potions = [.. ContentSamples.ItemsByType
            .Where(p => p.Key > 0 && IsShufflablePotion(p.Value))
            .Select(p => p.Key)
            .OrderBy(_ => Main.rand.Next())];

        for (int i = 0; i < potions.Count && potions.Count > 1; i++)
            Map[potions[i]] = ContentSamples.ItemsByType[potions[(i + 1) % potions.Count]].buffType;
    }
}

internal class PotionShufflePlayer : ModPlayer
{
    public override void PreUpdate()
    {
        foreach (Item item in Player.inventory.Append(Main.mouseItem))
        {
            if (!item.IsAir && PotionShuffleSystem.TryGetBuff(item.type, out int buff))
                item.buffType = buff;
        }
    }
}