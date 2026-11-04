namespace ForgottenSouls.Content.Globals;

internal sealed class BlueprintBagDrops : GlobalItem
{
    public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
    {
        if (!ItemID.Sets.BossBag[item.type])
            return;

        itemLoot.Add(
            new BlueprintDropRule(
                1f,
                1,
                3
            )
        );
    }
}