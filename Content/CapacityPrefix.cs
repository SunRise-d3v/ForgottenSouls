namespace ForgottenSouls.Content.Prefix;

internal abstract class CapacityPrefix : ModPrefix
{
    private static readonly Dictionary<int, int> Bonuses = [];

    protected abstract int Bonus { get; }

    public override PrefixCategory Category => PrefixCategory.Accessory;

    public override bool CanRoll(Item item) => true;

    public override void SetStaticDefaults()
    {
        Bonuses[Type] = Bonus;
    }

    public override void Unload()
    {
        Bonuses.Clear();
    }

    public override void ModifyValue(ref float valueMult)
    {
        valueMult *= 1f + Bonus / 200f;
    }

    public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
    {
        yield return new TooltipLine(Mod, "CapacityPrefix", Language.GetTextValue("Mods.ForgottenSouls.Misc.CapacityPrefix", Bonus))
        {
            IsModifier = true,
        };
    }

    internal static bool TryGetBonus(int prefix, out int bonus) => Bonuses.TryGetValue(prefix, out bonus);
}

internal class PorterPrefix : CapacityPrefix
{
    protected override int Bonus => 20;
}

internal class PackAnimalPrefix : CapacityPrefix
{
    protected override int Bonus => 50;
}

internal class HaulerPrefix : CapacityPrefix
{
    protected override int Bonus => 100;
}

internal class CapacityPrefixItem : GlobalItem
{
    public override void UpdateAccessory(Item item, Player player, bool hideVisual) => Apply(item, player);

    public override void UpdateEquip(Item item, Player player) => Apply(item, player);

    private static void Apply(Item item, Player player)
    {
        if (CapacityPrefix.TryGetBonus(item.prefix, out int bonus))
            player.GetModPlayer<WeightPlayer>().ExtraLimit += bonus;
    }
}