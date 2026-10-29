namespace ForgottenSouls.Content.Mechanics;

internal sealed class HouseRaid : ModSystem
{
    public const int RaidIntervalDays = 7; 
    private const int BaseMobs = 6;     
    private const int MobsPerRaid = 3;   
    private const int MaxMobsTotal = 36;  
    private const int BaseAlive = 5;      
    private const int AliveStep = 2;    
    private const int MaxAlive = 18;      
    private const int SpawnMinTiles = 35;   
    private const int SpawnMaxTiles = 55;

    private static int daysPassed; 
    private static int raidNumber; 

    private static bool raidActive;
    private static int level;
    private static int toSpawn, spawned, maxAlive, spawnTimer;
    private static Point baseTile;
    private static bool wasDay = true;

    public override void ClearWorld()
    {
        daysPassed = 0;
        raidNumber = 0;
        raidActive = false;
    }

    public override void OnWorldLoad()
    {
        wasDay = Main.dayTime;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["days"] = daysPassed;
        tag["raids"] = raidNumber;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        daysPassed = tag.GetInt("days");
        raidNumber = tag.GetInt("raids");
    }

    public override void PostUpdateWorld()
    {
        if (!Sadomasochist.Active)
            return;

        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        bool day = Main.dayTime;

        if (day && !wasDay)
            daysPassed++;

        if (!day && wasDay && daysPassed >= RaidIntervalDays && !raidActive)
            StartRaid();

        wasDay = day;

        if (raidActive)
            UpdateRaid();
    }

    private static void StartRaid()
    {
        if (!FindBase(out baseTile))
            return;

        raidNumber++;
        daysPassed = 0;
        level = raidNumber;

        toSpawn = Math.Min(MaxMobsTotal, BaseMobs + MobsPerRaid * (raidNumber - 1));
        maxAlive = Math.Min(MaxAlive, BaseAlive + AliveStep * (raidNumber - 1));
        spawned = 0;
        spawnTimer = 120;
        raidActive = true;

        Announce($"Нападение на ваш дом! Волна №{raidNumber}: ожидается около {toSpawn} врагов.", new Color(255, 90, 60));
    }

    private static void UpdateRaid()
    {
        if (Main.dayTime)
        {
            raidActive = false;
            Announce("Рассвет. Нападение закончилось.", new Color(255, 220, 120));
            return;
        }

        if (--spawnTimer > 0)
            return;

        spawnTimer = Main.rand.Next(150, 300);

        int alive = CountAlive();

        if (spawned >= toSpawn)
        {
            if (alive == 0)
            {
                raidActive = false;
                Announce("Нападение отражено!", new Color(120, 255, 140));
            }
            return;
        }

        if (alive >= maxAlive)
            return;

        if (!FindBase(out baseTile))
        {
            raidActive = false;
            return;
        }

        int count = Math.Min(Main.rand.Next(1, 4), Math.Min(maxAlive - alive, toSpawn - spawned));
        for (int i = 0; i < count; i++)
        {
            if (SpawnMob())
                spawned++;
        }
    }

    private static int CountAlive()
    {
        int alive = 0;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.active && npc.TryGetGlobalNPC(out HouseRaidNPC raid) && raid.Level > 0)
                alive++;
        }
        return alive;
    }

    private static bool FindBase(out Point result)
    {
        long sumX = 0, sumY = 0;
        int n = 0;

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.active || !npc.townNPC || npc.homeless || npc.homeTileX <= 0 || npc.homeTileY <= 0)
                continue;

            sumX += npc.homeTileX;
            sumY += npc.homeTileY;
            n++;
        }

        result = n == 0 ? default : new Point((int)(sumX / n), (int)(sumY / n));
        return n > 0;
    }

    private static int[] MobPool()
    {
        if (Main.hardMode)
            return [NPCID.PossessedArmor, NPCID.Wraith, NPCID.Werewolf, NPCID.DemonEye];

        return [NPCID.Zombie, NPCID.DemonEye, NPCID.Skeleton];
    }

    private static bool SpawnMob()
    {
        int[] pool = MobPool();
        int type = pool[Main.rand.Next(pool.Length)];

        int side = Main.rand.NextBool() ? 1 : -1;
        int x = baseTile.X + side * Main.rand.Next(SpawnMinTiles, SpawnMaxTiles + 1);
        if (!WorldGen.InWorld(x, baseTile.Y, 50))
            return false;

        int groundY = -1;
        for (int y = baseTile.Y - 40; y < baseTile.Y + 60; y++)
        {
            if (WorldGen.SolidTile(x, y) && !WorldGen.SolidTile(x, y - 1) && !WorldGen.SolidTile(x, y - 2) && !WorldGen.SolidTile(x, y - 3))
            {
                groundY = y;
                break;
            }
        }

        if (groundY < 0)
            return false;

        NPC npc = NPC.NewNPCDirect(new EntitySource_WorldEvent("HouseRaid"), x * 16 + 8, groundY * 16, type);
        if (npc == null || !npc.active)
            return false;

        if (npc.TryGetGlobalNPC(out HouseRaidNPC raid))
            raid.Setup(level, baseTile);

        npc.netUpdate = true;
        return true;
    }

    private static void Announce(string text, Color color)
    {
        if (Main.netMode == NetmodeID.Server)
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), color);
        else
            Main.NewText(text, color);
    }
}

