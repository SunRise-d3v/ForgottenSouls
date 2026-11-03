namespace ForgottenSouls.Content.Mechanics;

internal abstract class ChallengeItem : ModItem
{
    protected abstract bool IsActive(Player player);
    protected abstract void Activate(Player player);

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
        Item.consumable = true;
        Item.UseSound = SoundID.Item4;
    }

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
    public override string Texture => Helper.TextureLoadItem("IronHeart");

    protected override string MessageKey => "Mods.ForgottenSouls.Messages.IronHeartActivated";

    protected override bool IsActive(Player player) => player.GetModPlayer<ChallengePlayer>().IronHeart;

    protected override void Activate(Player player) => player.GetModPlayer<ChallengePlayer>().IronHeart = true;
}

internal sealed class GlassSoul : ChallengeItem
{
    public override string Texture => Helper.TextureLoadItem("GlassSoul");

    protected override string MessageKey => "Mods.ForgottenSouls.Messages.GlassSoulActivated";

    protected override bool IsActive(Player player)
        => player.difficulty == PlayerDifficultyID.Hardcore || player.difficulty == PlayerDifficultyID.Creative;

    protected override void Activate(Player player)
    {
        player.difficulty = PlayerDifficultyID.Hardcore;

        if (Main.netMode == NetmodeID.MultiplayerClient)
            NetMessage.SendData(MessageID.PlayerInfo, -1, -1, null, player.whoAmI);
    }
}

internal sealed class UnbearablePain : ChallengeItem
{
    public override string Texture => Helper.TextureLoadItem("UnbearablePain");

    protected override string MessageKey => "Mods.ForgottenSouls.Messages.UnbearablePainActivated";

    protected override bool IsActive(Player player) => player.GetModPlayer<ChallengePlayer>().UnbearablePain;

    protected override void Activate(Player player) => player.GetModPlayer<ChallengePlayer>().UnbearablePain = true;
}