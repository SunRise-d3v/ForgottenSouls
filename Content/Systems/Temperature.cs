namespace ForgottenSouls.Content.Mechanics;

internal sealed class Temperature : ModPlayer
{
    private const float ColdStep = 0.0005f;  
    private const float HeatStep = 0.0006f;    
    private const float RecoverStep = 0.0012f; 

    private static readonly float[] StageThresholds = [0.15f, 0.35f, 0.55f, 0.75f, 0.90f];

    private static readonly float[] MoveSlow = [0f, 0.10f, 0.18f, 0.25f, 0.32f, 0.40f];
    private static readonly int[] DefenseLoss = [0, 0, 6, 10, 14, 20];
    private static readonly float[] AttackSlow = [0f, 0f, 0.10f, 0.15f, 0.20f, 0.25f];
    private static readonly float[] PickSlow = [0f, 0f, 0.20f, 0.30f, 0.40f, 0.50f];
    private static readonly int[] DamagePerTick = [0, 0, 0, 0, 6, 16]; 

    private static readonly HashSet<int> SnowClothing =
    [
        ItemID.EskimoHood, ItemID.EskimoCoat, ItemID.EskimoPants,
        ItemID.PinkEskimoHood, ItemID.PinkEskimoCoat, ItemID.PinkEskimoPants
    ];

    private static readonly HashSet<int> ColdArmor =
    [
        ItemID.FrostHelmet, ItemID.FrostBreastplate, ItemID.FrostLeggings
    ];

    private const float ArmorInsulation = 0.07f;     
    private const float ColdArmorInsulation = 0.18f; 
    private const float SnowInsulation = 0.15f;      
    private const float SnowWarmth = 0.12f;     
    private const float MaxInsulation = 0.85f;

    private const float TorchInHandWarmth = 0.25f;
    private const float CampfireWarmth = 0.6f;
    private const float PlacedTorchWarmth = 0.08f; 
    private const int MaxPlacedTorches = 3;
    private const int TorchScanRadius = 5;     

    public float Temp;

    private int nearbyTorches;
    private int scanTimer;

