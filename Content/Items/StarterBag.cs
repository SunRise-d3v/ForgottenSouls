namespace ForgottenSouls.Content.Items;

internal sealed class StarterBag : ModItem
{
    public override string Texture => Helper.TextureLoad("Items", "StarterBag");

    private static readonly (int Type, int Stack)[] Weapons =
    [
        (ItemID.WandofSparking, 1),
        (ItemID.BabyBirdStaff, 1),
        (ItemID.WoodenArrow, 150)
    ];

    private static readonly (int Type, int Stack)[] Supplies =
    [
        (ItemID.WaterCandle, 1),
        (ItemID.Torch, 100),
        (ItemID.Rope, 50),
        (ItemID.Squirrel, 1),
        (ItemID.SilverCoin, 40)
    ];

    private static readonly (int Type, int Stack)[] FixedPotions =
    [
        (ItemID.RecallPotion, 10),
        (ItemID.HealingPotion, 15)
    ];

    private static readonly int[] RandomPotions =
    [
        ItemID.MiningPotion,
        ItemID.SpelunkerPotion,
        ItemID.ShinePotion
    ];

    private static readonly int[] IronGear =
    [
        ItemID.IronPickaxe, ItemID.IronAxe, ItemID.IronBroadsword, ItemID.IronBow
    ];

    private static readonly int[] LeadGear =
    [
        ItemID.LeadPickaxe, ItemID.LeadAxe, ItemID.LeadBroadsword, ItemID.LeadBow
    ];

    #region Mods Item

    #region Magic Storage
    private const string StorageUnit = "StorageUnit";
    private const string StorageHeart = "StorageHeart";
    private const string CraftingAccess = "CraftingAccess";
    #endregion

    #region Fargowiltas
    private const string AutoHouse = "AutoHouse";
    private const string MiniInstaBridge = "MiniInstaBridge";
    #endregion

    #region FargowiltasSouls
    private const string PuffInABottle = "PuffInABottle";
    private const string EternityAdvisor = "EternityAdvisor";
    #endregion

    #endregion

    public override void SetStaticDefaults()
    {
        ItemID.Sets.OpenableBag[Type] = true;
    }

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
        GiveAll(player, Weapons);
        GiveAll(player, Supplies);
        GivePotions(player);
        GiveOreGear(player);
        GiveStorage(player);
        GiveFargoItems(player);

        Give(player, ItemID.ManaCrystal, 1);
        Give(player, ItemID.Apple, 3);
        Give(player, ModContent.ItemType<FRankAdventurerEmblem>());
    }

    private void GivePotions(Player player)
    {
        GiveAll(player, FixedPotions);

        foreach (int potion in RandomPotions)
            Give(player, potion, Main.rand.Next(1, 3));
    }

    private void GiveOreGear(Player player)
    {
        int[] gear = WorldGen.SavedOreTiers.Iron == TileID.Iron ? IronGear : LeadGear;

        foreach (int item in gear)
            Give(player, item);
    }

    private void GiveStorage(Player player)
    {
        if (!ModCompat.MagicStorage)
        {
            Give(player, ItemID.Chest, 5);
            return;
        }

        GiveFromMods(player, 4, (Mods.MagicStorage, StorageUnit));
        GiveFromMods(player, 1, (Mods.MagicStorage, StorageHeart));
        GiveFromMods(player, 1, (Mods.MagicStorage, CraftingAccess));
    }

    private void GiveFargoItems(Player player)
    {
        GiveFromMods(player, 3, (Mods.Fargowiltas, AutoHouse));

        GiveFromMods(player, 2,
            (Mods.Fargowiltas, MiniInstaBridge),
            (Mods.Fargowiltas, MiniInstaBridge));

        GiveFromMods(player, 1,
            (Mods.FargowiltasSouls, PuffInABottle),
            (Mods.FargowiltasSouls, PuffInABottle));

        GiveFromMods(player, 1,
            (Mods.FargowiltasSouls, EternityAdvisor),
            (Mods.FargowiltasSouls, EternityAdvisor));
    }

    private void Give(Player player, int type, int stack = 1)
        => player.QuickSpawnItem(player.GetSource_OpenItem(Type), type, stack);

    private void GiveAll(Player player, (int Type, int Stack)[] items)
    {
        foreach ((int type, int stack) in items)
            Give(player, type, stack);
    }

    private void GiveFromMods(Player player, int stack, params (string Mod, string Name)[] candidates)
    {
        foreach ((string mod, string name) in candidates)
        {
            if (ModContent.TryFind(mod, name, out ModItem modItem))
            {
                Give(player, modItem.Type, stack);
                return;
            }
        }

        Mod.Logger.Warn($"StarterBag: предмет не найден ({string.Join(", ", candidates.Select(c => $"{c.Mod}/{c.Name}"))})");
    }
}