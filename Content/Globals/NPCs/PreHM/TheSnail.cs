namespace ForgottenSouls.Content.NPCs;

[AutoloadBossHead]
internal sealed class TheSnail : ModNPC
{
    public override string Texture => "Terraria/Images/NPC_" + NPCID.Snail;

    public override string BossHeadTexture => Helper.TextureLoadNPC("TheSnail_Head_Boss");

    public override void BossHeadRotation(ref float rotation) => rotation = 0f;

    private const int SpawnDistanceMin = 75;
    private const int SpawnDistanceMax = 100;
    private const int RelocateDistanceMin = 150;
    private const int RelocateDistanceMax = 200;

    private const float CrawlSpeed = 0.55f;
    private const int RepathIntervalTicks = 30;
    private const int SearchRadius = 70;
    private const int GridSize = SearchRadius * 2 + 1;

    private static readonly Color OutlineColor = new(255, 175, 90, 0);

    private static readonly int[] Parent = new int[GridSize * GridSize];
    private static readonly int[] PathCost = new int[GridSize * GridSize];
    private static readonly PriorityQueue<int, int> Frontier = new();

    private static readonly Point[] Directions = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];

    private readonly List<Point> _path = [];
    private int _pathIndex;
    private int _repathTimer;
    private bool _pathBlocked;
    private Point? _moveTarget;
    private int _facing = -1;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Snail];

        NPCID.Sets.ImmuneToAllBuffs[Type] = true;
        NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers { Hide = true });
    }

    public override void SetDefaults()
    {
        NPC.width = 24;
        NPC.height = 16;
        NPC.lifeMax = 1;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.knockBackResist = 0f;
        NPC.aiStyle = -1;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.dontTakeDamage = true;
        NPC.immortal = true;
        NPC.lavaImmune = true;
        NPC.trapImmune = true;
        NPC.dontCountMe = true;
        NPC.npcSlots = 0f;
        NPC.value = 0f;
        NPC.HitSound = null;
        NPC.DeathSound = null;
        NPC.boss = false;
        NPC.BossBar = ModContent.GetInstance<SnailBossBar>();

        AnimationType = NPCID.Snail;

        for (int i = 0; i < NPC.buffImmune.Length; i++)
            NPC.buffImmune[i] = true;
    }

    public override bool CheckActive() => false;

    public override bool CheckDead()
    {
        NPC.life = NPC.lifeMax;
        return false;
    }

    //public override void BossHeadSlot(ref int index) => Helper.TextureLoadNPC("TheSnail_Head_Boss");

    public override void AI()
    {
        NPC.timeLeft = NPC.activeTime;
        NPC.velocity = Vector2.Zero;

        Player target = Main.player[Player.FindClosest(NPC.position, NPC.width, NPC.height)];

        if (!target.active || target.dead)
        {
            _moveTarget = null;
            return;
        }

        KillOnTouch(target);

        if (target.dead)
            return;

        Crawl(target);
    }

    private void KillOnTouch(Player target)
    {
        if (!NPC.Hitbox.Intersects(target.Hitbox))
            return;

        target.KillMe(PlayerDeathReason.ByNPC(NPC.whoAmI), target.statLifeMax2 + 1000, 0);
    }

    private void Crawl(Player target)
    {
        if (_repathTimer > 0)
            _repathTimer--;

        if (_moveTarget is null)
        {
            Point current = NPC.Center.ToTileCoordinates();
            NPC.Center = CellCenter(current);

            Point goal = target.Center.ToTileCoordinates();

            int feetCellY = (int)((target.Bottom.Y - 1f) / 16f);
            Point groundGoal = new(goal.X, feetCellY);

            if (HasGroundSupport(groundGoal.X, groundGoal.Y))
                goal = groundGoal;

            if (_repathTimer <= 0 || (!_pathBlocked && _pathIndex >= _path.Count))
            {
                ComputePath(current, goal);
                _repathTimer = RepathIntervalTicks;
            }

            if (_pathIndex < _path.Count)
                _moveTarget = _path[_pathIndex++];
            else if (_pathBlocked)
                _moveTarget = StepToward(current, goal);

            if (_moveTarget is null || _moveTarget == current)
            {
                _moveTarget = null;
                return;
            }

            TryOpenDoor(_moveTarget.Value, _moveTarget.Value.X - current.X);
        }

        Vector2 targetCenter = CellCenter(_moveTarget.Value);
        Vector2 delta = targetCenter - NPC.Center;

        Vector2 step;
        if (Math.Abs(delta.X) >= Math.Abs(delta.Y))
            step = new Vector2(Math.Sign(delta.X), 0f);
        else
            step = new Vector2(0f, Math.Sign(delta.Y));

        UpdateOrientation(step);

        float remaining = Math.Abs(delta.X) + Math.Abs(delta.Y);

        if (remaining <= CrawlSpeed)
        {
            NPC.Center = targetCenter;
            _moveTarget = null;
        }
        else
        {
            NPC.Center += step * CrawlSpeed;
        }
    }

    private void ComputePath(Point start, Point goal)
    {
        _path.Clear();
        _pathIndex = 0;
        _pathBlocked = false;

        Frontier.Clear();
        Array.Fill(PathCost, int.MaxValue);

        int originX = start.X - SearchRadius;
        int originY = start.Y - SearchRadius;
        int startIndex = SearchRadius * GridSize + SearchRadius;

        PathCost[startIndex] = 0;
        Parent[startIndex] = -1;
        Frontier.Enqueue(startIndex, 0);

        int best = startIndex;
        int bestDistance = Manhattan(start, goal);

        while (Frontier.TryDequeue(out int current, out int currentCost))
        {
            if (currentCost != PathCost[current])
                continue;

            int cx = current % GridSize + originX;
            int cy = current / GridSize + originY;
            int distance = Math.Abs(cx - goal.X) + Math.Abs(cy - goal.Y);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = current;
            }

            if (distance == 0)
            {
                best = current;
                break;
            }

            foreach (Point direction in Directions)
            {
                int nx = cx + direction.X;
                int ny = cy + direction.Y;
                int gx = nx - originX;
                int gy = ny - originY;

                if (gx < 0 || gy < 0 || gx >= GridSize || gy >= GridSize)
                    continue;

                if (!IsPassable(nx, ny))
                    continue;

                int index = gy * GridSize + gx;
                int newCost = currentCost + MovementCost(nx, ny, direction);

                if (newCost >= PathCost[index])
                    continue;

                PathCost[index] = newCost;
                Parent[index] = current;
                Frontier.Enqueue(index, newCost);
            }
        }


        if (best == startIndex)
        {
            _pathBlocked = true;
            return;
        }

        for (int node = best; node != startIndex; node = Parent[node])
            _path.Add(new Point(node % GridSize + originX, node / GridSize + originY));

        _path.Reverse();
    }

    private static int MovementCost(int x, int y, Point direction)
    {
        if (direction.Y != 0)
            return 45;

        return HasGroundSupport(x, y) ? 10 : 30;
    }

    private static bool HasGroundSupport(int x, int y)
    {
        if (!WorldGen.InWorld(x, y + 1, 10))
            return false;

        Tile below = Main.tile[x, y + 1];

        if (!below.HasUnactuatedTile)
            return false;

        int type = below.TileType;
        return Main.tileSolid[type] || Main.tileSolidTop[type];
    }

    private static int Manhattan(Point a, Point b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

    private static Point StepToward(Point current, Point goal)
    {
        int dx = goal.X - current.X;
        int dy = goal.Y - current.Y;

        if (dx == 0 && dy == 0)
            return current;

        return Math.Abs(dx) >= Math.Abs(dy)
            ? new Point(current.X + Math.Sign(dx), current.Y)
            : new Point(current.X, current.Y + Math.Sign(dy));
    }

    private static bool IsPassable(int x, int y)
    {
        if (!WorldGen.InWorld(x, y, 10))
            return false;

        Tile tile = Main.tile[x, y];

        if (!tile.HasUnactuatedTile)
            return true;

        int type = tile.TileType;

        if (!Main.tileSolid[type] || Main.tileSolidTop[type])
            return true;

        return type == TileID.ClosedDoor
               || type == TileID.TallGateClosed
               || type == TileID.TrapdoorClosed;
    }

    private static void TryOpenDoor(Point cell, int direction)
    {
        Tile tile = Main.tile[cell.X, cell.Y];

        if (tile.HasTile && tile.TileType == TileID.ClosedDoor)
            WorldGen.OpenDoor(cell.X, cell.Y, direction == 0 ? 1 : direction);
    }

    private static Vector2 CellCenter(Point cell) => new(cell.X * 16f + 8f, cell.Y * 16f + 8f);

    private void UpdateOrientation(Vector2 step)
    {
        if (step.X != 0f)
        {
            _facing = (int)step.X;
            NPC.rotation = 0f;
        }
        else if (step.Y != 0f)
        {
            float quarter = MathHelper.PiOver2;
            bool up = step.Y < 0f;
            NPC.rotation = (_facing == -1) == up ? quarter : -quarter;
        }

        NPC.spriteDirection = _facing;
        NPC.direction = _facing;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
            return true;

        Texture2D texture = TextureAssets.Npc[Type].Value;
        Rectangle frame = NPC.frame;
        Vector2 origin = frame.Size() / 2f;

        Vector2 position = NPC.position - screenPos
            + new Vector2(NPC.width / 2f, NPC.height - frame.Height * NPC.scale / 2f + 4f + NPC.gfxOffY);

        SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        float pulse = 0.75f + 0.25f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.5f);

        for (int i = 0; i < 8; i++)
        {
            Vector2 direction = (i * MathHelper.PiOver4).ToRotationVector2();

            Main.EntitySpriteDraw(texture, position + direction * 4f, frame, OutlineColor * (0.18f * pulse), NPC.rotation, origin, NPC.scale, effects, 0);
            Main.EntitySpriteDraw(texture, position + direction * 2f, frame, OutlineColor * (0.40f * pulse), NPC.rotation, origin, NPC.scale, effects, 0);
        }

        return true;
    }

    internal static void SpawnNear(Player player)
    {
        int type = ModContent.NPCType<TheSnail>();

        if (NPC.AnyNPCs(type))
            return;

        int side = Main.rand.NextBool() ? 1 : -1;
        int distance = Main.rand.Next(SpawnDistanceMin, SpawnDistanceMax + 1);
        Point cell = player.Center.ToTileCoordinates();
        cell.X += side * distance;

        cell = FindFreeCell(cell);

        int index = NPC.NewNPC(player.GetSource_Misc("Snail"), (int)CellCenter(cell).X, (int)CellCenter(cell).Y + 8, type);

        if (index >= 0 && index < Main.maxNPCs)
            Main.npc[index].Center = CellCenter(cell);
    }

    internal static void Relocate(Player player)
    {
        int index = NPC.FindFirstNPC(ModContent.NPCType<TheSnail>());

        if (index < 0)
            return;

        Point spawn = player.SpawnX >= 0 ? new Point(player.SpawnX, player.SpawnY) : new Point(Main.spawnTileX, Main.spawnTileY);

        int side = Main.rand.NextBool() ? 1 : -1;
        int distance = Main.rand.Next(RelocateDistanceMin, RelocateDistanceMax + 1);
        int x = spawn.X + side * distance;

        if (x < 60 || x > Main.maxTilesX - 60)
            x = spawn.X - side * distance;

        x = Math.Clamp(x, 60, Main.maxTilesX - 60);

        Point cell = FindFreeCell(new Point(x, spawn.Y));

        NPC snail = Main.npc[index];
        snail.Center = CellCenter(cell);

        if (snail.ModNPC is TheSnail theSnail)
            theSnail.ResetMovement();

        snail.netUpdate = true;
    }

    private void ResetMovement()
    {
        _path.Clear();
        _pathIndex = 0;
        _moveTarget = null;
        _repathTimer = 0;
        _pathBlocked = false;
    }

    private static Point FindFreeCell(Point cell)
    {
        for (int offset = 0; offset <= 40; offset++)
        {
            foreach (int sign in new[] { 1, -1 })
            {
                Point candidate = new(cell.X, cell.Y + offset * sign);

                if (IsPassable(candidate.X, candidate.Y))
                    return candidate;
            }
        }

        return cell;
    }

    internal static void SpawnAt(Vector2 center)
    {
        int type = ModContent.NPCType<TheSnail>();

        if (NPC.AnyNPCs(type))
            return;

        Point cell = FindFreeCell(center.ToTileCoordinates());
        int index = NPC.NewNPC(new EntitySource_WorldEvent("Snail"), (int)CellCenter(cell).X, (int)CellCenter(cell).Y + 8, type);

        if (index >= 0 && index < Main.maxNPCs)
        {
            Main.npc[index].Center = CellCenter(cell);
            Main.npc[index].netUpdate = true;
        }
    }
}

