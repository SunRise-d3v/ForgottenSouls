namespace ForgottenSouls.Content.ModPlayers;

internal class LosingSouls : ModPlayer
{
    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        int totalSoulsLost = 0;

        int[] soulTypes =
        [
            ModContent.ItemType<Soul>()
        // Добавь другие типы душ
        ];

        for (int i = 0; i < Player.inventory.Length; i++)
        {
            Item item = Player.inventory[i];

            if (item != null && soulTypes.Contains(item.type) && item.stack > 0)
            {
                int amountToLose = (int)(item.stack * 0.1f);
                if (amountToLose < 1 && item.stack >= 10)
                    amountToLose = 1;

                if (amountToLose > 0)
                {
                    item.stack -= amountToLose;
                    totalSoulsLost += amountToLose;

                    Item.NewItem(Player.GetSource_Death(), Player.position, Player.width, Player.height, item.type, amountToLose);
                }
            }
        }

        if (totalSoulsLost > 0 && Main.netMode != NetmodeID.Server)
        {
            Color textColor = new Color(136, 231, 136);

            // Создание текста
            int index = CombatText.NewText(Player.Hitbox, textColor, LastLostSouls = totalSoulsLost);

            // Установка длительности текста: 60 фреймов = 1 секунда
            if (index >= 0 && index < Main.combatText.Length && Main.combatText[index] != null)
                Main.combatText[index].lifeTime = 60;
        }
    }

    public int LastLostSouls;

    public override void OnRespawn()
    {
        LastLostSouls = 0;
    }
}

internal sealed class DeathScreenSoulsSystem : ModSystem
{
    private const float TextScale = 0.6f;
    private const float OffsetBelowCenter = 50f; // raise / lower the text here
    private static readonly Color TextColor = new(136, 231, 136);

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");

        if (index < 0)
            return;

        layers.Insert(index, new LegacyGameInterfaceLayer(
            "ForgottenSouls: Death Screen Souls",
            () =>
            {
                Draw();
                return true;
            },
            InterfaceScaleType.UI));
    }

    private static void Draw()
    {
        Player player = Main.LocalPlayer;

        if (!player.dead)
            return;

        int lost = player.GetModPlayer<LosingSouls>().LastLostSouls;

        if (lost <= 0)
            return;

        // Key: Mods.ForgottenSouls.Messages.SoulsLost ({0} = amount of lost souls)
        string text = Language.GetTextValue("Mods.ForgottenSouls.Messages.SoulsLost", lost);

        Utils.DrawBorderStringBig(
            Main.spriteBatch,
            text,
            new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f + OffsetBelowCenter),
            TextColor,
            TextScale,
            0.5f,
            0f);
    }
}