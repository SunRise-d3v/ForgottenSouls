namespace ForgottenSouls.Content.Items;

internal sealed class HealingPotionNerf : GlobalItem
{
    private const float Multiplier = 0.8f;

    private static bool IsHealingPotion(Item item)
        => item.potion && item.healLife > 0;

    public override void GetHealLife(
        Item item,
        Player player,
        bool quickHeal,
        ref int healValue)
    {
        if (!Sadomasochist.Active || !IsHealingPotion(item))
            return;

        healValue = (int)Math.Round(healValue * Multiplier);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (!Sadomasochist.Active || !IsHealingPotion(item))
            return;

        foreach (TooltipLine line in tooltips)
        {
            if (line.Mod == "Terraria" && line.Name == "HealLife")
            {
                int healValue = (int)Math.Round(
                    Main.LocalPlayer.GetHealLife(item) * Multiplier);

                line.Text = Language.GetTextValue(
                    "CommonItemTooltip.RestoresLife",
                    healValue);
            }
        }
    }
}