internal sealed class SnailPlayer : ModPlayer
{
    public override void OnEnterWorld()
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            TheSnail.SpawnNear(Player);
    }

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            TheSnail.Relocate(Player);
    }
}
internal sealed class SnailSystem : ModSystem
{
    private const int CheckInterval = 10;

    private static Vector2? lastCenter;
    private int timer;

    public override void ClearWorld() => lastCenter = null;

    public override void SaveWorldData(TagCompound tag)
    {
        int index = NPC.FindFirstNPC(ModContent.NPCType<TheSnail>());

        if (index >= 0)
            lastCenter = Main.npc[index].Center;

        if (lastCenter is Vector2 center)
        {
            tag["snailX"] = center.X;
            tag["snailY"] = center.Y;
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        if (tag.ContainsKey("snailX"))
            lastCenter = new Vector2(tag.GetFloat("snailX"), tag.GetFloat("snailY"));
    }

    public override void PostUpdateWorld()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || ++timer < CheckInterval)
            return;

        timer = 0;

        int index = NPC.FindFirstNPC(ModContent.NPCType<TheSnail>());

        if (!Sadomasochist.Active)
        {
            if (index >= 0)
                Remove(index);

            return;
        }

        if (index >= 0)
            return;

        Player player = FirstActivePlayer();

        if (player == null)
            return;

        if (lastCenter is Vector2 center)
            TheSnail.SpawnAt(center);
        else
            TheSnail.SpawnNear(player);
    }

    private static void Remove(int index)
    {
        NPC snail = Main.npc[index];
        lastCenter = snail.Center;

        snail.active = false;
        snail.netUpdate = true;

        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.SyncNPC, number: index);
    }

    private static Player FirstActivePlayer()
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            if (Main.player[i].active && !Main.player[i].dead)
                return Main.player[i];
        }

        return null;
    }
}