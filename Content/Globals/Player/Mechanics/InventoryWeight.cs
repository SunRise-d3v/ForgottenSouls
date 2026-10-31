using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ForgottenSouls.Content;

// =====================================================================
//  Баффы перевеса
// =====================================================================

/// <summary>Лёгкий перевес: вес достиг лимита, игрок замедлен. Иконка 32x32: Content/Buffs/LightOverburden.png</summary>
internal class LightOverburden : ModBuff
{
    public override string Texture => Helper.TextureLoadBuff("Debuffs","LightOverburden");

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;   // перевес не снимается лечением
    }
}

/// <summary>Сильный перевес: вес достиг полуторного лимита, игрок не может сдвинуться с места. Иконка 32x32: Content/Buffs/HeavyOverburden.png</summary>
internal class HeavyOverburden : ModBuff
{
    public override string Texture => Helper.TextureLoadBuff("Debuffs", "HeavyOverburden");

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }
}

// =====================================================================
//  Вес инвентаря
// =====================================================================

/// <summary>
/// Вес инвентаря. Вес считается по предметам, которые игрок носит (инвентарь, патроны, надетая броня и аксессуары).
/// Достиг лимита: бафф «Лёгкий перевес» (замедление). Достиг лимита в полтора раза: бафф «Сильный перевес» (игрок не может сдвинуться).
/// </summary>
internal class WeightPlayer : ModPlayer
{
    public const float BaseLimit = 500f;
    public const float HeavyMultiplier = 1.5f;      // во сколько раз лимит должен быть превышен для сильного перевеса
    private const int RecalcInterval = 10;           // как часто пересчитываем вес (раз в 10 тиков)
    private const int BuffTicks = 10;                // бафф продлевается каждый тик и пропадает почти сразу после разгрузки
    private const float LightSpeedMultiplier = 0.6f; // скорость бега при лёгком перевесе

    public float Weight;

    /// <summary>Дополнительный лимит от префиксов на броне и аксессуарах (сбрасывается каждый тик и набирается заново).</summary>
    public float ExtraLimit;

    public float Limit => BaseLimit + ExtraLimit;

    private int _timer;

    public override void ResetEffects()
    {
        ExtraLimit = 0f;
    }

    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        if (++_timer >= RecalcInterval)
        {
            _timer = 0;
            Weight = CalculateWeight();
        }

        int light = ModContent.BuffType<LightOverburden>();
        int heavy = ModContent.BuffType<HeavyOverburden>();

        // Баффы взаимоисключающие: на игроке всегда не больше одного из них
        if (Weight >= Limit * HeavyMultiplier)
        {
            Player.AddBuff(heavy, BuffTicks);
            Player.ClearBuff(light);
        }
        else if (Weight >= Limit)
        {
            Player.AddBuff(light, BuffTicks);
            Player.ClearBuff(heavy);
        }
        else
        {
            Player.ClearBuff(light);
            Player.ClearBuff(heavy);
        }

        // Сильный перевес: игрок стоит на месте
        if (Player.HasBuff<HeavyOverburden>())
            Player.velocity.X = 0f;
    }

    // Сильный перевес: нельзя ни бегать, ни прыгать
    public override void SetControls()
    {
        if (!Player.HasBuff<HeavyOverburden>())
            return;

        Player.controlLeft = false;
        Player.controlRight = false;
        Player.controlJump = false;
    }

    public override void PostUpdateRunSpeeds()
    {
        if (Player.HasBuff<HeavyOverburden>())
        {
            Player.maxRunSpeed = 0f;
            Player.accRunSpeed = 0f;
            Player.runAcceleration = 0f;
        }
        else if (Player.HasBuff<LightOverburden>())
        {
            Player.maxRunSpeed *= LightSpeedMultiplier;
            Player.accRunSpeed *= LightSpeedMultiplier;
            Player.runAcceleration *= LightSpeedMultiplier;
        }
    }

    // ---------- Подсчёт веса ----------

    private float CalculateWeight()
    {
        float total = 0f;

        // Инвентарь вместе со слотами монет и патронов
        for (int i = 0; i < 58; i++)
            total += StackWeight(Player.inventory[i]);

        // Надетая броня и аксессуары (слоты 0-9, без косметики)
        for (int i = 0; i < 10; i++)
            total += StackWeight(Player.armor[i]);

        return total;
    }

    private static float StackWeight(Item item)
        => item.IsAir ? 0f : item.stack * UnitWeight(item);

    /// <summary>Вес одной штуки предмета. Значения можно менять под баланс.</summary>
    public static float UnitWeight(Item item)
    {
        // Души ничего не весят
        if (item.type == ModContent.ItemType<Soul>())
            return 0f;

        // Мешки с боссов весят ровно 50 каждый
        if (ItemID.Sets.BossBag[item.type])
            return 50f;

        // Деньги ничего не весят
        if (item.type >= ItemID.CopperCoin && item.type <= ItemID.PlatinumCoin)
            return 0f;

        // Патроны
        if (item.ammo > 0 && !item.notAmmo)
            return 0.05f;

        // Блоки и стены
        if (item.createTile >= 0 || item.createWall >= 0)
            return 0.1f;

        // Броня
        if (item.headSlot >= 0 || item.bodySlot >= 0 || item.legSlot >= 0)
            return 6f;

        // Аксессуары
        if (item.accessory)
            return 2f;

        // Инструменты
        if (item.pick > 0 || item.axe > 0 || item.hammer > 0)
            return 4f;

        // Зелья и расходники
        if (item.consumable && (item.buffType > 0 || item.healLife > 0 || item.healMana > 0))
            return 0.5f;

        // Оружие: стопки метательного лёгкие, остальное тяжёлое
        if (item.damage > 0)
            return item.maxStack > 1 ? 0.3f : 5f;

        // Материалы для крафта
        if (item.material)
            return 0.2f;

        return 0.5f;
    }
}

// =====================================================================
//  Надпись «вес/лимит» под инвентарём
// =====================================================================

internal class WeightSystem : ModSystem
{
    private const float TextOffsetY = 8f;   // насколько надпись опущена ниже середины слота корзины

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name == "Vanilla: Inventory");

        if (index < 0)
            return;

        layers.Insert(index + 1, new LegacyGameInterfaceLayer("ForgottenSouls: Inventory Weight", DrawWeight, InterfaceScaleType.UI));
    }

    private static bool DrawWeight()
    {
        // Показываем только когда открыт инвентарь
        if (Main.gameMenu || !Main.playerInventory)
            return true;

        WeightPlayer weight = Main.LocalPlayer.GetModPlayer<WeightPlayer>();
        string text = $"{(int)weight.Weight}/{(int)weight.Limit}";

        // Слот корзины стоит под последней колонкой инвентаря: слева от него и находится надпись
        float trashX = 20f + 9f * 56f * Main.inventoryScale;
        float trashY = 20f + 5f * 56f * Main.inventoryScale;
        float slotSize = 52f * Main.inventoryScale;

        var font = FontAssets.MouseText.Value;
        Vector2 size = font.MeasureString(text);

        // Прижимаем к левому краю корзины, по высоте ниже середины слота
        Vector2 position = new(trashX - 8f - size.X, trashY + (slotSize - size.Y) / 2f + TextOffsetY);

        Main.spriteBatch.DrawString(font, text, position, Color.White);

        return true;
    }
}