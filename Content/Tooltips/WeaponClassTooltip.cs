namespace DisasterMod.Content.ModTooltip;

internal sealed class WeaponClassTooltip : GlobalItem
{
    private static string Localization(string key) => Language.GetTextValue($"Mods.DisasterMod.WeaponClass.{key}");

    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return lateInstantiation
            && entity.damage > 0
            && entity.ammo == 0
            && entity.pick == 0
            && entity.axe == 0
            && entity.hammer == 0;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        (string name, Color color) = GetClass(item);

        TooltipLine line = new(Mod, "WeaponClass", name)
        {
            OverrideColor = color
        };

        int index = tooltips.FindIndex(t => t.Mod == "Terraria" && t.Name == "ItemName");
        tooltips.Insert(index + 1, line);
    }

    private static (string, Color) GetClass(Item item)
    {
        if (item.CountsAsClass(DamageClass.Summon))
            return (Localization("Summon"), new Color(120, 200, 255));

        if (item.CountsAsClass(DamageClass.Magic))
            return (Localization("Magic"), new Color(180, 120, 255));

        if (item.CountsAsClass(DamageClass.Ranged))
            return (Localization("Ranged"), new Color(255, 200, 90));

        if (item.CountsAsClass(DamageClass.Throwing))
            return (Localization("Throwing"), new Color(120, 230, 150));

        if (item.CountsAsClass(DamageClass.Melee))
            return (Localization("Melee"), new Color(255, 110, 110));

        if (item.DamageType != DamageClass.Generic)
            return (item.DamageType.DisplayName.Value, Color.LightGray);

        return (Localization("Universal"), Color.LightGray);
    }
}