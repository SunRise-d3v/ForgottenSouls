using ForgottenSouls.Content.Rarities;

namespace ForgottenSouls.Content.Mechanics;

/// <summary>State of the three "challenge" activations. Saved with the character.</summary>
internal sealed class ChallengePlayer : ModPlayer
{
    /// <summary>Iron Heart: no regeneration and no healing of any kind.</summary>
    public bool IronHeart;

    /// <summary>Unbearable Pain: maximum health is 1.</summary>
    public bool UnbearablePain;

    private int lastLife = -1;

    public override void SaveData(TagCompound tag)
    {
        if (IronHeart)
            tag["IronHeart"] = true;

        if (UnbearablePain)
            tag["UnbearablePain"] = true;
    }

    public override void LoadData(TagCompound tag)
    {
        IronHeart = tag.ContainsKey("IronHeart");
        UnbearablePain = tag.ContainsKey("UnbearablePain");
    }

    public override void OnEnterWorld() => lastLife = -1;

    public override void OnRespawn() => lastLife = -1;

    // ---- Iron Heart ----

    // Natural regeneration is switched off after the game has calculated it
    public override void UpdateLifeRegen()
    {
        if (!IronHeart)
            return;

        if (Player.lifeRegen > 0)
            Player.lifeRegen = 0;
    }

    // Healing items (potions, quick heal) restore nothing
    public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
    {
        if (IronHeart)
            healValue = 0;
    }

    // Safety net for every other source of healing (hearts, life steal, other mods...):
    // life is never allowed to go up from one tick to the next
    public override void PostUpdate()
    {
        if (!IronHeart || Player.dead)
        {
            lastLife = -1;
            return;
        }

        if (lastLife < 0)
            lastLife = Player.statLife;

        if (Player.statLife > lastLife)
            Player.statLife = lastLife;

        lastLife = Player.statLife;
    }

    // ---- Unbearable Pain ----

    // Set after the equipment has been applied, so it also overrides other max-health effects
    public override void PostUpdateEquips()
    {
        if (!UnbearablePain)
            return;

        Player.statLifeMax2 = 1;

        if (Player.statLife > 1)
            Player.statLife = 1;
    }
}

/// <summary>Base for the items that switch a challenge on when used.</summary>
internal abstract class ChallengeItem : ModItem
{
    protected abstract bool IsActive(Player player);
    protected abstract void Activate(Player player);

    // Key of the chat message shown after activation
    protected abstract string MessageKey { get; }

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.rare = ModContent.RarityType<ChallengeRarity>();
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 40;
        Item.useAnimation = 40;
        Item.consumable = true;   // the item is spent when the challenge is activated
        Item.UseSound = SoundID.Item4;
    }

    // Nothing to activate twice
    public override bool CanUseItem(Player player) => !IsActive(player);

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            Activate(player);
            Main.NewText(Language.GetTextValue(MessageKey), Color.OrangeRed);
        }

        return true;
    }
}

internal sealed class IronHeart : ChallengeItem
{
    // Placeholder: swap for your own texture
    public override string Texture => "Terraria/Images/Item_" + ItemID.Heart;

    protected override string MessageKey => "Mods.ForgottenSouls.Messages.IronHeartActivated";

    protected override bool IsActive(Player player) => player.GetModPlayer<ChallengePlayer>().IronHeart;

    protected override void Activate(Player player) => player.GetModPlayer<ChallengePlayer>().IronHeart = true;
}

internal sealed class GlassSoul : ChallengeItem
{
    // Placeholder: swap for your own texture
    public override string Texture => "Terraria/Images/Item_" + ItemID.Tombstone;

    protected override string MessageKey => "Mods.ForgottenSouls.Messages.GlassSoulActivated";

    // Already hardcore, or a Journey character (which has no death)
    protected override bool IsActive(Player player)
        => player.difficulty == PlayerDifficultyID.Hardcore || player.difficulty == PlayerDifficultyID.Creative;

    protected override void Activate(Player player)
    {
        player.difficulty = PlayerDifficultyID.Hardcore;

        // Other players must learn about the change too
        if (Main.netMode == NetmodeID.MultiplayerClient)
            NetMessage.SendData(MessageID.PlayerInfo, -1, -1, null, player.whoAmI);
    }
}

internal sealed class UnbearablePain : ChallengeItem
{
    // Placeholder (a stub item): swap for your own texture
    public override string Texture => "Terraria/Images/Item_" + ItemID.GuideVoodooDoll;

    protected override string MessageKey => "Mods.ForgottenSouls.Messages.UnbearablePainActivated";

    protected override bool IsActive(Player player) => player.GetModPlayer<ChallengePlayer>().UnbearablePain;

    protected override void Activate(Player player) => player.GetModPlayer<ChallengePlayer>().UnbearablePain = true;
}