namespace ForgottenSouls.Content.Mechanics;

internal class WeightPlayer : ModPlayer
{
    public const float BaseLimit = 1000f;
    public const float HeavyMultiplier = 1.5f;     
    private const int RecalcInterval = 10;      
    private const int SlowTicks = 10;          

    public float Weight;
    public float ExtraLimit;

    public float Limit => BaseLimit + ExtraLimit;

    public bool Overloaded => Weight >= Limit;
    public bool Immobile => Weight >= Limit * HeavyMultiplier;

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

        if (Immobile)
            Player.velocity.X = 0f;
        else if (Overloaded)
            Player.AddBuff(BuffID.Slow, SlowTicks);
    }

    public override void SetControls()
    {
        if (!Immobile)
            return;

        Player.controlLeft = false;
        Player.controlRight = false;
        Player.controlJump = false;
    }

    public override void PostUpdateRunSpeeds()
    {
        if (Immobile)
        {
            Player.maxRunSpeed = 0f;
            Player.accRunSpeed = 0f;
            Player.runAcceleration = 0f;
        }
    }

    private float CalculateWeight()
    {
        float total = 0f;

        for (int i = 0; i < 58; i++)
            total += StackWeight(Player.inventory[i]);

        for (int i = 0; i < 10; i++)
            total += StackWeight(Player.armor[i]);

        return total;
    }

    private static float StackWeight(Item item)
        => item.IsAir ? 0f : item.stack * UnitWeight(item);

    public static float UnitWeight(Item item)
    {
        if (item.type >= ItemID.CopperCoin && item.type <= ItemID.PlatinumCoin)
            return 0f;

        // Патроны
        if (item.ammo > 0 && !item.notAmmo)
            return 0.05f;

        // Блоки и стены
        if (item.createTile >= TileID.Dirt || item.createWall >= 0)
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

internal class WeightSystem : ModSystem
{
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name == "Vanilla: Inventory");

        if (index < 0)
            return;

        layers.Insert(index + 1, new LegacyGameInterfaceLayer("ForgottenSouls: Inventory Weight", DrawWeight, InterfaceScaleType.UI));
    }

    private static bool DrawWeight()
    {
        if (Main.gameMenu || !Main.playerInventory)
            return true;

        WeightPlayer weight = Main.LocalPlayer.GetModPlayer<WeightPlayer>();
        string text = $"Вес: {(int)weight.Weight}/{(int)weight.Limit}";

        float trashX = 20f + 9f * 56f * Main.inventoryScale;
        float trashY = 20f + 5f * 56f * Main.inventoryScale;
        float slotSize = 52f * Main.inventoryScale;

        var font = FontAssets.MouseText.Value;
        Vector2 size = font.MeasureString(text);

        Vector2 position = new(trashX - 8f - size.X, trashY + (slotSize - size.Y) / 2f);

        Main.spriteBatch.DrawString(font, text, position, Color.White);

        return true;
    }
}
