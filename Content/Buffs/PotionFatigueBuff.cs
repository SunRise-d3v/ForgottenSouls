namespace ForgottenSouls.Content.Buffs;

internal sealed class PotionFatigueBuff : ModBuff
{
    public override string Texture => "Terraria/Images/Buff_" + BuffID.PotionSickness;

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }

    public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    {
        var fatigue = Main.LocalPlayer.GetModPlayer<PotionFatiguePlayer>();
        buffName = "Усталость от зелий";

        int penalty = (int)Math.Round((1f - fatigue.Efficiency) * 100f);
        tip = $"Накоплено: {fatigue.Stacks}. Зелья лечат на {penalty}% слабее.\n" +
              "Не пейте зелья, и усталость сбросится.";
    }
}

internal sealed class PotionFatiguePlayer : ModPlayer
{
    public const int MaxStacks = 10;
    public const int FreeUses = 2;
    public const float ReducePerStack = 0.10f;
    public const float MaxReduce = 0.70f;
    public const int ResetTicks = 90 * 60;

    public int Stacks;
    private int idleTimer;
    private int halveSicknessTicks;

    public float Efficiency => 1f - Math.Min(MaxReduce, Math.Max(0, Stacks - FreeUses) * ReducePerStack);

    public void OnPotionUsed()
    {
        if (Player.HasEffect<FossilPotions>() && Main.rand.NextFloat() < FossilPotions.Chance)
        {
            Reset();
            halveSicknessTicks = 10;
            CombatText.NewText(Player.Hitbox, Color.SandyBrown, "Без усталости!");
            return;
        }

        Stacks = Math.Min(MaxStacks, Stacks + 1);
        idleTimer = 0;

        Player.AddBuff(ModContent.BuffType<PotionFatigueBuff>(), ResetTicks);
    }

    private void Reset()
    {
        Stacks = 0;
        idleTimer = 0;
        Player.ClearBuff(ModContent.BuffType<PotionFatigueBuff>());
    }

    public override void PostUpdate()
    {
        if (halveSicknessTicks > 0)
        {
            halveSicknessTicks--;

            int idx = Player.FindBuffIndex(BuffID.PotionSickness);
            if (idx >= 0)
            {
                Player.buffTime[idx] /= 2;
                Player.potionDelay /= 2;
                halveSicknessTicks = 0;
            }
        }

        if (Stacks <= 0)
            return;

        if (++idleTimer >= ResetTicks)
            Reset();
    }

    public override void UpdateDead()
    {
        Stacks = 0;
        idleTimer = 0;
        halveSicknessTicks = 0;
    }

    public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
    {
        healValue = (int)(healValue * Efficiency);
    }

    /*public override void GetHealMana(Item item, bool quickHeal, ref int healValue)
    {
        healValue = (int)(healValue * Efficiency);
    }*/
}

internal sealed class PotionFatigueItem : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        => lateInstantiation && (entity.healLife > 0 || entity.healMana > 0);

    public override void OnConsumeItem(Item item, Player player)
    {
        player.GetModPlayer<PotionFatiguePlayer>().OnPotionUsed();
    }
}