namespace ForgottenSouls.Content.Prefixes;

/// <summary>
/// Applies the prefix effects and gives accessories extra modifiers.
/// Extra modifiers: an accessory can carry up to 3 modifiers (the prefix itself + 2 extras), limited by rarity:
///   rarity -1, 0, 1, 2 -> 1 modifier;  rarity 3, 4, 5, 6 -> up to 2;  everything else (and modded rarities) -> up to 3.
/// </summary>
internal sealed class PrefixItem : GlobalItem
{
    private const float SecondChance = 0.12f;  // chance of a second modifier (if the rarity allows it)
    private const float ThirdChance = 0.35f;   // chance of a third one once there is a second (about 4% overall)

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        => lateInstantiation && (entity.accessory || entity.damage > 0);

    /// <summary>Prefix types of the extra modifiers (accessories only).</summary>
    public List<int> Extras = [];

    private bool rolled;

    // ---------------------------------------------------------------
    //  Extra modifiers
    // ---------------------------------------------------------------

    private static int MaxModifiers(Item item) => item.rare switch
    {
        >= -1 and <= 2 => 1,
        >= 3 and <= 6 => 2,
        _ => 3
    };

    /// <summary>Called when a mod prefix was applied to an accessory: rolls the extra modifiers.</summary>
    internal void RollExtras(Item item, int primaryPrefix)
    {
        Extras.Clear();
        rolled = true;

        int max = MaxModifiers(item);

        if (max <= 1 || !PrefixRegistry.ByType.TryGetValue(primaryPrefix, out PrefixTier primary))
            return;

        HashSet<PrefixStat> used = [primary.Stat];

        if (Main.rand.NextFloat() >= SecondChance)
            return;

        AddExtra(item, used);

        if (max >= 3 && Extras.Count == 1 && Main.rand.NextFloat() < ThirdChance)
            AddExtra(item, used);
    }

    private void AddExtra(Item item, HashSet<PrefixStat> used)
    {
        List<PrefixTier> candidates = [];

        foreach (PrefixTier tier in PrefixRegistry.All)
        {
            if (!tier.IsAccessoryKind || used.Contains(tier.Stat))
                continue;

            if (tier.HardmodeOnly && !Main.hardMode)
                continue;

            if (tier.Kind == PrefixKind.Wings && item.wingSlot <= 0)
                continue;

            candidates.Add(tier);
        }

        if (candidates.Count == 0)
            return;

        PrefixTier pick = candidates[Main.rand.Next(candidates.Count)];

        if (!PrefixRegistry.TypeByName.TryGetValue(pick.Name, out int type))
            return;

        Extras.Add(type);
        used.Add(pick.Stat);
    }

    public override void PreReforge(Item item)
    {
        Extras.Clear();
        rolled = false;
    }

    public override GlobalItem Clone(Item from, Item to)
    {
        PrefixItem copy = (PrefixItem)base.Clone(from, to);
        copy.Extras = [.. Extras];
        return copy;
    }

    public override void SaveData(Item item, TagCompound tag)
    {
        if (Extras.Count == 0)
            return;

        List<string> names = [];

        foreach (int type in Extras)
        {
            ModPrefix prefix = PrefixLoader.GetPrefix(type);

            if (prefix != null)
                names.Add(prefix.FullName);
        }

        tag["extras"] = names;
    }

    public override void LoadData(Item item, TagCompound tag)
    {
        Extras.Clear();

        foreach (string name in tag.GetList<string>("extras"))
        {
            if (ModContent.TryFind(name, out ModPrefix prefix))
                Extras.Add(prefix.Type);
        }

        rolled = Extras.Count > 0;
    }

    public override void NetSend(Item item, BinaryWriter writer)
    {
        writer.Write((byte)Extras.Count);

        foreach (int type in Extras)
            writer.Write((ushort)type);
    }

    public override void NetReceive(Item item, BinaryReader reader)
    {
        Extras.Clear();
        int count = reader.ReadByte();

        for (int i = 0; i < count; i++)
            Extras.Add(reader.ReadUInt16());
    }

    // The primary prefix writes its own line; the extras are added below it
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (Extras.Count == 0 || !PrefixRegistry.ByType.ContainsKey(item.prefix))
            return;

        int number = 1;

        foreach (int type in Extras)
        {
            if (PrefixRegistry.ByType.TryGetValue(type, out PrefixTier tier))
                tooltips.Add(PrefixText.Line(Mod, tier, $"ExtraPrefix{number++}"));
        }
    }

    // ---------------------------------------------------------------
    //  Effects
    // ---------------------------------------------------------------

    // Accessories hand their stats to the player; the player applies them (with the caps)
    public override void UpdateAccessory(Item item, Player player, bool hideVisual)
    {
        PrefixPlayer data = player.GetModPlayer<PrefixPlayer>();

        foreach (PrefixTier tier in PrefixRegistry.TiersOf(item))
        {
            if (tier.IsAccessoryKind)
                data.Add(tier.Stat, tier.Value);
        }
    }

    // Flat damage: added after the multipliers, so the tooltip and the fight show exactly +N
    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
    {
        float flat = PrefixRegistry.SumWeapon(item, PrefixStat.FlatDamage);

        if (flat > 0f)
            damage.Flat += flat;
    }

    // Whip range, only while the whip is in hand
    public override void HoldItem(Item item, Player player)
    {
        float range = PrefixRegistry.SumWeapon(item, PrefixStat.WhipRange);

        if (range > 0f)
            player.whipRangeMultiplier += Math.Min(range, PrefixRegistry.Cap(PrefixStat.WhipRange)) / 100f;
    }
}

/// <summary>Remembers the armor penetration and life steal of the weapon a projectile was fired from.</summary>
internal sealed class PrefixProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    public float ArmorPenetration;
    public float LifeSteal;

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (source is not EntitySource_ItemUse use || use.Item == null)
            return;

        ArmorPenetration = PrefixRegistry.SumWeapon(use.Item, PrefixStat.ArmorPenetration);
        LifeSteal = PrefixRegistry.SumWeapon(use.Item, PrefixStat.LifeSteal);
    }
}