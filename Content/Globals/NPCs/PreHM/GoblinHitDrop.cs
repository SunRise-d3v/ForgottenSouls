namespace ForgottenSouls.Content.Globals;

internal sealed class GoblinHitDrop : GlobalNPC
{
    //private const float Chance = 0.44f;
    private const bool DropWholeStack = false;

    [Obsolete]
    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
    {
        if (!Pool.Goblins.Contains(npc.type) || target.dead)
            return;

        float chance = npc.type == NPCID.GoblinThief
             ? 1f
             : 0.20f;

        if (Main.rand.NextFloat() >= chance)
            return;

        List<int> slots = [];

        for (int i = 0; i < 50; i++)
        {
            Item item = target.inventory[i];

            if (!item.IsAir)
                slots.Add(i);
        }

        if (slots.Count == 0)
            return;

        int slot = Main.rand.Next(slots);
        var source = target.GetSource_Misc("GoblinHit");

        if (DropWholeStack || target.inventory[slot].stack <= 1)
        {
            target.DropItem(source, target.Center, ref target.inventory[slot]);
        }
        else
        {
            Item single = target.inventory[slot].Clone();
            single.stack = 1;
            target.inventory[slot].stack--;
            target.DropItem(source, target.Center, ref single);
        }
    }
}