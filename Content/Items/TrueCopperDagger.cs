namespace ForgottenSouls.Content;

internal class TrueCopperDagger : ModItem
{
    public override string Texture => "Terraria/Images/Item_" + ItemID.CopperShortsword;
#if false
    private const int DaggerCount = 6;    
    private const int StaggerTicks = 3;    
    private const float OutlineThickness = 2f; 

    private static Texture2D _silhouette;

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.damage = 42;         
        Item.DamageType = DamageClass.Melee;
        Item.knockBack = 3f;
        Item.crit = 4;
        Item.useTime = 24;
        Item.useAnimation = 24;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noMelee = true;           
        Item.autoReuse = true;
        Item.UseSound = SoundID.Item1;
        Item.shoot = ModContent.ProjectileType<TrueCopperDaggerProjectile>();
        Item.shootSpeed = 10f;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.sellPrice(gold: 2);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.CopperShortsword)
            .AddIngredient(ItemID.BrokenHeroSword)
            .AddIngredient(ItemID.CopperBar, 10)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Vector2 target = Main.MouseWorld;

        for (int i = 0; i < DaggerCount; i++)
        {
            float angle = -MathHelper.PiOver2 + (i - (DaggerCount - 1) / 2f) * 0.55f + Main.rand.NextFloat(-0.15f, 0.15f);
            Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(70f, 130f);

            Projectile.NewProjectile(source, player.Center + offset, offset, type, damage, knockback, player.whoAmI, target.X, target.Y, i * StaggerTicks);
        }

        return false;
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D silhouette = GetSilhouette(TextureAssets.Item[Item.type].Value);

        float pulse = 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f);
        Color outline = Color.Lerp(new Color(255, 190, 30), new Color(255, 245, 160), pulse);

        for (int ring = 1; ring <= 2; ring++)
        {
            float dist = OutlineThickness * ring * scale;

            for (int i = 0; i < 8; i++)
            {
                Vector2 dir = (MathHelper.PiOver4 * i).ToRotationVector2() * dist;
                Color ringColor = outline * (ring == 1 ? 0.3f : 0.1f);

                spriteBatch.Draw(silhouette, position + dir, frame, ringColor, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        return true;
    }

    private static Texture2D GetSilhouette(Texture2D source)
    {
        if (_silhouette != null)
            return _silhouette;

        Color[] pixels = new Color[source.Width * source.Height];
        source.GetData(pixels);

        for (int i = 0; i < pixels.Length; i++)
        {
            int a = pixels[i].A;
            pixels[i] = new Color(a, a, a, a);
        }

        _silhouette = new Texture2D(Main.graphics.GraphicsDevice, source.Width, source.Height);
        _silhouette.SetData(pixels);

        return _silhouette;
    }

    public override void Unload()
    {
        Texture2D silhouette = _silhouette;
        _silhouette = null;

        if (silhouette != null)
            Main.QueueMainThreadAction(silhouette.Dispose);
    }
}

internal class TrueCopperDaggerProjectile : ModProjectile
{
    public override string Texture => "Terraria/Images/Item_" + ItemID.CopperShortsword;

    private const int HoverTicks = 14;   
    private const int DashTicks = 36;         
    private const float StartSpeed = 14f;
    private const float EndSpeed = 34f;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 12;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
        Projectile.timeLeft = 300;
        Projectile.alpha = 255;   
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.localAI[1] = Projectile.velocity.X;
        Projectile.localAI[2] = Projectile.velocity.Y;
        Projectile.velocity = Vector2.Zero;
    }

    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        Vector2 target = new(Projectile.ai[0], Projectile.ai[1]);
        int delay = (int)Projectile.ai[2];

        float timer = ++Projectile.localAI[0];
        int dashStart = HoverTicks + delay;

        if (timer < dashStart)
        {
            Vector2 offset = new(Projectile.localAI[1], Projectile.localAI[2]);
            Projectile.Center = Vector2.Lerp(Projectile.Center, owner.Center + offset, 0.35f);
            Projectile.velocity = Vector2.Zero;

            Vector2 aim = (target - Projectile.Center).SafeNormalize(Vector2.UnitX);
            Projectile.rotation = aim.ToRotation() + MathHelper.PiOver4;
            Projectile.alpha = Math.Max(0, Projectile.alpha - 40);
            return;
        }

        if (timer == dashStart)
        {
            Projectile.velocity = (target - Projectile.Center).SafeNormalize(Vector2.UnitX) * StartSpeed;

            if (delay == 0)
                SoundEngine.PlaySound(SoundID.Item169 with { Volume = 0.6f }, Projectile.Center);
        }

        float t = (timer - dashStart) / DashTicks;
        float speed = MathHelper.Lerp(StartSpeed, EndSpeed, MathHelper.Clamp(t * 3f, 0f, 1f));

        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * speed;
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

        if (!Main.dedServ && Main.rand.NextBool(2))
            Dust.NewDustPerfect(Projectile.Center, DustID.Copper, -Projectile.velocity * 0.1f, 100, default, 1f).noGravity = true;

        if (t > 0.8f)
            Projectile.alpha = Math.Min(255, Projectile.alpha + 30);

        if (t >= 1f)
            Projectile.Kill();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.instance.LoadItem(ItemID.CopperShortsword);
        Texture2D texture = TextureAssets.Item[ItemID.CopperShortsword].Value;
        Vector2 origin = texture.Size() / 2f;
        float opacity = 1f - Projectile.alpha / 255f;

        for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
        {
            if (Projectile.oldPos[i] == Vector2.Zero)
                continue;

            float fade = 1f - i / (float)Projectile.oldPos.Length;
            Color trail = new Color(255, 150, 70, 0) * (0.35f * fade * opacity);

            Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, trail, Projectile.oldRot[i], origin, Projectile.scale, SpriteEffects.None, 0);
        }

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

        return false;
    }
#endif
}