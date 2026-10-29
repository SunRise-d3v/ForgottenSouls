namespace ForgottenSouls.Content.NPCs;

internal sealed class SharkAttacks : GlobalNPC
{
    private const float AggroRange = 1600f;       
    private const float ChaseSpeed = 7f;   
    private const float ChaseLerp = 0.06f;    

    private const float MinAttackRange = 80f;
    private const float MaxAttackRange = 800f;
    private const int AttemptChance = 20;    

    private const int LeapWindupTicks = 20;   
    private const int DashCooldownMin = 90;  
    private const int DashCooldownMax = 150;
    private const float LeapMinUpSpeed = 10f;   
    private const float LeapMaxUpSpeed = 17f;    
    private const float LeapMaxSideSpeed = 12f;
    private const int LeapMaxTicks = 300;          
    private const int LeapMinAirTicks = 20;
    private const int MaxSurfaceDistanceTiles = 24;
    private const int StrandedHopTicks = 15;    

    private const int SpitWindupTicks = 30;
    private const int SpitCooldownMin = 600;   
    private const int SpitCooldownMax = 900;
    private const float BoltSpeed = 11f;

    private enum State
    {
        Swim,
        LeapWindup,
        Leaping,
        SpitWindup,
    }

    private State _state;
    private int _timer;
    private int _dashCooldown = 60;
    private int _spitCooldown = 300;
    private int _hopTimer;
    private float _leapSideSpeed;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => npc.type == NPCID.Shark;

    public override void PostAI(NPC npc)
    {
        if (!Sadomasochist.Active)
            return;

        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        switch (_state)
        {
            case State.Swim:
                UpdateSwim(npc);
                break;

            case State.LeapWindup:
            case State.SpitWindup:
                UpdateWindup(npc);
                break;

            case State.Leaping:
                UpdateLeaping(npc);
                break;
        }
    }

    private void UpdateSwim(NPC npc)
    {
        if (_dashCooldown > 0)
            _dashCooldown--;

        if (_spitCooldown > 0)
            _spitCooldown--;

        if (!npc.wet)
        {
            ReturnToWater(npc);
            return;
        }

        Player target = FindTarget(npc);
        if (target == null)
            return;

        Vector2 toTarget = (target.Center - npc.Center).SafeNormalize(Vector2.Zero);
        npc.velocity = Vector2.Lerp(npc.velocity, toTarget * ChaseSpeed, ChaseLerp);

        bool dashReady = _dashCooldown <= 0;
        bool spitReady = _spitCooldown <= 0;

        if (!dashReady && !spitReady)
            return;

        float distance = npc.Distance(target.Center);
        if (distance < MinAttackRange || distance > MaxAttackRange)
            return;

        if (!Collision.CanHitLine(npc.position, npc.width, npc.height, target.position, target.width, target.height))
            return;

        if (!Main.rand.NextBool(AttemptChance))
            return;

        bool canLeap = dashReady
            && target.Center.Y < npc.Center.Y + 100f
            && SurfaceDistanceTiles(npc) <= MaxSurfaceDistanceTiles;

        bool spit = spitReady && (!canLeap || Main.rand.NextBool(3));

        if (spit)
        {
            _state = State.SpitWindup;
            _timer = SpitWindupTicks;
        }
        else if (canLeap)
        {
            _state = State.LeapWindup;
            _timer = LeapWindupTicks;
        }
    }

    private void UpdateWindup(NPC npc)
    {
        npc.velocity *= 0.85f;

        Player target = FindTarget(npc);
        if (target == null)
        {
            _state = State.Swim;
            return;
        }

        npc.direction = npc.spriteDirection = target.Center.X > npc.Center.X ? 1 : -1;

        if (!Main.dedServ && Main.rand.NextBool(2))
            Dust.NewDust(npc.position, npc.width, npc.height, DustID.Water, 0f, -2f);

        if (--_timer > 0)
            return;

        if (_state == State.LeapWindup)
            StartLeap(npc, target);
        else
            Spit(npc, target);
    }

