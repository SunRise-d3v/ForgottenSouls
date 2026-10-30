namespace ForgottenSouls.Content.UI;

internal class RagePlayer : ModPlayer
{
#if false
    public const float MaxRage = 100f;

    private const float NearbyRadius = 600f;        
    private const float GainPerMonster = 0.12f;       
    private const int MaxCountedMonsters = 5;    
    private const float DecayPerTick = 0.15f;   

    private const float ActivationThreshold = MaxRage; 
    private const float ActiveSeconds = 10f;      
    private const float DamageBonus = 0.11f;            //+11% 
    private const int DefensePenalty = 5;               //-5

    public float Rage;
    public bool Active;

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (RageSystem.ActivateKey?.JustPressed == true)
            TryActivate();
    }

    private void TryActivate()
    {
        if (Active || Player.dead || Rage < ActivationThreshold)
            return;

        Active = true;

        SoundEngine.PlaySound(SoundID.Roar with { Volume = 0.6f, Pitch = 0.3f }, Player.Center);
        CombatText.NewText(Player.Hitbox, Color.OrangeRed, "Ярость!");
    }

    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        if (Player.dead)
        {
            Active = false;
            Rage = 0f;
            return;
        }

        if (Active)
        {
            Rage -= MaxRage / (ActiveSeconds * 60f);

            if (Rage <= 0f)
            {
                Rage = 0f;
                Active = false;
                return;
            }

            SpawnBloodParticles();
        }
        else
        {
            int monsters = CountNearbyMonsters();

            if (monsters > 0)
                Rage += Math.Min(monsters, MaxCountedMonsters) * GainPerMonster;
            else
                Rage -= DecayPerTick;

            Rage = MathHelper.Clamp(Rage, 0f, MaxRage);
        }
    }

    public override void PostUpdateEquips()
    {
        if (!Active)
            return;

        Player.GetDamage(DamageClass.Generic) += DamageBonus;
        Player.statDefense -= DefensePenalty;
    }

    private int CountNearbyMonsters()
    {
        int count = 0;

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];

            if (!npc.active || npc.friendly || npc.townNPC || npc.damage <= 0 || NPCID.Sets.CountsAsCritter[npc.type])
                continue;

            if (npc.Distance(Player.Center) <= NearbyRadius)
                count++;
        }

        return count;
    }

    private void SpawnBloodParticles()
    {
        if (Main.dedServ)
            return;

        for (int i = 0; i < 2; i++)
        {
            Vector2 position = Player.Center + Main.rand.NextVector2Circular(Player.width / 2f, Player.height / 2f);
            Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f) + new Vector2(0f, -1.5f);

            Dust dust = Dust.NewDustPerfect(position, DustID.Blood, velocity, 100, default, Main.rand.NextFloat(1f, 1.6f));
            dust.noGravity = Main.rand.NextBool(3);
        }

        Lighting.AddLight(Player.Center, 0.5f, 0.05f, 0.05f);
    }
}

internal class RageSystem : ModSystem
{
    public static ModKeybind ActivateKey { get; private set; }

    private static readonly Vector2 BarPosition = new(10f, 90f);
    private const float FillOffsetY = 2f;        

    private const float ShakeStrength = 2f;        

    private static Asset<Texture2D> _frame;
    private static Asset<Texture2D> _fill;

    public override void Load()
    {
        ActivateKey = KeybindLoader.RegisterKeybind(Mod, "ActivateRage", Keys.V);

        if (Main.dedServ)
            return;

        _frame = ModContent.Request<Texture2D>(Helper.TextureLoadUI("RageBarFrame"));
        _fill = ModContent.Request<Texture2D>(Helper.TextureLoadUI("RageBarFill"));
    }

    public override void Unload()
    {
        ActivateKey = null;
        _frame = null;
        _fill = null;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name == "Vanilla: Inventory");

        if (index < 0)
            index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");

        if (index < 0)
            return;

        layers.Insert(index + 1, new LegacyGameInterfaceLayer("ForgottenSouls: Rage Bar", DrawBar, InterfaceScaleType.UI));
    }

    private static bool DrawBar()
    {
        if (Main.gameMenu || _frame == null || _fill == null)
            return true;

        Player player = Main.LocalPlayer;

        if (!player.active || player.dead)
            return true;

        RagePlayer rage = player.GetModPlayer<RagePlayer>();

        Texture2D frame = _frame.Value;
        Texture2D fill = _fill.Value;
        SpriteBatch spriteBatch = Main.spriteBatch;

        spriteBatch.Draw(frame, BarPosition, Color.White);

        Vector2 fillOrigin = BarPosition + new Vector2((frame.Width - fill.Width) / 2f, (frame.Height - fill.Height) / 2f + FillOffsetY);

        int width = (int)Math.Round(fill.Width * rage.Rage / RagePlayer.MaxRage);

        if (width > 0)
        {
            Rectangle source = new(fill.Width - width, 0, width, fill.Height);
            Vector2 position = fillOrigin + new Vector2(fill.Width - width, 0f);

            Color tint = Color.White;

            if (rage.Active)
            {
                float pulse = 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 12f);
                tint = Color.Lerp(Color.White, new Color(255, 90, 60), pulse);
            }

            spriteBatch.Draw(fill, position, source, tint);
        }

        Rectangle area = new((int)BarPosition.X, (int)BarPosition.Y, frame.Width, frame.Height);

        if (area.Contains(Main.MouseScreen.ToPoint()))
        {
            string key = ActivateKey?.GetAssignedKeys().Count > 0 ? ActivateKey.GetAssignedKeys()[0] : "не назначена";
            Main.instance.MouseText($"Ярость: {(int)rage.Rage}/{(int)RagePlayer.MaxRage}\nКлавиша: {key}");
        }

        return true;
    }

    public override void ModifyScreenPosition()
    {
        if (Main.gameMenu || !Main.LocalPlayer.active)
            return;

        if (Main.LocalPlayer.GetModPlayer<RagePlayer>().Active)
            Main.screenPosition += Main.rand.NextVector2Circular(ShakeStrength, ShakeStrength);
    }
#endif
}