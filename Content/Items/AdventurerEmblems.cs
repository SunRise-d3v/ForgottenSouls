namespace ForgottenSouls.Content.Items;

internal abstract class AdventurerEmblems : ModItem
{
    public override string Texture => null;
    protected virtual int Buff => -1;
    protected virtual int SellPrice => -1;
    protected virtual int Rarity => -1;

    public override void SetDefaults()
    {
        Item.maxStack = 1;
        Item.consumable = false;
        Item.width = 16;
        Item.height = 16;
        Item.rare = Rarity;

        Item.accessory = false;

        Item.value = Item.sellPrice(silver: SellPrice);
    }

    public override void UpdateInventory(Player player)
    {
        player.AddBuff(Buff, 60);
    }
}

internal sealed class FRankAdventurerEmblem : AdventurerEmblems
{
    protected override int Rarity => 0;
    protected override int SellPrice => 1;

    public override string Texture => Helper.TextureLoadItem("FRankAdventurerEmblem");
    protected override int Buff => ModContent.BuffType<HeroVillage1>();
}

internal sealed class CRankAdventurerEmblem : AdventurerEmblems
{
    protected override int Rarity => 3;
    protected override int SellPrice => 50;

    public override string Texture => Helper.TextureLoadItem("CRankAdventurerEmblem");
    protected override int Buff => ModContent.BuffType<HeroVillage2>();
}

internal sealed class BRankAdventurerEmblem : AdventurerEmblems
{
    protected override int Rarity => 4;
    protected override int SellPrice => 1 * 100;

    public override string Texture => Helper.TextureLoadItem("BRankAdventurerEmblem");
    protected override int Buff => ModContent.BuffType<HeroVillage3>();
}

internal sealed class ARankAdventurerEmblem : AdventurerEmblems
{
    protected override int Rarity => 5;
    protected override int SellPrice => 10 * 100;

    public override string Texture => Helper.TextureLoadItem("ARankAdventurerEmblem");
    protected override int Buff => ModContent.BuffType<HeroVillage4>();
}