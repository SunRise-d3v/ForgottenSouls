namespace ForgottenSouls.Content;

internal sealed class ObserverSystem : ModSystem
{
    public const float MaxKarma = 400f;

    private static readonly float[] Thresholds = [60f, 150f, 300f];

    private static float karma;

    public static float Karma => karma;

    public static int Level
    {
        get
        {
            int level = 0;

            for (int i = 0; i < Thresholds.Length; i++)
            {
                if (Math.Abs(karma) >= Thresholds[i])
                    level = i + 1;
            }

            return karma < 0f ? -level : level;
        }
    }

    public static string LevelKey(int level) => level == 0 ? "Neutral" : level > 0 ? $"Pleased{level}" : $"Angry{-level}";

    private int minuteTimer;
    private int evilTimer;
    private int lightningTimer = 1200;
    private int grenadeTimer = 1200;
    private int bossTimer = 60 * 60 * 18;
    private float lastEvil = -1f;
    private bool wasDay = true;

    public override void ClearWorld()
    {
        karma = 0f;
        lastEvil = -1f;
    }

    public override void OnWorldLoad()
    {
        wasDay = Main.dayTime;
        lastEvil = -1f;
    }

    public override void SaveWorldData(TagCompound tag) => tag["karma"] = karma;

    public override void LoadWorldData(TagCompound tag) => karma = tag.GetFloat("karma");

    // Clients learn the karma when they join and whenever the server sends world data
    public override void NetSend(BinaryWriter writer) => writer.Write(karma);

    public override void NetReceive(BinaryReader reader) => karma = reader.ReadSingle();

    public static void AddKarma(float amount)
    {
        if (amount == 0f)
            return;

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            ModPacket packet = ModContent.GetInstance<ForgottenSouls>().GetPacket();
            packet.Write((byte)5);
            packet.Write(amount);
            packet.Send();
            return;
        }

        ApplyKarma(amount);
    }

    public static void ApplyKarma(float amount)
    {
        int oldLevel = Level;
        karma = MathHelper.Clamp(karma + amount, -MaxKarma, MaxKarma);
        int newLevel = Level;

        if (newLevel == oldLevel)
            return;

        Color color = newLevel > oldLevel ? new Color(120, 220, 255) : new Color(255, 90, 90);
        string key = "Mods.ForgottenSouls.Observer.Changed." + LevelKey(newLevel);

        if (Main.netMode == NetmodeID.Server)
        {
            ChatHelper.BroadcastChatMessage(NetworkText.FromKey(key), color);
            NetMessage.SendData(MessageID.WorldData);
        }
        else
        {
            Main.NewText(Language.GetTextValue(key), color);
        }
    }

    public override void PostUpdateWorld()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        UpdateHousing();
        UpdateEvil();
        UpdateSkyEvents();
        UpdatePunishments();
    }

    private void UpdateHousing()
    {
        if (++minuteTimer < 3600)
            return;

        minuteTimer = 0;

        int housed = 0, homeless = 0;

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];

            if (!npc.active || !npc.townNPC || NPCID.Sets.NoTownNPCHappiness[npc.type])
                continue;

            if (npc.homeless)
                homeless++;
            else
                housed++;
        }

        float reward = Math.Min(housed, 10) * 0.3f * (homeless > 0 ? 0.5f : 1f);
        float drift = -Math.Sign(karma) * Math.Min(Math.Abs(karma), 0.3f);

        ApplyKarma(reward + drift);
    }

    private void UpdateEvil()
    {
        if (++evilTimer < 600)
            return;

        evilTimer = 0;

        float evil = WorldGen.tEvil + WorldGen.tBlood;

        if (lastEvil < 0f)
        {
            lastEvil = evil;
            return;
        }

        float delta = MathHelper.Clamp(evil - lastEvil, -10f, 10f);
        lastEvil = evil;

        if (delta >= 1f)
            ApplyKarma(-3f * delta);
        else if (delta <= -1f)
            ApplyKarma(4f * -delta);
    }

    private void UpdateSkyEvents()
    {
        bool day = Main.dayTime;
        int level = Level;

        if (level < 0)
        {
            float chance = 0.07f * -level;

            // Dusk
            if (wasDay && !day && !Main.bloodMoon && !Main.eclipse && Main.rand.NextFloat() < chance)
            {
                Main.bloodMoon = true;
                Announce("LegacyMisc.8", Color.OrangeRed);
            }

            // Dawn
            if (!wasDay && day && Main.hardMode && !Main.bloodMoon && !Main.eclipse && Main.rand.NextFloat() < chance)
            {
                Main.eclipse = true;
                Announce("LegacyMisc.20", Color.Orange);
            }
        }

        wasDay = day;
    }

    private static void Announce(string key, Color color)
    {
        if (Main.netMode == NetmodeID.Server)
        {
            ChatHelper.BroadcastChatMessage(NetworkText.FromKey(key), color);
            NetMessage.SendData(MessageID.WorldData);
        }
        else
        {
            Main.NewText(Language.GetTextValue(key), color);
        }
    }

    private void UpdatePunishments()
    {
        int level = Level;

        if (level > -2)
            return;

        List<Player> players = [];

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player p = Main.player[i];

            if (p.active && !p.dead)
                players.Add(p);
        }

        if (players.Count == 0)
            return;

        if (--lightningTimer <= 0)
        {
            lightningTimer = Main.rand.Next(1200, 2400);
            SpawnLightning(players[Main.rand.Next(players.Count)]);
        }

        if (level > -3)
            return;

        if (--grenadeTimer <= 0)
        {
            grenadeTimer = Main.rand.Next(900, 1800);
            SpawnGrenades(players[Main.rand.Next(players.Count)]);
        }

        if (--bossTimer <= 0 && !Main.dayTime && !AnyBossAlive())
        {
            bossTimer = Main.rand.Next(60 * 60 * 16, 60 * 60 * 22);
            NPC.SpawnOnPlayer(players[Main.rand.Next(players.Count)].whoAmI, PickBoss());
        }
    }

    private static void SpawnLightning(Player target)
    {
        int damage = Main.hardMode ? 90 : 45;
        Vector2 position = new(target.Center.X + Main.rand.Next(-160, 161), target.Bottom.Y);

        Projectile.NewProjectile(new EntitySource_WorldEvent("Observer"), position, Vector2.Zero,
            ModContent.ProjectileType<ObserverLightning>(), damage, 0f, Main.myPlayer);
    }

    private static void SpawnGrenades(Player target)
    {
        int damage = Main.hardMode ? 80 : 40;

        for (int i = 0; i < 3; i++)
        {
            Vector2 position = new(target.Center.X + Main.rand.Next(-350, 351), target.Center.Y - 500f);
            Vector2 velocity = new(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(2f, 4f));

            Projectile.NewProjectile(new EntitySource_WorldEvent("Observer"), position, velocity,
                ModContent.ProjectileType<ObserverGrenade>(), damage, 0f, Main.myPlayer);
        }
    }

    private static bool AnyBossAlive()
    {
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            if (Main.npc[i].active && Main.npc[i].boss)
                return true;
        }

        return false;
    }

    private static int PickBoss()
    {
        List<int> pool = [.. Pool.Bosses.Where(b => b.Beaten).Select(b => b.Type)];

        return pool.Count == 0 ? NPCID.EyeofCthulhu : pool[Main.rand.Next(pool.Count)];
    }
}

