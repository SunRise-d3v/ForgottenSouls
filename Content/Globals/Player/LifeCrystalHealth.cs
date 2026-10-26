namespace ForgottenSouls.Content.Items;

internal sealed class LifeCrystalHealth : ModPlayer
{
#if false
    public const int StartHealth = 50; 
    public const int CrystalBonus = 15; 
    public const int MaxLife = 400;       
    public const int VanillaBonus = 20;

    public int Crystals;              
    private bool migrated;
    private int lastBase = -1;

    public static int MaxCrystals => (int)Math.Ceiling((MaxLife - StartHealth) / (double)CrystalBonus);

    public override void SaveData(TagCompound tag)
    {
        tag["crystals"] = Crystals;
        tag["migrated"] = migrated;
    }

    public override void LoadData(TagCompound tag)
    {
        Crystals = tag.GetInt("crystals");
        migrated = tag.GetBool("migrated");
    }

    public override void OnEnterWorld()
    {
        if (migrated)
            return;

        migrated = true;
        int old = Math.Clamp((Player.statLifeMax - 100) / VanillaBonus, 0, 15);
        Crystals = old;
        Player.statLifeMax = 100 + Math.Max(0, Player.statLifeMax - 400);
    }

    public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
    {
        int target = Math.Min(MaxLife, StartHealth + CrystalBonus * Crystals);
        health = new StatModifier(1f, 1f, target - 100);
        mana = StatModifier.Default;
    }

    public override void PreUpdate()
    {
        lastBase = Player.statLifeMax;
    }

    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer || lastBase < 0)
            return;

        if (Player.statLifeMax - lastBase == VanillaBonus)
        {
            Player.statLifeMax -= VanillaBonus;
            Player.statLifeMax2 -= VanillaBonus;
            Crystals++;

            Player.statLife = Math.Max(1, Player.statLife - (VanillaBonus - CrystalBonus));
            FixHealPopup();
        }
    }

    private static void FixHealPopup()
    {
        string vanilla = VanillaBonus.ToString();
        string fixedText = CrystalBonus.ToString();

        for (int i = 0; i < Main.maxCombatText; i++)
        {
            CombatText text = Main.combatText[i];
            if (text.active && text.text == vanilla && text.color == CombatText.HealLife)
                text.text = fixedText;
        }
    }
#endif
}

internal sealed class LifeCrystalLimit : GlobalItem
{
#if false
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        => entity.type == ItemID.LifeCrystal;

    public override bool CanUseItem(Item item, Player player)
        => player.GetModPlayer<LifeCrystalHealthPlayer>().Crystals < LifeCrystalHealthPlayer.MaxCrystals;
#endif
}