namespace ForgottenSouls.Content.Prefixes;

/// <summary>Collects the prefix stats from every equipped accessory and applies them, with the caps.</summary>
internal sealed class PrefixPlayer : ModPlayer
{
    public int ArmorPenetration;
    public float LifeSteal;
    public float DamageOverTime;
    public float DamageReduction;
    public float Thorns;
    public float LifeRegeneration;     // health per second
    public float WingTime;             // seconds
    public float ManaCostReduction;
    public float ManaRegeneration;
    public int MinionBonus;            // the best one counts, they do not add up
    public float WhipRange;

    private float lifeStealRemainder;

    public override void ResetEffects()
    {
        ArmorPenetration = 0;
        LifeSteal = 0f;
        DamageOverTime = 0f;
        DamageReduction = 0f;
        Thorns = 0f;
        LifeRegeneration = 0f;
        WingTime = 0f;
        ManaCostReduction = 0f;
        ManaRegeneration = 0f;
        MinionBonus = 0;
        WhipRange = 0f;
    }

    /// <summary>Called by PrefixItem for every modifier of every equipped accessory.</summary>
    internal void Add(PrefixStat stat, float value)
    {
        switch (stat)
        {
            case PrefixStat.ArmorPenetration: ArmorPenetration += (int)value; break;
            case PrefixStat.DamageOverTime: DamageOverTime += value; break;
            case PrefixStat.LifeSteal: LifeSteal += value; break;
            case PrefixStat.DamageReduction: DamageReduction += value; break;
            case PrefixStat.Thorns: Thorns += value; break;
            case PrefixStat.LifeRegeneration: LifeRegeneration += value; break;
            case PrefixStat.WingTime: WingTime += value; break;
            case PrefixStat.ManaCostReduction: ManaCostReduction += value; break;
            case PrefixStat.ManaRegeneration: ManaRegeneration += value; break;
            case PrefixStat.MinionBonus: MinionBonus = Math.Max(MinionBonus, (int)value); break;
            case PrefixStat.Capacity: Player.GetModPlayer<WeightPlayer>().ExtraLimit += (int)value; break;
        }
    }

    public override void PostUpdateEquips()
    {
        Player.endurance += Math.Min(DamageReduction, PrefixRegistry.Cap(PrefixStat.DamageReduction)) / 100f;
        Player.manaCost *= 1f - Math.Min(ManaCostReduction, PrefixRegistry.Cap(PrefixStat.ManaCostReduction)) / 100f;
        Player.manaRegen += (int)Math.Min(ManaRegeneration, PrefixRegistry.Cap(PrefixStat.ManaRegeneration));

        // Minions: +1 before Hardmode, +2 after it at most, and the bonuses never add up
        Player.maxMinions += Math.Min(MinionBonus, Main.hardMode ? 2 : 1);

        if (WingTime > 0f && Player.wingsLogic > 0)
            Player.wingTimeMax += (int)(WingTime * 60f);
    }

    // Health regeneration: lifeRegen counts in half hearts, so health per second is doubled
    public override void UpdateLifeRegen()
    {
        Player.lifeRegen += (int)(Math.Min(LifeRegeneration, PrefixRegistry.Cap(PrefixStat.LifeRegeneration)) * 2f);
    }

    // ---- Armor penetration: accessories + the weapon, 15 at most in total ----

    public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        => ApplyArmorPenetration(PrefixRegistry.SumWeapon(item, PrefixStat.ArmorPenetration), ref modifiers);

    public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        => ApplyArmorPenetration(proj.TryGetGlobalProjectile(out PrefixProjectile data) ? data.ArmorPenetration : 0f, ref modifiers);

    private void ApplyArmorPenetration(float weaponValue, ref NPC.HitModifiers modifiers)
    {
        int total = (int)Math.Min(ArmorPenetration + weaponValue, PrefixRegistry.Cap(PrefixStat.ArmorPenetration));

        if (total > 0)
            modifiers.ArmorPenetration += total;
    }

    // ---- Life steal: accessories + the weapon, 4% at most in total ----

    public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        => ApplyLifeSteal(PrefixRegistry.SumWeapon(item, PrefixStat.LifeSteal), target, damageDone);

    public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        => ApplyLifeSteal(proj.TryGetGlobalProjectile(out PrefixProjectile data) ? data.LifeSteal : 0f, target, damageDone);

    private void ApplyLifeSteal(float weaponValue, NPC target, int damageDone)
    {
        float percent = Math.Min(LifeSteal + weaponValue, PrefixRegistry.Cap(PrefixStat.LifeSteal));

        if (percent <= 0f || damageDone <= 0 || target.friendly || target.lifeMax <= 5)
            return;

        if (Player.whoAmI != Main.myPlayer || Player.statLife >= Player.statLifeMax2)
            return;

        // Small percentages heal fractions of a point: keep the remainder until it adds up to a whole point
        lifeStealRemainder += damageDone * percent / 100f;
        int healing = (int)lifeStealRemainder;

        if (healing <= 0)
            return;

        lifeStealRemainder -= healing;
        Player.Heal(healing);
    }

    // ---- Thorns: part of the damage taken goes back to the attacker ----

    public override void OnHurt(Player.HurtInfo info)
    {
        float percent = Math.Min(Thorns, PrefixRegistry.Cap(PrefixStat.Thorns));

        if (percent <= 0f || info.Damage <= 0 || Player.whoAmI != Main.myPlayer)
            return;

        int damage = (int)(info.Damage * percent / 100f);

        if (damage <= 0)
            return;

        int npcIndex = info.DamageSource.SourceNPCIndex;

        if (npcIndex < 0 || npcIndex >= Main.maxNPCs)
            return;

        NPC attacker = Main.npc[npcIndex];

        if (attacker.active && !attacker.friendly && attacker.lifeMax > 5)
        {
            attacker.StrikeNPC(new NPC.HitInfo
            {
                Damage = damage,
                Knockback = 0f,
                HitDirection = Player.direction,
                DamageType = DamageClass.Generic
            });
        }
    }
}