internal sealed class ObserverBuff : ModBuff
{
    private const string TextureFolder = "ForgottenSouls/Assets/Textures/Buffs/";
    private const int DebuffFromLevel = -1;
    private static int appliedLevel = int.MinValue;

    private static readonly Asset<Texture2D>[] textures = new Asset<Texture2D>[7];

    private static string TexturePath(int level) => TextureFolder + "ObserverBuff" + level;

    public override string Texture => TexturePath(0);

    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.buffNoSave[Type] = true;

        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }

    public override bool RightClick(int buffIndex) => false;

    public override void Update(Player player, ref int buffIndex)
    {
        Main.debuff[Type] = ObserverSystem.Level <= DebuffFromLevel;
        RefreshIcon(Type);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams)
    {
        RefreshIcon(Type);
        drawParams.Texture = TextureAssets.Buff[Type].Value;

        return true;
    }

    public override void Unload()
    {
        appliedLevel = int.MinValue;
        Array.Clear(textures);
    }

    public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    {
        tip = Language.GetTextValue("Mods.ForgottenSouls.Observer.Level." + ObserverSystem.LevelKey(ObserverSystem.Level));
    }

    internal static void RefreshIcon(int buffType)
    {
        if (Main.dedServ)
            return;

        int level = ObserverSystem.Level;

        if (level == appliedLevel)
            return;

        appliedLevel = level;

        ref Asset<Texture2D> slot = ref textures[level + 3];
        slot ??= ModContent.Request<Texture2D>(TexturePath(level), AssetRequestMode.ImmediateLoad);

        TextureAssets.Buff[buffType] = slot;
    }
}

internal sealed class ObserverPlayer : ModPlayer
{
    private int graveyardTimer;

    public override void PostUpdate()
    {
        Player.AddBuff(ModContent.BuffType<ObserverBuff>(), 6, true);
        ObserverBuff.RefreshIcon(ModContent.BuffType<ObserverBuff>());

        if (Player.whoAmI == Main.myPlayer && Player.ZoneGraveyard && ++graveyardTimer >= 300)
        {
            graveyardTimer = 0;
            ObserverSystem.AddKarma(-1f);
        }
    }

