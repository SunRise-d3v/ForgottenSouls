namespace DisasterMod.Content.Items;

internal sealed class AcornAmmo : ModSystem
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