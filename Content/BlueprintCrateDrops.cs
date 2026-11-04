namespace ForgottenSouls.Content.Globals;

internal sealed class BlueprintCrateDrops : GlobalItem
{
    private const float WoodenChance = 0.08f;
    private const float IronChance = 0.13f;
    private const float GoldenChance = 0.17f;
    private const float HardmodeChance = 0.20f;

    public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
    {
        float chance = GetCrateChance(item.type);

        if (chance <= 0f)
            return;

        itemLoot.Add(
            new BlueprintDropRule(
                chance,
                1,
                1
            )
        );
    }

    private static float GetCrateChance(int itemType)
    {
        if (itemType == ItemID.WoodenCrate
            || itemType == ItemID.WoodenCrateHard)
            return WoodenChance;

        if (itemType == ItemID.IronCrate
            || itemType == ItemID.IronCrateHard)
            return IronChance;

        if (itemType == ItemID.GoldenCrate
            || itemType == ItemID.GoldenCrateHard)
            return GoldenChance;

        if (Main.hardMode && IsHardmodeCrate(itemType))
            return HardmodeChance;

        return 0f;
    }

    private static bool IsHardmodeCrate(int itemType)
    {
        return itemType == ItemID.WoodenCrate
            || itemType == ItemID.IronCrate
            || itemType == ItemID.GoldenCrate
            || itemType == ItemID.CorruptFishingCrate
            || itemType == ItemID.CrimsonFishingCrate
            || itemType == ItemID.DungeonFishingCrate
            || itemType == ItemID.FloatingIslandFishingCrate
            || itemType == ItemID.HallowedFishingCrate
            || itemType == ItemID.JungleFishingCrate
            || itemType == ItemID.WoodenCrateHard
            || itemType == ItemID.IronCrateHard
            || itemType == ItemID.GoldenCrateHard
            || itemType == ItemID.CorruptFishingCrateHard
            || itemType == ItemID.CrimsonFishingCrateHard
            || itemType == ItemID.DungeonFishingCrateHard
            || itemType == ItemID.FloatingIslandFishingCrateHard
            || itemType == ItemID.HallowedFishingCrateHard
            || itemType == ItemID.JungleFishingCrateHard
            || itemType == ItemID.FrozenCrate
            || itemType == ItemID.FrozenCrateHard
            || itemType == ItemID.OasisCrate
            || itemType == ItemID.OasisCrateHard
            || itemType == ItemID.LavaCrate
            || itemType == ItemID.LavaCrateHard
            || itemType == ItemID.OceanCrate
            || itemType == ItemID.OceanCrateHard;
    }
}