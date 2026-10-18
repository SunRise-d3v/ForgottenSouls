using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoCompromises.Content.Globals;

public class HideEmptyHeartsOverlay : ModResourceOverlay
{
    // Метод срабатывает ПЕРЕД отрисовкой каждого отдельного сердечка (или его рамки)
    public override bool PreDrawResource(ResourceOverlayDrawContext context)
    {
        // Проверяем, что игра пытается нарисовать именно полоску ЗДОРОВЬЯ (а не маны)
        if (context.resourceNumber >= 0)
        {
            Player player = Main.LocalPlayer;

            // Если у игрока сейчас заниженное здоровье до 50 HP (то есть до 2.5 сердец)
            if (player.statLifeMax == 50)
            {
                // Каждое сердце вмещает 20 единиц HP.
                // context.resourceNumber — это порядковый номер контейнера на экране (0, 1, 2, 3...)
                // Первое сердце = 0, второе = 1, третье (половинка) = 2.

                // Если игра пытается нарисовать контейнеры, идущие ПОСЛЕ третьего сердца (номер 3 и выше)
                if (context.resourceNumber >= 3)
                {
                    // Возвращаем false, полностью запрещая игре отрисовывать этот элемент (и его серую рамку тоже)
                    return false;
                }
            }
        }

        // Разрешаем отрисовку для первых 2.5 сердец или если у игрока другое здоровье
        return true;
    }
}