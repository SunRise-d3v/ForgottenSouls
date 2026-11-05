namespace DisasterMod.Content.Items;

internal sealed class MirrorManaCost : GlobalItem
{
    private const int MANA_COST = 20;

    private static readonly HashSet<int> Mirrors =
    [
        ItemID.MagicMirror,
        ItemID.IceMirror,
        ItemID.CellPhone,
        ItemID.Shellphone,
        ItemID.ShellphoneOcean,
        ItemID.ShellphoneHell,
    ];

    public override bool CanUseItem(Item item, Player player)
    {
        if (Mirrors.Contains(item.type))
        {
            if (player.statMana < MANA_COST)
                return false;

            player.statMana -= MANA_COST;

            if (!Main.dedServ && player.whoAmI == Main.myPlayer)
                player.manaRegenDelay = (int)player.maxRegenDelay;

            return true;
        }

        return base.CanUseItem(item, player);
    }
}