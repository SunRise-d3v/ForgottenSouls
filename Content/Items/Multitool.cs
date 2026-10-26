namespace ForgottenSouls.Content;

internal class Multitool : ModItem
{
    public override string Texture => Helper.TextureLoad("Items", "Multitool");

    private const float SpreadDegrees = 15f;

    private const int MinShots = 4;
    private const int MaxShots = 5;

    private static readonly int[] Pool =
    [
        ProjectileID.SnowBallFriendly,
        ProjectileID.Shuriken,
        ProjectileID.RottenEgg,
        ProjectileID.ThrowingKnife,
        ProjectileID.PoisonedKnife,
        ProjectileID.BoneDagger,
        ProjectileID.StarAnise,
        ProjectileID.JavelinFriendly,
        ProjectileID.Bone,
        ProjectileID.VampireKnife,
        ProjectileID.BoneJavelin,
        ProjectileID.FrostDaggerfish
    ];

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;

        Item.damage = 111;
        Item.DamageType = DamageClass.Ranged;
        Item.knockBack = 3f;

        Item.useStyle = ItemUseStyleID.Swing;
        Item.noUseGraphic = true;
        Item.noMelee = true;
        Item.UseSound = SoundID.Item1;

        Item.useTime = 28;
        Item.useAnimation = 28;
        Item.autoReuse = true;

        Item.shoot = ProjectileID.WoodenArrowFriendly;
        Item.shootSpeed = 17f;

        Item.rare = ItemRarityID.Orange;
        Item.value = Item.sellPrice(gold: 2);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        int count = Main.rand.Next(MinShots, MaxShots + 1);

        for (int i = 0; i < count; i++)
        {
            int projType = Pool[Main.rand.Next(Pool.Length)];
            Vector2 vel = velocity.RotatedByRandom(MathHelper.ToRadians(SpreadDegrees)) * Main.rand.NextFloat(0.95f, 1.05f);

            int index = Projectile.NewProjectile(source, position, vel, projType, damage, knockback, player.whoAmI);
            if (index < 0 || index >= Main.maxProjectiles)
                continue;

            Projectile proj = Main.projectile[index];
            proj.friendly = true;
            proj.hostile = false;
            proj.GetGlobalProjectile<MultitoolProjectile>().FromMultitool = true;
        }

        return false;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<TrueSnowball>(), 1)
            .AddIngredient(ModContent.ItemType<TrueShuriken>(), 1)
            .AddIngredient(ModContent.ItemType<TrueRottenEgg>(), 1)
            .AddIngredient(ModContent.ItemType<TrueThrowingKnife>(), 1)
            .AddIngredient(ModContent.ItemType<TruePoisonedKnife>(), 1)
            .AddIngredient(ModContent.ItemType<TrueBoneDagger>(), 1)
            .AddIngredient(ModContent.ItemType<TrueStarAnise>(), 1)
            .AddIngredient(ModContent.ItemType<TrueJavelin>(), 1)
            .AddIngredient(ModContent.ItemType<TrueBone>(), 1)
            .AddIngredient(ModContent.ItemType<TrueBoneJavelin>(), 1)
            .AddIngredient(ModContent.ItemType<TrueDaggerFish>(), 1)
            .AddIngredient(ItemID.VampireKnives, 1)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

internal class MultitoolProjectile : GlobalProjectile
{
    private const float EffectChance = 0.5f;

    private const float HomingRange = 350f;
    private const float HomingStrength = 0.04f;
    private const float MaxTurnAngle = 70f;

    private static readonly Dictionary<int, (int Buff, int Ticks)> Effects = new()
    {
        { ProjectileID.SnowBallFriendly, (BuffID.Chilled, 240) },
        { ProjectileID.Shuriken, (BuffID.Bleeding, 300) },
        { ProjectileID.RottenEgg, (-1, 0) },
        { ProjectileID.ThrowingKnife, (-2, 300) },
        { ProjectileID.PoisonedKnife, (BuffID.Poisoned, 300) },
        { ProjectileID.BoneDagger, (BuffID.Confused, 180) },
        { ProjectileID.StarAnise, (BuffID.Midas, 600) },
        { ProjectileID.JavelinFriendly, (BuffID.Slow, 240) },
        { ProjectileID.Bone, (BuffID.ShadowFlame, 240) },
        { ProjectileID.VampireKnife, (-3, 0) },
        { ProjectileID.BoneJavelin, (BuffID.Venom, 240) },
        { ProjectileID.FrostDaggerfish, (BuffID.Frostburn, 300) }
    };

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        => lateInstantiation && Effects.ContainsKey(entity.type);

    public bool FromMultitool;

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (!FromMultitool || projectile.owner != Main.myPlayer || projectile.type != ProjectileID.RottenEgg)
            return;

        if (Main.rand.NextFloat() < EffectChance)
            modifiers.FinalDamage *= 2f;
    }

    public override void PostAI(Projectile projectile)
    {
        if (!FromMultitool || projectile.owner != Main.myPlayer)
            return;

        float speed = projectile.velocity.Length();
        if (speed < 1f)
            return;

        NPC best = null;
        float bestDist = HomingRange;

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.CanBeChasedBy(projectile))
                continue;

            float dist = Vector2.Distance(projectile.Center, npc.Center);
            if (dist >= bestDist)
                continue;

            float angle = MathF.Abs(MathHelper.WrapAngle(projectile.velocity.ToRotation() - (npc.Center - projectile.Center).ToRotation()));
            if (angle > MathHelper.ToRadians(MaxTurnAngle))
                continue;

            best = npc;
            bestDist = dist;
        }

        if (best == null)
            return;

        Vector2 desired = (best.Center - projectile.Center).SafeNormalize(projectile.velocity.SafeNormalize(Vector2.UnitX));
        Vector2 newDir = Vector2.Lerp(projectile.velocity.SafeNormalize(Vector2.UnitX), desired, HomingStrength).SafeNormalize(desired);
        projectile.velocity = newDir * speed;

        if (Main.GameUpdateCount % 4 == 0)
            projectile.netUpdate = true;
    }

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (!FromMultitool || projectile.owner != Main.myPlayer)
            return;

        if (!Effects.TryGetValue(projectile.type, out var effect) || Main.rand.NextFloat() >= EffectChance)
            return;

        switch (effect.Buff)
        {
            case -1:
                break;

            case -2:
                target.AddBuff(ModContent.BuffType<ShatteredArmorBuff>(), effect.Ticks);
                break;

            case -3:
                Main.player[projectile.owner].Heal(Math.Max(1, damageDone / 10));
                break;

            default:
                target.AddBuff(effect.Buff, effect.Ticks);
                break;
        }
    }
}
internal class ShatteredArmorBuff : ModBuff
{
    public const int DefenseLoss = 12;

    public override string Texture => "Terraria/Images/Buff_" + BuffID.BrokenArmor;

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }

    public override void Update(NPC npc, ref int buffIndex)
    {
        npc.GetGlobalNPC<ShatteredArmorNPC>().Shattered = true;
    }
}

internal class ShatteredArmorNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public bool Shattered;

    public override void ResetEffects(NPC npc)
    {
        Shattered = false;
    }

    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (Shattered)
            modifiers.Defense.Flat -= ShatteredArmorBuff.DefenseLoss;
    }
}