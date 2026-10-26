namespace ForgottenSouls.Content.ModPlayers;

internal sealed class GunJamPlayer : ModPlayer
{
    private const int ClickIntervalTicks = 20;

    private static readonly SoundStyle ClickSound = new SoundStyle(Helper.SoundLoad("", "GunClick")) with { Volume = 0.9f, Pitch = -0.3f };
    private static readonly SoundStyle JamSound = new SoundStyle(Helper.SoundLoad("", "JamSound")) with { Volume = 1f, Pitch = -0.7f };

    internal int JammedItemType = -1;

    private int _clickCooldown;

    public override void PostUpdate()
    {
        if (_clickCooldown > 0)
            _clickCooldown--;

        if (JammedItemType != -1 && !Player.HasBuff<JammedDebuff>())
            JammedItemType = -1;
    }

    internal bool IsJammed(Item item) => JammedItemType == item.type && Player.HasBuff<JammedDebuff>();

    internal void Jam(Item item, int ticks)
    {
        JammedItemType = item.type;
        Player.AddBuff(ModContent.BuffType<JammedDebuff>(), ticks);
        SoundEngine.PlaySound(JamSound, Player.Center);
    }

    internal void PlayClick()
    {
        if (_clickCooldown > 0)
            return;

        _clickCooldown = ClickIntervalTicks;
        SoundEngine.PlaySound(ClickSound, Player.Center);
    }
}

internal sealed class GunJamItem : GlobalItem
{
    private const float JamChance = 0.0065f;
    private const int JamMinTicks = 65;  
    private const int JamMaxTicks = 170; 

    private static bool UsesBullets(Item item) => item.damage > 0 && item.useAmmo == AmmoID.Bullet;

    public override bool CanUseItem(Item item, Player player)
    {
        if (!UsesBullets(item))
            return true;

        GunJamPlayer jam = player.GetModPlayer<GunJamPlayer>();

        if (!jam.IsJammed(item))
            return true;

        jam.PlayClick();
        return false;
    }

    public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        if (player.HeldItem.type == ModContent.ItemType<NavalRustrifle>() || !UsesBullets(item))
            return true;

        if (player.HasBuff<JammedDebuff>())
            return true;

        if (Main.rand.NextFloat() >= JamChance)
            return true;

        player.GetModPlayer<GunJamPlayer>().Jam(item, Main.rand.Next(JamMinTicks, JamMaxTicks + 1));
        return false;
    }
}