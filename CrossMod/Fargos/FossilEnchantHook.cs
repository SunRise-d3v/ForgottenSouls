using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FargowiltasSouls.Content.Items.Accessories.Enchantments;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Toggler;
using FargowiltasSouls.Core.Toggler.Content;

namespace ForgottenSouls.Content;

internal class FossilEnchantHook : ModSystem
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
        // orig() намеренно не вызываем: без FossilEffect возрождения нет
        player.AddEffect<FossilBones>(self.Item);
        player.AddEffect<FossilGuard>(self.Item);
        player.AddEffect<FossilPotions>(self.Item);
    }
}

/// <summary>Короткий иммунитет после получения урона (то, что делал FossilEffect, но без возрождения).</summary>
internal class FossilGuard : AccessoryEffect
{
    public override Header ToggleHeader => null;

    public override void OnHurt(Player player, Player.HurtInfo info)
    {
        player.immune = true;
        player.immuneTime = Math.Max(player.immuneTime, 60);
    }
}

/// <summary>
/// При использовании зелья восстановления шанс 25% сократить зельную болезнь вдвое и убрать усталость от зелий.
/// Логика - в PotionFatiguePlayer (PotionFatigue.cs).
/// </summary>
internal class FossilPotions : AccessoryEffect
{
    public const float Chance = 0.15f;

    public override Header ToggleHeader => Header.GetHeader<SpiritHeader>();
    public override int ToggleItemType => ModContent.ItemType<FossilEnchant>();
}