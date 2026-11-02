namespace ForgottenSouls.Content;

internal class ItemWeightTooltip : GlobalItem
{
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (item.IsAir)
            return;

        string weight = WeightPlayer.UnitWeight(item).ToString("0.##");

        TooltipLine line = new(Mod, "ItemWeight", Language.GetTextValue("Mods.ForgottenSouls.Misc.ItemWeight", weight));

        int priceIndex = tooltips.FindIndex(l => l.Mod == "Terraria" && l.Name == "Price");

        if (priceIndex >= 0)
            tooltips.Insert(priceIndex, line);
        else
            tooltips.Add(line);
    }
}