internal sealed class HouseRaidNPC : GlobalNPC
{
    private const float LifePerLevel = 0.12f;
    private const float MaxLifeMult = 2.2f;
    private const float DamagePerLevel = 0.06f;
    private const float MaxDamageMult = 1.6f;
    private const int DefensePerLevel = 1;
    private const int MaxDefenseBonus = 8;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => lateInstantiation && !entity.friendly && !entity.townNPC && !entity.boss;

    public int Level;
    private short baseX, baseY;
    private bool scaled;

    public void Setup(int level, Point baseTile)
    {
        Level = Math.Min(level, 255);
        baseX = (short)baseTile.X;
        baseY = (short)baseTile.Y;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.Write((byte)Level);
        binaryWriter.Write(baseX);
        binaryWriter.Write(baseY);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        Level = binaryReader.ReadByte();
        baseX = binaryReader.ReadInt16();
        baseY = binaryReader.ReadInt16();

        if (Level > 0 && !scaled && Main.netMode == NetmodeID.MultiplayerClient)
        {
            scaled = true;
            ApplyScaling(npc, false);
        }
    }

    private void ApplyScaling(NPC npc, bool scaleLife)
    {
        if (scaleLife)
        {
            float lifeMult = Math.Min(MaxLifeMult, 1f + LifePerLevel * Level);
            npc.lifeMax = (int)(npc.lifeMax * lifeMult);
            npc.life = (int)(npc.life * lifeMult);
        }

        float damageMult = Math.Min(MaxDamageMult, 1f + DamagePerLevel * Level);
        npc.damage = (int)(npc.damage * damageMult);
        npc.defDamage = npc.damage;
        npc.defense += Math.Min(MaxDefenseBonus, DefensePerLevel * Level);
        npc.defDefense = npc.defense;
    }

    public override bool PreAI(NPC npc)
    {
        if (!Sadomasochist.Active)
            return false;

        if (Level > 0 && !scaled && Main.netMode != NetmodeID.MultiplayerClient)
        {
            scaled = true;
            ApplyScaling(npc, true);
            npc.netUpdate = true;
        }

        return true;
    }

    public override void PostAI(NPC npc)
    {
        if (!Sadomasochist.Active)
            return;

        if (Level <= 0)
            return;

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player p = Main.player[i];
            if (p.active && !p.dead && Vector2.Distance(p.Center, npc.Center) < 600f)
                return;
        }

        Vector2 target = new(baseX * 16f + 8f, baseY * 16f);
        Vector2 toTarget = target - npc.Center;

        if (toTarget.Length() < 400f)
            return;

        if (npc.noGravity)
        {
            npc.velocity += toTarget.SafeNormalize(Vector2.Zero) * 0.06f;

            if (npc.velocity.Length() > 4f)
                npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * 4f;
        }
        else
        {
            int dir = Math.Sign(toTarget.X);
            npc.velocity.X = MathHelper.Clamp(npc.velocity.X + dir * 0.07f, -2.6f, 2.6f);
            npc.direction = npc.spriteDirection = dir;
        }
    }
}