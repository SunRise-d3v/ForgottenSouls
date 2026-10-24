using FargowiltasSouls.Content.Buffs.Masomode;

namespace ForgottenSouls.Content;

/// <summary>
/// Шанс уклониться от атаки игрока есть только у того монстра, которого реально задело оружие или снаряд игрока:
///  - статичные существа (стоят без движения / манекен / турели) - попадание 100%;
///  - ловкие существа (Тим и подобные) на 100% уклоняются от ПЕРВОЙ атаки игрока;
///  - остальные уклоняются с шансом из ваших значений (BaseMissChance ... MaxMissChance).
/// Дебаффы атакующего игрока по-прежнему повышают шанс монстра увернуться.
/// </summary>
internal class EnemyDodgeNPC : GlobalNPC
{
    public const float BaseMissChance = 0.02f;
    public const float BossMultiplier = 0.1f;
    public const float MaxMissChance = 0.13f;
    public const bool AffectMinions = false;

    private const int StaticTicks = 45; // тиков без движения, после которых существо считается статичным

    /// <summary>
    /// Ловкие существа: 100% уклонение от первой атаки.
    /// Значение - дополнительный шанс уклонения после первой атаки (прибавляется к базовому, потом общий потолок MaxMissChance).
    /// </summary>
    public static readonly Dictionary<int, float> Evasive = new()
    {
        { NPCID.Tim,         0.05f },
        { NPCID.DarkCaster,  0.03f },
        { NPCID.RuneWizard,  0.04f },
        { NPCID.Necromancer, 0.03f }
    };

    /// <summary>Всегда статичные: в них попадаешь на 100%, даже если они «ловкие».</summary>
    public static readonly HashSet<int> AlwaysStatic =
    [
        NPCID.TargetDummy,
        NPCID.MartianTurret
    ];

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => lateInstantiation && !entity.friendly && !entity.townNPC;

    private int stillTicks;
    private bool firstAttackDodged;
    private bool dodgedThisHit;   // текущее попадание превращено в промах, его урон потом возвращается

    public bool IsStatic(NPC npc)
        => AlwaysStatic.Contains(npc.type) || stillTicks >= StaticTicks;

    public override void PostAI(NPC npc)
    {
        if (npc.velocity.LengthSquared() < 0.04f)
        {
            if (stillTicks < 10000)
                stillTicks++;
        }
        else
        {
            stillTicks = 0;
        }
    }

    public float GetDodgeChance(NPC npc, Player attacker)
    {
        float chance = BaseMissChance;

        if (attacker.HasBuff(BuffID.Darkness) || attacker.HasBuff(BuffID.Blackout) ||
            attacker.HasBuff(BuffID.Obstructed) || attacker.HasBuff(BuffID.Confused))
            chance += 0.05f;

        if (attacker.HasBuff(ModContent.BuffType<GuiltyBuff>()) ||
            attacker.HasBuff(ModContent.BuffType<LethargicBuff>()) ||
            attacker.HasBuff(ModContent.BuffType<LovestruckBuff>()))
            chance += 0.04f;

        chance += MathHelper.Clamp(npc.velocity.Length() - 6f, 0f, 2f) * 0.01f;

        if (Evasive.TryGetValue(npc.type, out float bonus))
            chance += bonus;

        if (npc.boss)
            chance *= BossMultiplier;

        chance -= attacker.GetModPlayer<MissChancePlayer>().AccuracyBonus;
        return MathHelper.Clamp(chance, 0f, MaxMissChance);
    }

    private bool RollDodge(NPC npc, Player attacker)
    {
        if (IsStatic(npc))
            return false;

        if (Evasive.ContainsKey(npc.type) && !firstAttackDodged)
        {
            // Первая атака по ловкому существу: уклонение 100%
            firstAttackDodged = true;
            return true;
        }

        return Main.rand.NextFloat() < GetDodgeChance(npc, attacker);
    }

    /// <summary>
    /// Вызывается только когда удар уже засчитан (оружие реально задело именно этого моба).
    /// Если моб уклонился, превращаем удар в промах: убираем урон, откидывание, крит и числа, показываем «Miss!».
    /// </summary>
    private void TryDodge(NPC npc, Player attacker, ref NPC.HitModifiers modifiers)
    {
        // Существо с 1 здоровья не трогаем: минимальный урон в игре 1 и он бы его убил
        if (npc.dontTakeDamage || npc.life <= 1 || !RollDodge(npc, attacker))
            return;

        modifiers.FinalDamage *= 0f;
        modifiers.DisableCrit();
        modifiers.DisableKnockback();
        modifiers.HideCombatText();

        dodgedThisHit = true;
        CombatText.NewText(npc.Hitbox, Color.LightGray, "Miss!");
    }

    // Минимальный урон в игре 1, возвращаем его, чтобы промах действительно ничего не отнимал
    private void RestoreIfDodged(NPC npc, int damageDone)
    {
        if (!dodgedThisHit)
            return;

        dodgedThisHit = false;
        npc.life = Math.Min(npc.life + damageDone, npc.lifeMax);
    }

    public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
    {
        if (player.whoAmI == Main.myPlayer)
            TryDodge(npc, player, ref modifiers);
    }

    public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        if (projectile.owner != Main.myPlayer)
            return;

        if (!AffectMinions && (projectile.minion || projectile.sentry ||
            ProjectileID.Sets.MinionShot[projectile.type] || ProjectileID.Sets.SentryShot[projectile.type]))
            return;

        TryDodge(npc, Main.player[projectile.owner], ref modifiers);
    }

    public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        => RestoreIfDodged(npc, damageDone);

    public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        => RestoreIfDodged(npc, damageDone);
}

internal class MissChancePlayer : ModPlayer
{
    /// <summary>Вычитается из шанса монстра уклониться от атаки игрока.</summary>
    public float AccuracyBonus;

    public override void ResetEffects()
    {
        AccuracyBonus = 0f;
    }
}