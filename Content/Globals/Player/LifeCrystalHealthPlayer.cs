namespace ForgottenSouls.Content;

internal sealed class LifeCrystalHealthPlayer : ModPlayer
{
    private const int VanillaBonus = 20;
    private const int NewBonus = 15;

    private int _lastLifeMax = -1;

    public override void PreUpdate()
    {
        _lastLifeMax = Player.statLifeMax;
    }

    public override void PostUpdate()
    {
        if (_lastLifeMax < 0 || Player.statLifeMax - _lastLifeMax != VanillaBonus)
            return;

        int excess = VanillaBonus - NewBonus;

        Player.statLifeMax -= excess;
        Player.statLife = System.Math.Max(1, Player.statLife - excess);
    }
}

internal sealed class LifeCrystalTooltip : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.LifeCrystal;

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        foreach (TooltipLine line in tooltips)
        {
            if (line.Mod == "Terraria" && line.Name.StartsWith("Tooltip"))
                line.Text = line.Text.Replace("20", "15");
        }
    }
}