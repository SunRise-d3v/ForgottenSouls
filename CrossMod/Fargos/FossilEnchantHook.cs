namespace CrossMod;

internal sealed class FossilEnchantHook : ModSystem
{
    private delegate void OrigUpdateAccessory(FossilEnchant self, Player player, bool hideVisual);

    public override void Load()
    {
        MethodInfo method = typeof(FossilEnchant).GetMethod(
            nameof(FossilEnchant.UpdateAccessory),
            BindingFlags.Instance | BindingFlags.Public,
            null,
            [typeof(Player), typeof(bool)],
            null);

        if (method != null)
            MonoModHooks.Add(method, UpdateAccessoryHook);
        else
            Mod.Logger.Warn("FossilEnchant.UpdateAccessory не найден: хук не установлен");
    }

    private static void UpdateAccessoryHook(OrigUpdateAccessory orig, FossilEnchant self, Player player, bool hideVisual)
    {
        // orig()
        player.AddEffect<FossilBones>(self.Item);
        player.AddEffect<FossilGuard>(self.Item);
        player.AddEffect<FossilPotions>(self.Item);
    }
}

internal class FossilGuard : AccessoryEffect
{
    public override Header ToggleHeader => null;

    public override void OnHurt(Player player, Player.HurtInfo info)
    {
        player.immune = true;
        player.immuneTime = Math.Max(player.immuneTime, 60);
    }
}

internal sealed class FossilPotions : AccessoryEffect
{
    public const float Chance = 0.15f;

    public override Header ToggleHeader => Header.GetHeader<SpiritHeader>();
    public override int ToggleItemType => ModContent.ItemType<FossilEnchant>();
}