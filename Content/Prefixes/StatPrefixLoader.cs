namespace ForgottenSouls.Content.Prefixes;

/// <summary>Creates one ModPrefix for every entry of the table in PrefixRegistry.</summary>
internal sealed class StatPrefixLoader : ILoadable
{
    public void Load(Mod mod)
    {
        foreach (PrefixTier tier in PrefixRegistry.All)
            mod.AddContent(new StatPrefix(tier));
    }

    public void Unload()
    {
        PrefixRegistry.ByType.Clear();
        PrefixRegistry.TypeByName.Clear();
    }
}

internal static class PrefixText
{
    public static TooltipLine Line(Mod mod, PrefixTier tier, string name)
    {
        string value = tier.Value.ToString("0.##", CultureInfo.InvariantCulture);

        return new TooltipLine(mod, name, Language.GetTextValue($"Mods.ForgottenSouls.Prefixes.Stat.{tier.Stat}", value))
        {
            IsModifier = true,
        };
    }
}

[Autoload(false)] // created by StatPrefixLoader
internal sealed class StatPrefix(PrefixTier tier) : ModPrefix
{
    public override string Name => tier.Name;

    public override PrefixCategory Category
        => tier.Kind is PrefixKind.Weapon or PrefixKind.Whip ? PrefixCategory.AnyWeapon : PrefixCategory.Accessory;

    public override void SetStaticDefaults()
    {
        PrefixRegistry.ByType[Type] = tier;
        PrefixRegistry.TypeByName[tier.Name] = Type;
    }

    public override bool CanRoll(Item item)
    {
        if (tier.HardmodeOnly && !Main.hardMode)
            return false;

        return tier.Kind switch
        {
            PrefixKind.Wings => item.wingSlot > 0,
            PrefixKind.Whip => ProjectileID.Sets.IsAWhip[item.shoot],
            PrefixKind.Accessory => true,
            _ => item.damage > 0
        };
    }

    // Better tiers make the item worth more (this also makes reforging treat them as "better")
    public override void ModifyValue(ref float valueMult)
    {
        valueMult *= 1f + 0.1f * tier.Rank;
    }

    // A prefix was just put on an item: accessories may get extra modifiers
    public override void Apply(Item item)
    {
        if (tier.IsAccessoryKind && item.accessory)
            item.GetGlobalItem<PrefixItem>().RollExtras(item, Type);
    }

    public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
    {
        yield return PrefixText.Line(Mod, tier, "StatPrefix");
    }
}