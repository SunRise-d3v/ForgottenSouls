using Terraria.DataStructures;

internal class LifeCrystalTileExplosion : GlobalTile
{
    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (type == TileID.Heart && !fail && !effectOnly)
        {
            // 1. ПОЛНОСТЬЮ ОТКЛЮЧАЕМ ДРОП предмета Кристалла Жизни из сломанного блока
            noItem = true;

            // Выполняем спавн только на стороне сервера или в одиночной игре.
            // Это гарантирует, что появится строго ОДИН валун, а не по одному от каждого клиента.
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 spawnPosition = new(i * 16 + 16, j * 16 + 16);

                Projectile.NewProjectile(
                    new EntitySource_TileBreak(i, j),
                    spawnPosition,
                    new Vector2(0f, 0f),
                    ProjectileID.LifeCrystalBoulder,
                    100,
                    6f,
                    Main.myPlayer
                );
            }
        }
    }
}

internal class LifeCrystalBoulderDropBlocker : GlobalProjectile
{
    // Хук вызывается в самый последний момент жизни снаряда (когда он разбился)
    public override void Kill(Projectile projectile, int timeLeft)
    {
        // Проверяем, что разбился именно ванильный валун Кристалла Жизни
        if (projectile.type == ProjectileID.LifeCrystalBoulder)
        {
            // Поскольку валун спавнит предмет Кристалла Жизни внутри своего ванильного кода Kill(),
            // мы ищем в мире свежесозданные предметы прямо вокруг места крушения валуна
            foreach (Item item in Main.item)
            {
                // Если предмет существует, это Кристалл Жизни и он появился только что (время жизни совпадает с кадром)
                if (item != null && item.active && item.type == ItemID.LifeCrystal && item.timeSinceItemSpawned == 0)
                {
                    // Проверяем расстояние между точкой взрыва валуна и предметом
                    if (Vector2.Distance(projectile.Center, item.Center) < 50f)
                    {
                        // Полностью удаляем выпавший кристалл из мира
                        item.TurnToAir();
                    }
                }
            }
        }
    }
}