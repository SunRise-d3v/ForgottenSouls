using Terraria.Audio;

namespace ForgottenSouls.Content;

/// <summary>
/// Каждый удар монстра по игроку телом (контактный урон, без снарядов) с шансом 0.5%
/// «выбивает оружие из рук»: хотбар переключается на другой случайный слот.
/// </summary>
internal class DisarmPlayer : ModPlayer
{
    public const float DisarmChance = 0.005f; // 0.5%
    public const int HotbarSlots = 10;
    public const int WeaponDamageThreshold = 15; // предмет с уроном выше этого считается оружием

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
    {
        TryDisarm();
    }

    private void TryDisarm()
    {
        // Решение принимает клиент самого игрока: selectedItem синхронизируется оттуда
        if (Player.whoAmI != Main.myPlayer || Main.rand.NextFloat() >= DisarmChance)
            return;

        // Подходят только слоты, где пусто или лежит не оружие (урон <= 15)
        List<int> slots = [];
        for (int s = 0; s < HotbarSlots; s++)
        {
            Item item = Player.inventory[s];
            if (s != Player.selectedItem && (item.IsAir || item.damage <= WeaponDamageThreshold))
                slots.Add(s);
        }

        // Все остальные слоты заняты оружием - выбить нечем
        if (slots.Count == 0)
            return;

        Player.selectedItem = slots[Main.rand.Next(slots.Count)];

        // Прерываем текущее использование предмета
        Player.itemAnimation = 0;
        Player.itemTime = 0;

        Main.NewText("Оружие выбито!", Color.OrangeRed);
        SoundEngine.PlaySound(SoundID.Item7, Player.Center);
    }
}