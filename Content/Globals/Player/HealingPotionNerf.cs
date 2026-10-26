namespace ForgottenSouls.Content.Items;

internal sealed class HealingPotionNerf : GlobalItem
{
    private const float Multiplier = 0.8f;   // 100% - 20%

    private static bool IsHealingPotion(Item item) => item.potion && item.healLife > 0;

    public override void GetHealLife(Item item, Player player, bool quickHeal, ref int healValue)
    {
        if (!IsHealingPotion(item))
            return;

        healValue = (int)Math.Round(healValue * Multiplier);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (!IsHealingPotion(item))
            return;

        foreach (TooltipLine line in tooltips)
        {
            if (line.Mod == "Terraria" && line.Name == "HealLife")
                line.Text = Language.GetTextValue("CommonItemTooltip.RestoresLife", Main.LocalPlayer.GetHealLife(item));
        }
    }
}