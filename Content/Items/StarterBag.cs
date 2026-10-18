namespace NoCompromises.Content.Items;

internal sealed class StarterBag : ModItem
{
    public override void SetDefaults()
    {
        Item.maxStack = 9999;
        Item.consumable = true;

        Item.width = 32;
        Item.height = 32;

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 75);
    }

    public override bool CanRightClick() => true;

    public override void RightClick(Player player)
    {
        base.RightClick(player);

        //Weapon
        player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.WandofSparking, 1);
        player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.BabyBirdStaff, 1);
        //Additionally
        player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.WoodenArrow, 100);
        player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.ManaCrystal, Main.rand.Next(1, 3));
        //Potion
        player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.MiningPotion, Main.rand.Next(1, 3));
        player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.SpelunkerPotion, Main.rand.Next(1, 3));
        player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.ShinePotion, Main.rand.Next(1, 3));
        player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.RecallPotion, 5);
        //Items
        player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.Chest, 3);
        player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.Torch, 25);
        player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.Bomb, 10);
        //If there is silver in the world
        if (ItemID.Sets.Factory.CreateBoolSet(ItemID.SilverBar)[ItemID.SilverBar])
        {
            //Tools
            player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.SilverPickaxe, 1);
            player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.SilverAxe, 1);
            //Weapon
            player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.SilverBroadsword, 1);
            player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.SilverBow, 1);
        }
        else if (ItemID.Sets.Factory.CreateBoolSet(ItemID.TungstenBar)[ItemID.TungstenBar])
        {
            //Tools
            player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.TungstenPickaxe, 1);
            player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.TungstenAxe, 1);
            //Weapon
            player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.TungstenBroadsword, 1);
            player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.TungstenBow, 1);
        }
        
        //Accessories
        //player.QuickSpawnItem(player.GetSource_DropAsItem(), ModContent.ItemType<FRankAdventurerEmblem>(), 1);
        //Random Accessories
        int randomNumber = Main.rand.Next(1, 6);
        switch (randomNumber)
        {
            case 1:
                player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.ClimbingClaws, 1);
                break;
            case 2:
                player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.AnkletoftheWind, 1);
                break;
            case 3:
                player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.Aglet, 1);
                break;
            case 4:
                player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.HandWarmer, 1);
                break;
            case 5:
                player.QuickSpawnItem(player.GetSource_DropAsItem(), ItemID.Shackle, 1);
                break;
        }
    }
}