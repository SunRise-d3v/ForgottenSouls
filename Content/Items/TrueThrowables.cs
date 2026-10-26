using Terraria.DataStructures;

namespace ForgottenSouls.Content;

internal static class TrueThrowables
{
    public const int CraftCost = 500;
    public const float DamageMult = 1.25f;
    public const float EffectChance = 0.5f; // шанс эффекта при попадании (можно переопределить у предмета)
}

/// <summary>Эффект «истинного» предмета: дебафф (тип + время в тиках) или двойной урон.</summary>
internal readonly record struct ThrowEffect(int Buff, int Ticks, bool DoubleDamage = false);

/// <summary>
/// Общая основа для всех «истинных» метательных предметов.
/// Наследнику достаточно указать ванильный предмет и русское название.
/// </summary>
internal abstract class TrueThrowableBase : ModItem
{
    protected abstract int BaseType { get; }
    protected abstract string RuName { get; }

    // Эффект именно этого предмета и его описание для подсказки
    protected abstract ThrowEffect Effect { get; }
    protected abstract string EffectDescription { get; }
    protected virtual float EffectChance => TrueThrowables.EffectChance;

    // Текстура ванильного предмета
    public override string Texture => "Terraria/Images/Item_" + BaseType;

    public override void SetDefaults()
    {
        Item.CloneDefaults(BaseType);

        Item.consumable = false;
        Item.maxStack = 1;

        Item.ammo = AmmoID.None;
        Item.notAmmo = true;
        Item.damage = (int)(Item.damage * TrueThrowables.DamageMult);
        Item.rare = Math.Max(Item.rare, ItemRarityID.Orange);
        Item.value = Item.sellPrice(gold: 1);

        Item.SetNameOverride(RuName);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "TrueThrowable", "Не расходуется"));
        tooltips.Add(new TooltipLine(Mod, "TrueThrowableEffect", $"{(int)(EffectChance * 100)}% шанс при попадании: {EffectDescription}"));
    }

    // Бросаем ванильный снаряд и помечаем его эффектом этого предмета
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        int index = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

        if (index >= 0 && index < Main.maxProjectiles
            && Main.projectile[index].TryGetGlobalProjectile(out TrueThrowableProjectile data))
        {
            ThrowEffect effect = Effect;
            data.Active = true;
            data.Buff = effect.Buff;
            data.Ticks = effect.Ticks;
            data.DoubleDamage = effect.DoubleDamage;
            data.Chance = EffectChance;
        }

        return false; // обычный снаряд уже создан выше
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(BaseType, TrueThrowables.CraftCost)
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

// ---- 12 предметов ----

internal class TrueSnowball : TrueThrowableBase
{
    protected override int BaseType => ItemID.Snowball;
    protected override string RuName => "Истинный снежок";
    protected override ThrowEffect Effect => new(BuffID.Chilled, 300);
    protected override string EffectDescription => "окоченелость";
}

internal class TrueShuriken : TrueThrowableBase
{
    protected override int BaseType => ItemID.Shuriken;
    protected override string RuName => "Истинный сюрикен";
    protected override ThrowEffect Effect => new(BuffID.Bleeding, 360);
    protected override string EffectDescription => "кровотечение";
}

internal class TrueRottenEgg : TrueThrowableBase
{
    protected override int BaseType => ItemID.RottenEgg;
    protected override string RuName => "Истинное тухлое яйцо";
    protected override ThrowEffect Effect => new(0, 0, true);
    protected override string EffectDescription => "двойной урон";
}

internal class TrueThrowingKnife : TrueThrowableBase
{
    protected override int BaseType => ItemID.ThrowingKnife;
    protected override string RuName => "Истинный метательный нож";
    protected override ThrowEffect Effect => new(ModContent.BuffType<ShatteredArmorBuff>(), 360);
    protected override string EffectDescription => "расколотая броня (защита врага ниже)";
}

internal class TruePoisonedKnife : TrueThrowableBase
{
    protected override int BaseType => ItemID.PoisonedKnife;
    protected override string RuName => "Истинный отравленный нож";
    protected override ThrowEffect Effect => new(BuffID.Poisoned, 360);
    protected override string EffectDescription => "отравление";
}

internal class TrueBoneDagger : TrueThrowableBase
{
    protected override int BaseType => ItemID.BoneDagger;
    protected override string RuName => "Истинный костяной метательный нож";
    protected override ThrowEffect Effect => new(BuffID.Confused, 240);
    protected override string EffectDescription => "смятение: враг теряет направление";
}

internal class TrueStarAnise : TrueThrowableBase
{
    protected override int BaseType => ItemID.StarAnise;
    protected override string RuName => "Истинный бадьян";
    protected override ThrowEffect Effect => new(BuffID.Midas, 600);
    protected override string EffectDescription => "Мидас: с врага падает больше монет";
}

internal class TrueJavelin : TrueThrowableBase
{
    protected override int BaseType => ItemID.Javelin;
    protected override string RuName => "Истинное метательное копьё";
    protected override ThrowEffect Effect => new(BuffID.Slow, 300);
    protected override string EffectDescription => "замедление";
}

internal class TrueBone : TrueThrowableBase
{
    protected override int BaseType => ItemID.Bone;
    protected override string RuName => "Истинная кость";
    protected override ThrowEffect Effect => new(BuffID.ShadowFlame, 300);
    protected override string EffectDescription => "теневое пламя";
}

internal class TrueMolotovCocktail : TrueThrowableBase
{
    protected override int BaseType => ItemID.MolotovCocktail;
    protected override string RuName => "Истинный коктейль Молотова";
    protected override ThrowEffect Effect => new(BuffID.Oiled, 360);
    protected override string EffectDescription => "масло: враг получает больше урона от огня";
}

internal class TrueBoneJavelin : TrueThrowableBase
{
    protected override int BaseType => ItemID.BoneJavelin;
    protected override string RuName => "Истинное костяное метательное копьё";
    protected override ThrowEffect Effect => new(BuffID.Venom, 300);
    protected override string EffectDescription => "яд";
}

internal class TrueDaggerFish : TrueThrowableBase
{
    protected override int BaseType => ItemID.FrostDaggerfish;
    protected override string RuName => "Истинная метательная рыба-кинжал";
    protected override ThrowEffect Effect => new(BuffID.Frostburn, 360);
    protected override string EffectDescription => "обжигающий холод";
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

/// <summary>Применяет эффект «истинного» предмета к снаряду, который этот предмет бросил.</summary>
internal class TrueThrowableProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
        => lateInstantiation && entity.friendly;

    public bool Active;
    public int Buff;
    public int Ticks;
    public bool DoubleDamage;
    public float Chance;

    // Двойной урон (с шансом)
    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (!Active || !DoubleDamage || projectile.owner != Main.myPlayer)
            return;

        if (Main.rand.NextFloat() < Chance)
            modifiers.FinalDamage *= 2f;
    }

    // Дебафф (с шансом); применяет клиент владельца снаряда
    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (!Active || Buff <= 0 || projectile.owner != Main.myPlayer)
            return;

        if (Main.rand.NextFloat() < Chance)
            target.AddBuff(Buff, Ticks);
    }
}