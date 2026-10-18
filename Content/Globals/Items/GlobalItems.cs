namespace NoCompromises.Content.Items;

/*internal class GlobalItems : GlobalItem
{
    public override void SetDefaults(Item item)
    {
        if (item.pick > 0 || item.axe > 0 || item.hammer > 0)
            item.useTime = (int)(item.useTime * 0.81f);
    }
}*/

/*public class NonConsumingBossSummons : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        int[] bossSummons = {
                //Pre-Hardmode
                ItemID.SlimeCrown,
                ItemID.SuspiciousLookingEye,
                ItemID.WormFood,
                ItemID.BloodySpine,
                ItemID.Abeemination,
                //Skeletron
                ItemID.DeerThing,
                //Wall Of Flesh

                //Hardmode
                ItemID.QueenSlimeCrystal,
                ItemID.MechanicalEye,
                ItemID.MechanicalWorm,
                ItemID.MechanicalSkull,
                //Plantera
                ItemID.LihzahrdPowerCell,
                //Duke Fishron
                //Empress Of Light
                //Lunatic Cultist
                ItemID.CelestialSigil
            };

        if (Array.Exists(bossSummons, x => x == entity.type))
            return true;

        if (entity.ModItem != null)
        {
            try
            {
                string tooltip = Language.GetTextValue(entity.ModItem.Tooltip.Key);
                return tooltip.Contains("Summons") &&
                       tooltip.Contains("boss", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    public override void SetDefaults(Item item)
    {
        item.consumable = false;
        item.maxStack = 1;
    }

    /*public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "NonConsumable", Language.GetTextValue("Mods.DisasterMod.CommonItemTooltip.NotConsumed")));
    }
}*/

/*public class MushroomNoPotionSickness : GlobalItem
{
    public override bool AppliesToEntity(Item item, bool lateInstantiation)
    {
        return item.type == ItemID.Mushroom;
    }

    public override bool CanUseItem(Item item, Player player)
    {
        return true;
    }

    public override void OnConsumeItem(Item item, Player player)
    {
        player.ClearBuff(BuffID.PotionSickness);
    }

    public override void SetDefaults(Item item)
    {
        item.potion = false;
    }
}*/