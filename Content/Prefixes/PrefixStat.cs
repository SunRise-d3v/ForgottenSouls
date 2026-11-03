namespace ForgottenSouls.Content.Prefixes;

/// <summary>What a prefix improves.</summary>
internal enum PrefixStat
{
    ArmorPenetration,
    DamageOverTime,
    LifeSteal,
    DamageReduction,
    Thorns,
    LifeRegeneration,
    WingTime,
    ManaCostReduction,
    ManaRegeneration,
    MinionBonus,
    WhipRange,
    Capacity,
    FlatDamage
}

/// <summary>What kind of item a prefix can appear on.</summary>
internal enum PrefixKind
{
    Weapon,     // any weapon
    Whip,       // whips only
    Accessory,  // accessories (wings included)
    Wings       // wings only
}

/// <param name="Name">Internal name of the generated ModPrefix (also the localization key).</param>
/// <param name="Rank">1 = weakest tier of this stat and kind. Higher rank = more expensive item.</param>
internal sealed record PrefixTier(string Name, PrefixStat Stat, PrefixKind Kind, float Value, bool HardmodeOnly, int Rank)
{
    public bool IsAccessoryKind => Kind is PrefixKind.Accessory or PrefixKind.Wings;
}

/// <summary>The table of every prefix in the mod. To add or change a prefix, edit Build().</summary>
internal static class PrefixRegistry
{
    public static readonly List<PrefixTier> All = [];

    // Filled when the prefixes are loaded
    public static readonly Dictionary<int, PrefixTier> ByType = [];
    public static readonly Dictionary<string, int> TypeByName = [];

    static PrefixRegistry() => Build();

    /// <summary>The total a stat can never exceed, however many sources add to it.</summary>
    public static float Cap(PrefixStat stat) => stat switch
    {
        PrefixStat.ArmorPenetration => 15f,
        PrefixStat.DamageOverTime => 10f,
        PrefixStat.LifeSteal => 4f,
        PrefixStat.DamageReduction => 4f,
        PrefixStat.Thorns => 40f,
        PrefixStat.LifeRegeneration => 10f,   // health per second
        PrefixStat.ManaCostReduction => 40f,
        PrefixStat.ManaRegeneration => 20f,
        PrefixStat.WhipRange => 25f,
        _ => float.MaxValue                   // wing time, capacity, flat damage: no cap
    };

    private static void Build()
    {
        //        stat                              kind                    values that roll always     values that need Hardmode
        Define(PrefixStat.ArmorPenetration, PrefixKind.Weapon, [2, 4, 5], [7]);
        Define(PrefixStat.ArmorPenetration, PrefixKind.Accessory, [2, 4, 5], [7]);

        Define(PrefixStat.DamageOverTime, PrefixKind.Accessory, [1, 2, 3, 4], [5]);

        Define(PrefixStat.LifeSteal, PrefixKind.Weapon, [0.3f, 0.7f, 1f], [2.5f]);
        Define(PrefixStat.LifeSteal, PrefixKind.Accessory, [0.3f, 0.7f, 1f], [2.5f]);

        Define(PrefixStat.DamageReduction, PrefixKind.Accessory, [0.5f, 1f, 1.5f], [2f, 2.5f]);

        Define(PrefixStat.Thorns, PrefixKind.Accessory, [3, 5], [10, 15]);

        Define(PrefixStat.LifeRegeneration, PrefixKind.Accessory, [0.5f, 1f, 1.5f, 2f], [2.5f, 3f]);

        Define(PrefixStat.WingTime, PrefixKind.Wings, [2, 3, 6], [10]);

        Define(PrefixStat.ManaCostReduction, PrefixKind.Accessory, [2, 4, 6, 8], [11]);

        Define(PrefixStat.ManaRegeneration, PrefixKind.Accessory, [1, 2, 3, 4], [5, 6, 7]);

        Define(PrefixStat.MinionBonus, PrefixKind.Accessory, [1], [2]);

        Define(PrefixStat.WhipRange, PrefixKind.Whip, [3, 5, 7], [10]);

        // Older prefixes keep their names, so items that already carry them stay valid
        Define(PrefixStat.Capacity, PrefixKind.Accessory, [20, 50, 100], [],
            "PorterPrefix", "PackAnimalPrefix", "HaulerPrefix");

        Define(PrefixStat.FlatDamage, PrefixKind.Weapon, [5, 10, 15], [],
            "TemperedPrefix", "ForgedPrefix", "MasterworkPrefix");
    }

    private static void Define(PrefixStat stat, PrefixKind kind, float[] values, float[] hardmodeValues, params string[] names)
    {
        int rank = 0;

        foreach (float value in values)
            Add(stat, kind, value, false, ++rank, names);

        foreach (float value in hardmodeValues)
            Add(stat, kind, value, true, ++rank, names);
    }

    private static void Add(PrefixStat stat, PrefixKind kind, float value, bool hardmode, int rank, string[] names)
    {
        string name = names.Length >= rank ? names[rank - 1] : $"{stat}{kind}{rank}";
        All.Add(new PrefixTier(name, stat, kind, value, hardmode, rank));
    }

    // ---- Reading what an item carries ----

    /// <summary>The prefix of the item plus (on accessories) the extra modifiers.</summary>
    public static IEnumerable<PrefixTier> TiersOf(Item item)
    {
        if (!ByType.TryGetValue(item.prefix, out PrefixTier primary))
            yield break;

        yield return primary;

        if (!item.TryGetGlobalItem(out PrefixItem data))
            yield break;

        foreach (int type in data.Extras)
        {
            if (ByType.TryGetValue(type, out PrefixTier extra))
                yield return extra;
        }
    }

    /// <summary>Sum of a stat over the weapon-type prefixes of a weapon.</summary>
    public static float SumWeapon(Item item, PrefixStat stat)
    {
        float total = 0f;

        foreach (PrefixTier tier in TiersOf(item))
        {
            if (tier.Stat == stat && tier.Kind is PrefixKind.Weapon or PrefixKind.Whip)
                total += tier.Value;
        }

        return total;
    }
}