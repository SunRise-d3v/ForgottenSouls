namespace NoCompromises.Content.Globals.Items;

internal class SpeedingUpItemUsage : GlobalItem
{
    public override void SetDefaults(Item item)
    {
        if (item.pick > 0 || item.axe > 0 || item.hammer > 0)
            item.useTime = (int)(item.useTime * 0.81f);
    }
}