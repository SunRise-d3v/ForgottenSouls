namespace NoCompromises.Content.Items;

internal class MirrorManaCost : GlobalItem
{
    private const int MANA_COST = 20;

    public override bool CanUseItem(Item item, Player player)
    {
        if (IsMirror(item.type))
        {
            if (player.statMana >= MANA_COST)
            {
                player.statMana -= MANA_COST;
                if (!Main.dedServ && player.whoAmI == Main.myPlayer)
                    player.manaRegenDelay = (int)player.maxRegenDelay;
                return true;
            }
            else
                return false;
        }

        return base.CanUseItem(item, player);
    }

    private static bool IsMirror(int type)
    {
        return
        type == ItemID.MagicMirror ||
        type == ItemID.IceMirror ||
        type == ItemID.CellPhone ||
        type == ItemID.Shellphone ||
        type == ItemID.ShellphoneOcean ||
        type == ItemID.ShellphoneHell;
    }
}