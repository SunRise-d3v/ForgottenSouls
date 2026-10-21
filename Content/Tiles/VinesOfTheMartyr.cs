namespace ForgottenSouls.Content.Tiles;

internal sealed class VinesOfTheMartyr : ModNPC
{
    public override string Texture => Helper.TextureLoad("NPCs", "VinesOfTheMartyr");

    private const int MaxStage = 8;
    private const int TicksPerStage = 22 * 60;
    private const int PoisonTicks = 5 * 60;
    private const int SlowTicks = 12;
    private const int MinBushSize = 3;
    private const int MaxBushSize = 7;

    private const int SlowFromStage = 2;
    private const int PoisonFromStage = 4;
    private const int DamageFromStage = 5;

    private const float FireDamageMultiplier = 4f;

    private static readonly HashSet<int> FireProjectiles =
    [
        ProjectileID.Flames,
        ProjectileID.Fireball,
        ProjectileID.FireArrow,
        ProjectileID.HellfireArrow,
        ProjectileID.MolotovCocktail,
        ProjectileID.MolotovFire,
        ProjectileID.MolotovFire2,
        ProjectileID.MolotovFire3,
        ProjectileID.Flamarang,
    ];

    private static int _nextBushId = 1;

    private int Stage => (int)MathHelper.Clamp(NPC.ai[1], 0f, MaxStage);

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = MaxStage + 1;

        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;

        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers { Hide = true });
    }

    public override void SetDefaults()
    {
        NPC.width = 28;
        NPC.height = 40;
        NPC.lifeMax = 15;
        NPC.damage = 18;
        NPC.defense = 0;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.dontCountMe = true;
        NPC.value = 0f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
    }

    public override bool CheckActive() => false;

    public override void OnSpawn(IEntitySource source)
    {
        if (source is EntitySource_SpawnNPC && Main.netMode != NetmodeID.MultiplayerClient)
        {
            NPC.ai[2] = _nextBushId++;
            NPC.netUpdate = true;
            SpawnBush();
        }
    }

    public override void AI()
    {
        NPC.velocity = Vector2.Zero;

        if (Stage < MaxStage && ++NPC.ai[0] >= TicksPerStage)
        {
            NPC.ai[0] = 0f;
            NPC.ai[1]++;
            NPC.netUpdate = true;
        }

        if (++NPC.localAI[0] >= 30f)
        {
            NPC.localAI[0] = 0f;

            if (!HasGround(NPC))
            {
                NPC.StrikeInstantKill();
                return;
            }
        }

        NPC.damage = 0;

        int stageNumber = Stage + 1;
        bool slows = stageNumber >= SlowFromStage;
        bool poisons = stageNumber >= PoisonFromStage;
        bool damages = stageNumber >= DamageFromStage;

        if (!slows && !poisons && !damages)
            return;

        Rectangle zone = NPC.Hitbox;
        zone.Inflate(4, 4);

        bool canHit = false;

        foreach (Player player in Main.ActivePlayers)
        {
            if (player.dead || !player.Hitbox.Intersects(zone))
                continue;

            if (slows)
            {
                player.AddBuff(BuffID.Slow, SlowTicks);
                Main.buffNoTimeDisplay[BuffID.Slow] = true;
            }
            else if (!slows)
            {
                Main.buffNoTimeDisplay[BuffID.Slow] = false;
            }

            if (poisons)
            {
                player.AddBuff(BuffID.Poisoned, PoisonTicks);
                Main.buffNoTimeDisplay[BuffID.Poisoned] = true;
            }
            else if (!poisons)
            {
                Main.buffNoTimeDisplay[BuffID.Poisoned] = false;
            }
            else
            {
                Main.buffNoTimeDisplay[BuffID.Slow] = false;
                Main.buffNoTimeDisplay[BuffID.Poisoned] = false;
            }

            if (damages)
            {
                VinePlayer vinePlayer = player.GetModPlayer<VinePlayer>();
                vinePlayer.InVines = true;

                if (vinePlayer.DamageReady)
                    canHit = true;
            }
        }

        if (canHit)
            NPC.damage = NPC.defDamage;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
    {
        target.GetModPlayer<VinePlayer>().ResetDamageTimer();
    }

    public override void UpdateLifeRegen(ref int damage)
    {
        if (!NPC.onFire && !NPC.onFire3)
            return;

        if (NPC.lifeRegen < 0)
        {
            NPC.lifeRegen = (int)(NPC.lifeRegen * FireDamageMultiplier);
            damage = (int)(damage * FireDamageMultiplier);
        }
    }

    public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
    {
        if (FireProjectiles.Contains(projectile.type))
            modifiers.FinalDamage *= FireDamageMultiplier;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frame.Y = Stage * frameHeight;
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        int count = NPC.life > 0 ? 3 : 12;

        for (int i = 0; i < count; i++)
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Grass, hit.HitDirection, -1f);
    }

    internal static int CountBushes()
    {
        int type = ModContent.NPCType<VinesOfTheMartyr>();
        HashSet<int> bushes = [];

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];

            if (npc.active && npc.type == type)
                bushes.Add((int)npc.ai[2]);
        }

        return bushes.Count;
    }

    private void SpawnBush()
    {
        int total = Main.rand.Next(MinBushSize, MaxBushSize + 1);
        int originX = (int)(NPC.Center.X / 16f);
        int groundY = (int)(NPC.Bottom.Y / 16f);

        int left = Main.rand.Next(0, total);
        int right = total - 1 - left;

        int leftSpawned = SpawnRow(originX, -1, left, groundY);
        int rightSpawned = SpawnRow(originX, 1, right + (left - leftSpawned), groundY);

        int missing = right + (left - leftSpawned) - rightSpawned;
        if (missing > 0)
            SpawnRow(originX - leftSpawned - 1, -1, missing, groundY);
    }

    private int SpawnRow(int startX, int direction, int count, int groundY)
    {
        int spawned = 0;
        int x = startX;

        for (int n = 0; n < count; n++)
        {
            x += direction;

            if (!TryFindGround(x, groundY, out int y))
                break;

            int index = NPC.NewNPC(NPC.GetSource_FromAI(), x * 16 + 8, y * 16, Type);

            if (index >= 0 && index < Main.maxNPCs)
            {
                Main.npc[index].ai[1] = NPC.ai[1];
                Main.npc[index].ai[0] = Main.rand.Next(TicksPerStage / 2);
                Main.npc[index].ai[2] = NPC.ai[2];
                Main.npc[index].netUpdate = true;
            }

            spawned++;
        }

        return spawned;
    }

    private static bool TryFindGround(int x, int aroundY, out int y)
    {
        for (int dy = -2; dy <= 2; dy++)
        {
            int ty = aroundY + dy;
            Tile tile = Framing.GetTileSafely(x, ty);
            Tile above = Framing.GetTileSafely(x, ty - 1);

            if (tile.HasUnactuatedTile && tile.TileType == TileID.Grass && !above.HasTile)
            {
                y = ty;
                return true;
            }
        }

        y = 0;
        return false;
    }

    private static bool HasGround(NPC npc)
    {
        Tile tile = Framing.GetTileSafely((int)(npc.Center.X / 16f), (int)(npc.Bottom.Y / 16f));
        return tile.HasUnactuatedTile && Main.tileSolid[tile.TileType];
    }
}

internal sealed class VinePlayer : ModPlayer
{
    private const int DamageIntervalTicks = 45;

    internal bool InVines;
    internal bool DamageReady;

    private bool _wasInVines;
    private int _ticks;

    public override void PreUpdate()
    {
        _wasInVines = InVines;
        InVines = false;

        if (_wasInVines)
        {
            if (++_ticks >= DamageIntervalTicks)
                DamageReady = true;
        }
        else
        {
            _ticks = 0;
            DamageReady = false;
        }
    }

    public override void PostUpdateEquips()
    {
        if (_wasInVines)
            Player.noKnockback = true;
    }

    internal void ResetDamageTimer()
    {
        _ticks = 0;
        DamageReady = false;
    }
}

internal class VinesSpawn : GlobalNPC
{
    private const float Weight = 0.2f;

    private const int MaxBushes = 10;

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        if (spawnInfo.SpawnTileType != TileID.Grass)
            return;

        if (spawnInfo.PlayerInTown || spawnInfo.Invasion || !spawnInfo.Player.ZoneOverworldHeight)
            return;

        if (VinesOfTheMartyr.CountBushes() >= MaxBushes)
            return;

        pool[ModContent.NPCType<VinesOfTheMartyr>()] = Weight;
    }
}