    public override void PostUpdateEquips()
    {
        int level = ObserverSystem.Level;

        if (level > 0)
        {
            Player.GetDamage(DamageClass.Generic) += 0.03f * level;
            Player.GetCritChance(DamageClass.Generic) += level;
            Player.statDefense += 2 * level;
            Player.lifeRegen += level;
            Player.moveSpeed += 0.03f * level;
        }
        else if (level < 0)
        {
            int n = -level;

            Player.GetDamage(DamageClass.Generic) -= 0.04f * n;
            Player.statDefense -= 2 * n;
            Player.endurance -= 0.03f * n;
            Player.moveSpeed -= 0.03f * n;
            Player.lifeRegen -= n;
        }
    }

    public override void ModifyLuck(ref float luck)
    {
        int level = ObserverSystem.Level;
        luck += level > 0 ? 0.1f * level : 0.15f * level;
    }
}

internal sealed class ObserverNPC : GlobalNPC
{
    public override void SetDefaults(NPC npc)
    {
        int level = ObserverSystem.Level;

        if (level >= 0 || npc.friendly || npc.townNPC || npc.damage <= 0)
            return;

        int n = -level;

        npc.lifeMax = (int)(npc.lifeMax * (1f + 0.10f * n));
        npc.life = npc.lifeMax;
        npc.damage = (int)(npc.damage * (1f + 0.08f * n));
        npc.defDamage = npc.damage;
        npc.defense += n;
        npc.defDefense = npc.defense;
    }

    public override void OnKill(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (npc.lastInteraction >= Main.maxPlayers || !Main.player[npc.lastInteraction].active)
            return;

        if (npc.townNPC)
            ObserverSystem.AddKarma(-40f);
        else if (NPCID.Sets.CountsAsCritter[npc.type])
            ObserverSystem.AddKarma(-1.5f);
    }
}

internal sealed class ObserverTile : GlobalTile
{
    public override void PlaceInWorld(int i, int j, int type, Item item)
    {
        if (type == TileID.Saplings)
            ObserverSystem.AddKarma(1.5f);
    }
}

internal sealed class ObserverLightning : ModProjectile
{
    private const int WarnTicks = 50;
    private const int StrikeTicks = 8;

    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bullet;

    public override void SetDefaults()
    {
        Projectile.width = 40;
        Projectile.height = 40;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = WarnTicks + StrikeTicks + 2;
        Projectile.tileCollide = false;
        Projectile.aiStyle = -1;
        Projectile.hide = true;
    }

    private bool Striking => Projectile.ai[0] >= WarnTicks && Projectile.ai[0] < WarnTicks + StrikeTicks;

    public override void AI()
    {
        Projectile.ai[0]++;

        if (Projectile.ai[0] < WarnTicks)
        {
            float progress = Projectile.ai[0] / WarnTicks;
            float radius = 70f * (1f - progress) + 10f;
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);

            Dust d = Dust.NewDustPerfect(Projectile.Center + angle.ToRotationVector2() * radius, DustID.Electric, Vector2.Zero, 100, default, 1.1f);
            d.noGravity = true;
        }
        else if (Striking)
        {
            if (Projectile.ai[0] == WarnTicks)
                SoundEngine.PlaySound(SoundID.Thunder, Projectile.Center);

            for (int i = 0; i < 40; i++)
            {
                Vector2 pos = Projectile.Center - new Vector2(Main.rand.NextFloat(-10f, 10f), i * 22f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Electric, Vector2.Zero, 0, default, 1.5f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.8f, 0.9f, 1.2f);
        }
    }

    public override bool? CanHitNPC(NPC target) => false;

    public override bool CanHitPlayer(Player target) => Striking;

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        hitbox = new Rectangle((int)Projectile.Center.X - 30, (int)Projectile.Center.Y - 100, 60, 120);
    }

    public override bool PreDraw(ref Color lightColor) => false;
}

internal sealed class ObserverGrenade : ModProjectile
{
    private const int FuseTicks = 150;
    private const int ExplosionTicks = 3;

    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Grenade;

    public override void SetDefaults()
    {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = FuseTicks;
        Projectile.tileCollide = true;
        Projectile.aiStyle = -1;
    }

    private bool Exploding => Projectile.timeLeft <= ExplosionTicks;

    public override void AI()
    {
        Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.25f, 14f);
        Projectile.rotation += Projectile.velocity.X * 0.1f;

        if (Exploding)
        {
            Projectile.velocity = Vector2.Zero;
            return;
        }

        if (Main.rand.NextBool(3))
        {
            Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, Vector2.Zero, 100, default, 0.9f);
            d.noGravity = true;
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (Projectile.velocity.X != oldVelocity.X)
            Projectile.velocity.X = -oldVelocity.X * 0.6f;

        if (Projectile.velocity.Y != oldVelocity.Y)
            Projectile.velocity.Y = -oldVelocity.Y * 0.5f;

        return false;
    }

    public override bool? CanHitNPC(NPC target) => false;

    public override bool CanHitPlayer(Player target) => Exploding;

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        if (Exploding)
            hitbox.Inflate(70, 70);
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 25; i++)
        {
            Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Main.rand.NextVector2Circular(6f, 6f), 100, default, 1.8f);
            d.noGravity = true;
        }

        SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
    }
}