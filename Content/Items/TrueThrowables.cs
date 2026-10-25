namespace ForgottenSouls.Content;

internal static class TrueThrowables
{
    public const int CraftCost = 500;  
    public const float DamageMult = 1.25f;

    public static readonly (string Name, int BaseType, string RuName)[] List =
    [
        ("TrueSnowball",       ItemID.Snowball,       "Истинный снежок"),
        ("TrueShuriken",       ItemID.Shuriken,       "Истинный сюрикен"),
        ("TrueRottenEgg",      ItemID.RottenEgg,      "Истинное тухлое яйцо"),
        ("TrueThrowingKnife",  ItemID.ThrowingKnife,  "Истинный метательный нож"),
        ("TruePoisonedKnife",  ItemID.PoisonedKnife,  "Истинный отравленный нож"),
        ("TrueBoneDagger",     ItemID.BoneDagger,     "Истинный костяной метательный нож"),
        ("TrueStarAnise",      ItemID.StarAnise,      "Истинный бадьян"),
        ("TrueJavelin",        ItemID.Javelin,        "Истинное метательное копьё"),
        ("TrueBone",           ItemID.Bone,           "Истинная кость"),
        ("TrueMolotovCocktail",ItemID.MolotovCocktail,"Истинный коктейль Молотова"),
        ("TrueBoneJavelin",    ItemID.BoneJavelin,    "Истинное костяное метательное копьё"),
        ("TrueDaggerFish",     ItemID.FrostDaggerfish,"Истинная метательная рыба-кинжал")
    ];
}

internal class TrueThrowableLoader : ILoadable
{
    public void Load(Mod mod)
    {
        foreach (var (name, baseType, ruName) in TrueThrowables.List)
            mod.AddContent(new TrueThrowable(name, baseType, ruName));
    }

    public void Unload() { }
}

[Autoload(false)]
internal class TrueThrowable(string internalName, int baseType, string ruName) : ModItem
{
    private readonly string internalName = internalName;
    private readonly int baseType = baseType;
    private readonly string ruName = ruName;

    protected override bool CloneNewInstances => true;

    public override string Name => internalName;

    public override string Texture => "Terraria/Images/Item_" + baseType;

    public override void SetDefaults()
    {
        Item.CloneDefaults(baseType);

        Item.consumable = false;
        Item.maxStack = 1;

        Item.ammo = AmmoID.None;
        Item.notAmmo = true;
        Item.damage = (int)(Item.damage * TrueThrowables.DamageMult);
        Item.rare = Math.Max(Item.rare, ItemRarityID.Orange);
        Item.value = Item.sellPrice(gold: 1);

        Item.SetNameOverride(ruName);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "TrueThrowable", "Не расходуется"));
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(baseType, TrueThrowables.CraftCost)
            .AddTile(TileID.Anvils)
            .Register();
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D silhouette = TrueThrowableSilhouettes.Get(Item.type);
        if (silhouette == null)
            return true;

        float pulse = 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
        Color outline = Color.Lerp(new Color(255, 190, 30), new Color(255, 245, 160), pulse);

        for (int ring = 1; ring <= 2; ring++)
        {
            float dist = TrueThrowableSilhouettes.OutlineThickness * ring * scale;
            for (int i = 0; i < 8; i++)
            {
                Vector2 dir = (MathHelper.PiOver4 * i).ToRotationVector2() * dist;
                Color ringColor = outline * (ring == 1 ? 0.3f : 0.1f);
                spriteBatch.Draw(silhouette, position + dir, frame, ringColor, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        return true;
    }
}

internal class TrueThrowableSilhouettes : ModSystem
{
    public const float OutlineThickness = 1.1f; //default 1.5

    private static readonly Dictionary<int, Texture2D> cache = [];

    public static Texture2D Get(int itemType)
    {
        if (cache.TryGetValue(itemType, out Texture2D tex))
            return tex;

        Texture2D src = TextureAssets.Item[itemType].Value;
        Color[] data = new Color[src.Width * src.Height];
        src.GetData(data);

        for (int i = 0; i < data.Length; i++)
            data[i] = data[i].A > 20 ? Color.White : Color.Transparent;

        tex = new Texture2D(Main.graphics.GraphicsDevice, src.Width, src.Height);
        tex.SetData(data);
        cache[itemType] = tex;
        return tex;
    }

    public override void Unload()
    {
        Main.QueueMainThreadAction(() =>
        {
            foreach (Texture2D tex in cache.Values)
                tex.Dispose();
            cache.Clear();
        });
    }
}