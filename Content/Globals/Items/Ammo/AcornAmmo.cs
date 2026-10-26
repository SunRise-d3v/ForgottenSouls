namespace ForgottenSouls.Content.Items;

internal sealed class AcornAmmoSystem : ModSystem
{
    public override void Load()
    {
        On_Item.FitsAmmoSlot += FitsAmmoSlotDetour;
    }

    public override void Unload()
    {
        On_Item.FitsAmmoSlot -= FitsAmmoSlotDetour;
    }

    private static bool FitsAmmoSlotDetour(On_Item.orig_FitsAmmoSlot orig, Item self)
    {
        return self.type == ItemID.Acorn || orig(self);
    }
}

/*internal class AcornAmmo : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.Acorn;

    public override void SetDefaults(Item item)
    {
        item.ammo = ItemID.Acorn;
    }
}*/