    private void StartLeap(NPC npc, Player target)
    {
        float dx = target.Center.X - npc.Center.X;

        float height = Math.Max(0f, npc.Center.Y - target.Center.Y);
        float upSpeed = MathHelper.Clamp((float)Math.Sqrt(0.6f * (height + 160f)), LeapMinUpSpeed, LeapMaxUpSpeed);

        _leapSideSpeed = MathHelper.Clamp(dx / 45f, -LeapMaxSideSpeed, LeapMaxSideSpeed);

        npc.velocity = new Vector2(_leapSideSpeed, -upSpeed);
        npc.direction = npc.spriteDirection = _leapSideSpeed >= 0f ? 1 : -1;
        npc.netUpdate = true;

        _state = State.Leaping;
        _timer = LeapMaxTicks;
        _hopTimer = 0;
    }

    private void UpdateLeaping(NPC npc)
    {
        _timer--;

        bool airborneLongEnough = _timer < LeapMaxTicks - LeapMinAirTicks;

        if (!npc.wet && npc.velocity.Y != 0f)
        {
            npc.velocity.X = MathHelper.Lerp(npc.velocity.X, _leapSideSpeed, 0.05f);
        }
        else if (!npc.wet && airborneLongEnough)
        {
            ReturnToWater(npc);
        }

        if ((airborneLongEnough && npc.wet) || _timer <= 0)
        {
            _state = State.Swim;
            _dashCooldown = Main.rand.Next(DashCooldownMin, DashCooldownMax + 1);
        }
    }

    private void Spit(NPC npc, Player target)
    {
        Vector2 mouth = npc.Center + new Vector2(npc.direction * npc.width * 0.5f, 0f);
        Vector2 velocity = (target.Center - mouth).SafeNormalize(Vector2.UnitX * npc.direction) * BoltSpeed;

        Projectile.NewProjectile(
            npc.GetSource_FromAI(),
            mouth,
            velocity,
            ProjectileID.SharknadoBolt,
            npc.damage / 2,
            1f,
            Main.myPlayer);

        _state = State.Swim;
        _spitCooldown = Main.rand.Next(SpitCooldownMin, SpitCooldownMax + 1);
    }

    private static Player FindTarget(NPC npc)
    {
        Player target = Main.player[Player.FindClosest(npc.position, npc.width, npc.height)];

        if (!target.active || target.dead || npc.Distance(target.Center) > AggroRange)
            return null;

        npc.target = target.whoAmI;
        return target;
    }

    private void ReturnToWater(NPC npc)
    {
        if (npc.velocity.Y != 0f)
            return;

        if (--_hopTimer > 0)
            return;

        _hopTimer = StrandedHopTicks;

        if (!TryFindWaterDirection(npc, out int direction))
            return;

        npc.velocity = new Vector2(direction * 5f, -6f);
        npc.direction = npc.spriteDirection = direction;
        npc.netUpdate = true;
    }

    private static bool TryFindWaterDirection(NPC npc, out int direction)
    {
        int x = (int)(npc.Center.X / 16f);
        int y = (int)(npc.Bottom.Y / 16f) - 1;

        for (int i = 1; i <= 40; i++)
        {
            foreach (int dir in new[] { -1, 1 })
            {
                if (Framing.GetTileSafely(x + dir * i, y).LiquidAmount > 0)
                {
                    direction = dir;
                    return true;
                }
            }
        }

        direction = 0;
        return false;
    }

    private static int SurfaceDistanceTiles(NPC npc)
    {
        int x = (int)(npc.Center.X / 16f);
        int y = (int)(npc.Center.Y / 16f);

        for (int i = 1; i <= MaxSurfaceDistanceTiles + 1; i++)
        {
            Tile tile = Framing.GetTileSafely(x, y - i);

            if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType])
                return int.MaxValue;

            if (tile.LiquidAmount == 0)
                return i;
        }

        return int.MaxValue;
    }
}