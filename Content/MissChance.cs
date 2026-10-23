using FargowiltasSouls.Content.Buffs.Masomode;

namespace ForgottenSouls.Content;

internal class MissChancePlayer : ModPlayer
{
    public const float BaseMissChance = 0.02f;
    public const float BossMultiplier = 0.1f;
    public const float MaxMissChance = 0.13f;
    public const bool AffectMinions = false;

    public float AccuracyBonus;

    private struct MissEntry
    {
        public bool Miss;
        public uint Tick;
    }

    private readonly Dictionary<long, MissEntry> cache = [];

    public override void ResetEffects()
    {
        AccuracyBonus = 0f;
    }

    public float GetMissChance(NPC target)
    {
        float chance = BaseMissChance;

        if (Player.HasBuff(BuffID.Darkness) || Player.HasBuff(BuffID.Blackout) ||
            Player.HasBuff(BuffID.Obstructed) || Player.HasBuff(BuffID.Confused))
            chance += 0.05f;

        if (Player.HasBuff(ModContent.BuffType<GuiltyBuff>()) ||
            Player.HasBuff(ModContent.BuffType<LethargicBuff>()) ||
            Player.HasBuff(ModContent.BuffType<LovestruckBuff>()))
            chance += 0.04f;

        chance += MathHelper.Clamp(target.velocity.Length() - 6f, 0f, 2f) * 0.01f;

        if (target.boss)
            chance *= BossMultiplier;

        chance -= AccuracyBonus;
        return MathHelper.Clamp(chance, 0f, MaxMissChance);
    }

    private bool IsValidTarget(NPC target)
        => Player.whoAmI == Main.myPlayer && !target.friendly && !target.dontTakeDamage && target.type != NPCID.TargetDummy;

    private bool RollMiss(NPC target)
    {
        bool miss = Main.rand.NextFloat() < GetMissChance(target);
        if (miss)
            CombatText.NewText(target.Hitbox, Color.LightGray, "Промах!");
        return miss;
    }

    public override bool? CanHitNPCWithItem(Item item, NPC target)
    {
        if (!IsValidTarget(target))
            return null;

        if (RollMiss(target))
        {
            target.immune[Player.whoAmI] = Math.Max(target.immune[Player.whoAmI], Math.Max(Player.itemAnimation, 1));
            return false;
        }
        return null;
    }

    public override bool? CanHitNPCWithProj(Projectile proj, NPC target)
    {
        if (!AffectMinions && (proj.minion || proj.sentry || ProjectileID.Sets.MinionShot[proj.type] || ProjectileID.Sets.SentryShot[proj.type]))
            return null;

        if (!IsValidTarget(target))
            return null;

        long key = proj.identity * 256L + target.whoAmI;
        if (!cache.TryGetValue(key, out MissEntry entry))
        {
            entry = new MissEntry { Miss = RollMiss(target), Tick = Main.GameUpdateCount };
            cache[key] = entry;
        }

        return entry.Miss ? false : (bool?)null;
    }

    public override void PostUpdate()
    {
        if (Main.GameUpdateCount % 60 != 0 || cache.Count == 0)
            return;

        List<long> old = [];

        foreach (var kv in cache)
        {
            if (Main.GameUpdateCount - kv.Value.Tick > 120)
                old.Add(kv.Key);
        }

        foreach (long k in old)
            cache.Remove(k);
    }
}