namespace ForgottenSouls.Content;

internal class GrenadeNerfItem : GlobalItem
{
#if true
    public const float DamageMultiplier = 0.4f;

    private static readonly HashSet<int> VanillaGrenades =
    [
        ItemID.Bomb,
        ItemID.BombFish,
        ItemID.BouncyBomb,
        ItemID.BouncyDynamite,
        ItemID.DirtBomb,
        ItemID.DryBomb,
        ItemID.Dynamite,
        ItemID.HoneyBomb,
        ItemID.LavaBomb,
        ItemID.ScarabBomb,
        ItemID.StickyBomb,
        ItemID.DirtStickyBomb,
        ItemID.StickyDynamite,
        ItemID.WetBomb,
        ItemID.PartyGirlGrenade,
        ItemID.Grenade,
        ItemID.StickyGrenade,
        ItemID.BouncyGrenade
    ];

    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        => lateInstantiation && IsGrenade(entity);

    private static bool IsGrenade(Item item)
    {
        if (VanillaGrenades.Contains(item.type))
            return true;

        return item.ModItem != null && item.ModItem.Name.Contains("Grenade", StringComparison.OrdinalIgnoreCase);
    }

    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
    {
        damage *= DamageMultiplier;
    }
#endif
}