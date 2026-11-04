namespace ForgottenSouls.Content.Items;

internal abstract class ModDrawing : ModItem
{
    protected abstract int UnlockItemID { get; }

    public override string Texture => Helper.TextureLoadItem("Drawing");

    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 28;

        Item.maxStack = 1;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(gold: 1);

        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.consumable = true;

        Item.noUseGraphic = true;
        Item.createTile = -1;
        Item.createWall = -1;
        Item.ammo = AmmoID.None;
    }

    public override bool CanUseItem(Player player)
    {
        return !player.GetModPlayer<DrawingPlayer>().IsUnlocked(UnlockItemID);
    }

    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<DrawingPlayer>().Unlock(UnlockItemID);
        return true;
    }

    public override bool CanRightClick() => false;
}