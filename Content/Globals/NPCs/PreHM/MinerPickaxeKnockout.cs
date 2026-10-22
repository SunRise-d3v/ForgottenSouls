namespace ForgottenSouls.Content;

internal class MinerPickaxeKnockout : GlobalNPC
{
    public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => npc.type == NPCID.UndeadMiner;

    [Obsolete]
    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
    {
        if (target.dead)
            return;

        int slot = -1;

        if (target.selectedItem < 50 && IsPickaxe(target.inventory[target.selectedItem]))
        {
            slot = target.selectedItem;
        }
        else
        {
            List<int> pickaxes = [];

            for (int i = 0; i < 50; i++)
            {
                if (IsPickaxe(target.inventory[i]))
                    pickaxes.Add(i);
            }

            if (pickaxes.Count > 0)
                slot = Main.rand.Next(pickaxes);
        }

        if (slot < 0)
            return;

        target.DropItem(target.GetSource_Misc("MinerKnockout"), target.Center, ref target.inventory[slot]);
    }

    private static bool IsPickaxe(Item item) => !item.IsAir && item.pick > 0;
}