    public int Stage
    {
        get
        {
            float value = Math.Abs(Temp);
            int stage = 0;

            for (int i = 0; i < StageThresholds.Length; i++)
            {
                if (value >= StageThresholds[i])
                    stage = i + 1;
            }

            return stage;
        }
    }

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) => Temp = 0f;

    public override void OnRespawn() => Temp = 0f;

    public override void PostUpdateEquips()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        UpdateTemperature();
        ApplyPenalties();

        ThermometerUI.Temperature = Temp;
    }

    private void UpdateTemperature()
    {
        if (++scanTimer >= 30)
        {
            scanTimer = 0;
            nearbyTorches = CountNearbyTorches();
        }

        bool sheltered = IsSheltered();
        float ambient = Ambient(sheltered);
        float target;

        if (ambient < 0f)
        {
            target = ambient * (1f - Insulation()) + Warmth();
            target = Math.Min(target, 0.2f);
        }
        else
        {
            target = ambient * (1f - HeatProtection());
        }

        target = MathHelper.Clamp(target, -1f, 1f);

        bool recovering = Math.Sign(target) != Math.Sign(Temp) || Math.Abs(target) < Math.Abs(Temp);
        float step = recovering ? RecoverStep : (target < 0f ? ColdStep : HeatStep);

        Temp += MathHelper.Clamp(target - Temp, -step, step);
    }

    private float Ambient(bool sheltered)
    {
        if (Player.ZoneUnderworldHeight)
            return 1f;

        Point tile = Player.Center.ToTileCoordinates();

        if (tile.Y < Main.worldSurface * 0.12)
            return -1f;

        bool underground = Player.ZoneDirtLayerHeight || Player.ZoneRockLayerHeight;
        bool night = !Main.dayTime;
        bool headCovered = HeadCovered();
        float ambient;

        if (Player.ZoneSnow)
        {
            ambient = underground ? -0.9f : -0.55f;
        }
        else if (Player.ZoneUndergroundDesert)
        {
            ambient = 0.45f;
        }
        else if (Player.ZoneDesert && !underground)
        {
            ambient = night ? -0.25f : (sheltered ? 0.25f : 0.75f * (headCovered ? 0.5f : 1f));
        }
        else if (underground)
        {
            ambient = Player.ZoneRockLayerHeight ? -0.4f : -0.3f;
        }
        else if (Player.ZoneSkyHeight)
        {
            ambient = -0.55f; 
        }
        else
        {
            ambient = night
                ? -0.2f
                : (sheltered || Player.wet || Main.raining ? 0f : 0.25f * (headCovered ? 0.3f : 1f));
        }

        if (ambient < 0f && sheltered && !underground)
            ambient *= 0.5f;

        if (Player.wet)
            ambient += ambient < 0f ? -0.2f : -0.25f;

        return MathHelper.Clamp(ambient, -1f, 1f);
    }

    private bool IsSheltered()
    {
        Point tile = Player.Center.ToTileCoordinates();

        return WorldGen.InWorld(tile.X, tile.Y, 5) && Main.tile[tile.X, tile.Y].WallType != WallID.None;
    }

    private bool HeadCovered()
    {
        if (!Player.armor[0].IsAir || !Player.armor[10].IsAir)
            return true;

        int held = Player.HeldItem.type;
        return held == ItemID.Umbrella || held == ItemID.TragicUmbrella;
    }

    private float Insulation()
    {
        float value = 0f;

        for (int i = 0; i < 3; i++)
        {
            Item piece = Player.armor[i];

            if (piece.IsAir)
                continue;

            value += ColdArmor.Contains(piece.type) ? ColdArmorInsulation : ArmorInsulation;

            if (SnowClothing.Contains(piece.type))
                value += SnowInsulation;
        }

        return Math.Min(value, MaxInsulation);
    }

    private float Warmth()
    {
        float value = 0f;

        for (int i = 0; i < 3; i++)
        {
            if (SnowClothing.Contains(Player.armor[i].type))
                value += SnowWarmth;
        }

        Item held = Player.HeldItem;

        if (held.createTile >= TileID.Dirt && TileID.Sets.Torch[held.createTile])
            value += TorchInHandWarmth;

        if (Player.HasBuff(BuffID.Campfire))
            value += CampfireWarmth;

        value += Math.Min(nearbyTorches, MaxPlacedTorches) * PlacedTorchWarmth;

        return value;
    }

    private float HeatProtection()
    {
        float value = 0f;

        if (Player.buffImmune[BuffID.OnFire])
            value += 0.4f;

        return value;
    }

    private int CountNearbyTorches()
    {
        Point center = Player.Center.ToTileCoordinates();
        int count = 0;

        for (int x = center.X - TorchScanRadius; x <= center.X + TorchScanRadius; x++)
        {
            for (int y = center.Y - TorchScanRadius; y <= center.Y + TorchScanRadius; y++)
            {
                if (!WorldGen.InWorld(x, y, 5))
                    continue;

                Tile t = Main.tile[x, y];

                if (t.HasTile && TileID.Sets.Torch[t.TileType])
                    count++;
            }
        }

        return count;
    }

    private void ApplyPenalties()
    {
        int stage = Stage;

        if (stage == 0)
            return;

        bool cold = Temp < 0f;

        Player.moveSpeed -= MoveSlow[stage];

        if (stage >= 2)
        {
            Player.statDefense -= DefenseLoss[stage];
            Player.GetAttackSpeed(DamageClass.Generic) -= AttackSlow[stage];
            Player.pickSpeed += PickSlow[stage];
        }

        if (stage >= 3)
            Player.AddBuff(cold ? ModContent.BuffType<HypothermiaBuff>() : ModContent.BuffType<OverheatBuff>(), 5, true);

        if (stage >= 5 && Main.GameUpdateCount % 60 == 0)
            Player.AddBuff(cold ? BuffID.Frostburn : BuffID.OnFire, 120);
    }

    public override void UpdateBadLifeRegen()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        int stage = Stage;

        if (stage < 4)
            return;

        if (Player.lifeRegen > 0)
            Player.lifeRegen = 0;

        Player.lifeRegenTime = 0;
        Player.lifeRegen -= DamagePerTick[stage];
    }
}

internal sealed class OverheatBuff : ModBuff
{
    public override string Texture => "Terraria/Images/Buff_" + BuffID.OnFire;

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }

    public override bool RightClick(int buffIndex) => false;
}
