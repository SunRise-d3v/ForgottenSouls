namespace NoCompromises.Content.Global;

internal class ArmsDealerShop : ChangedShop
{
    protected override int NpcType => NPCID.ArmsDealer;

    protected override void ModifyShop(NPC npc, string shopName, Item[] items)
    {
        // После Глаза Ктулху — Мушкет и гробовщик
        if (NPC.downedBoss1)
        {
            AddItem(items, ItemID.Musket, priceInGold: 15);
            AddItem(items, ItemID.TheUndertaker, priceInGold: 15);
        }

        // После Пчелы — Обрез
        if (NPC.downedQueenBee)
            AddItem(items, ItemID.Boomstick, priceInGold: 25);

        // После Скелетрона — Пистолет
        if (NPC.downedBoss3)
            AddItem(items, ItemID.FlintlockPistol, priceInGold: 30);

        // После Стенки Плоти — Заводная штурмовая винтовка
        if (Main.hardMode)
            AddItem(items, ItemID.ClockworkAssaultRifle, priceInGold: 50);

        // После Плантеры — Боевой дробовик
        if (NPC.downedPlantBoss)
            AddItem(items, ItemID.TacticalShotgun, priceInGold: 75);

        // После всех трёх механиков — Узи
        if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)
            AddItem(items, ItemID.Uzi, priceInGold: 100);

        // После Голема — Снайперская винтовка
        if (NPC.downedGolemBoss)
            AddItem(items, ItemID.SniperRifle, priceInGold: 150);

        // После Скелетрона Прайма — Огнемёт
        if (NPC.downedMechBoss3)
            AddItem(items, ItemID.Flamethrower, priceInGold: 80);

        // Боеприпасы
        if (NPC.downedMechBossAny)
            AddItem(items, ItemID.CursedBullet, priceInGold: 999);

        if (NPC.downedPlantBoss)
            AddItem(items, ItemID.ChlorophyteBullet, priceInGold: 999);

        if (Main.hardMode)
            AddItem(items, ItemID.IchorBullet, priceInGold: 999);

        if (NPC.downedQueenSlime)
            AddItem(items, ItemID.CrystalBullet, priceInGold: 999);

        if (NPC.downedBoss2)
            AddItem(items, ItemID.SharkToothNecklace, priceInGold: 20);

        if (NPC.downedPlantBoss)
            AddItem(items, ItemID.SniperScope, priceInGold: 60);
    }
}