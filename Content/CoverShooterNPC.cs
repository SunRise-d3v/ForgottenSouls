namespace ForgottenSouls.Content.Globals;

public struct ShooterConfig
{
    public int Projectile;
    public float ShotSpeed;
    public float Range;
    public int HideMin;
    public int HideMax;
    public float WalkSpeed;
}

public static class CoverConfig
{
    public static readonly Dictionary<int, ShooterConfig> Shooters = new()
    {
        { NPCID.PirateDeadeye,   new ShooterConfig { Projectile = ProjectileID.BulletDeadeye, ShotSpeed = 10f, Range = 700f,  HideMin = 40, HideMax = 90,  WalkSpeed = 2.4f } },
        { NPCID.PirateCrossbower,new ShooterConfig { Projectile = ProjectileID.BulletDeadeye, ShotSpeed = 10f, Range = 700f,  HideMin = 40, HideMax = 90,  WalkSpeed = 2.4f } },
        { NPCID.PirateCaptain,   new ShooterConfig { Projectile = ProjectileID.BulletDeadeye, ShotSpeed = 11f, Range = 700f,  HideMin = 40, HideMax = 90,  WalkSpeed = 2.6f } },
        { NPCID.SkeletonSniper,  new ShooterConfig { Projectile = ProjectileID.SniperBullet,  ShotSpeed = 13f, Range = 1000f, HideMin = 70, HideMax = 140, WalkSpeed = 2.2f } },
    };

    public const int MaxCoverSearch = 14;
    public const int MinDistanceToPlayer = 112;
}

public class CoverShooterNPC : GlobalNPC
{
    private enum State { Normal, MoveToCover, Hide, Peek }

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => CoverConfig.Shooters.ContainsKey(entity.type);

    private State state = State.Normal;
    private int coverX, hideX;
    private int timer, moveTimer, peekTimer, checkTimer, cooldown, blindPeeks;
    private bool shot;

    public override bool PreAI(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return false;

        npc.TargetClosest(true);
        Player p = Main.player[npc.target];
        ShooterConfig cfg = CoverConfig.Shooters[npc.type];

        if (!p.active || p.dead)
        {
            state = State.Normal;
            return true;
        }

        float dist = Vector2.Distance(npc.Center, p.Center);
        if (dist > cfg.Range || dist < CoverConfig.MinDistanceToPlayer)
        {
            if (state != State.Normal)
            {
                state = State.Normal;
                cooldown = 60;
                npc.netUpdate = true;
            }
            return true;
        }

        if (cooldown > 0)
            cooldown--;

        if (state == State.Normal)
        {
            if (cooldown <= 0)
            {
                if (FindCover(npc, p, out coverX, out hideX))
                {
                    state = State.MoveToCover;
                    moveTimer = 0;
                    npc.netUpdate = true;
                }
                else
                {
                    cooldown = 45;
                }
            }

            if (state == State.Normal)
                return true;
        }

        int facing = Math.Sign(p.Center.X - npc.Center.X);

        switch (state)
        {
            case State.MoveToCover:
                {
                    float dx = hideX * 16f + 8f - npc.Center.X;

                    if (++moveTimer > 300)
                    {
                        Abort(npc);
                        break;
                    }

                    if (Math.Abs(dx) < 5f)
                    {
                        npc.velocity.X = 0f;
                        state = State.Hide;
                        timer = Main.rand.Next(cfg.HideMin, cfg.HideMax + 1);
                        checkTimer = 0;
                        npc.netUpdate = true;
                    }
                    else
                    {
                        int dir = Math.Sign(dx);
                        npc.velocity.X = dir * cfg.WalkSpeed;
                        npc.direction = npc.spriteDirection = dir;
                        if (npc.collideX && npc.velocity.Y == 0f)
                            npc.velocity.Y = -6f;
                    }
                    break;
                }

            case State.Hide:
                {
                    npc.velocity.X = 0f;
                    if (facing != 0)
                        npc.direction = npc.spriteDirection = facing;

                    if (++checkTimer % 30 == 0 && !IsCoverColumn(coverX, FeetRow(npc), CoverHeight(npc)))
                    {
                        Abort(npc);
                        break;
                    }

                    if (--timer <= 0 && npc.velocity.Y == 0f)
                    {
                        float rise = CoverHeight(npc) * 16f - npc.height / 2f + 16f;
                        npc.velocity.Y = -(float)Math.Sqrt(2f * 0.3f * rise) * 1.08f;
                        state = State.Peek;
                        peekTimer = 70;
                        shot = false;
                        npc.netUpdate = true;
                    }
                    break;
                }

            case State.Peek:
                {
                    npc.velocity.X = 0f;
                    if (facing != 0)
                        npc.direction = npc.spriteDirection = facing;

                    peekTimer--;

                    if (!shot && npc.velocity.Y > -1f)
                    {
                        shot = true;
                        if (Collision.CanHit(npc.position, npc.width, npc.height, p.position, p.width, p.height))
                        {
                            Shoot(npc, p, cfg);
                            blindPeeks = 0;
                        }
                        else
                        {
                            blindPeeks++;
                        }
                    }

                    bool landed = npc.collideY && peekTimer < 60;
                    if (peekTimer <= 0 || (shot && landed))
                    {
                        if (blindPeeks >= 3)
                        {
                            blindPeeks = 0;
                            Abort(npc);
                            cooldown = 240;
                            break;
                        }

                        state = State.Hide;
                        timer = Main.rand.Next(cfg.HideMin, cfg.HideMax + 1);
                        npc.netUpdate = true;
                    }
                    break;
                }
        }

        if (Main.netMode == NetmodeID.Server && Main.GameUpdateCount % 15 == 0)
            npc.netUpdate = true;

        return false;
    }

    private void Abort(NPC npc)
    {
        state = State.Normal;
        cooldown = 180;
        npc.netUpdate = true;
    }

    private static void Shoot(NPC npc, Player p, ShooterConfig cfg)
    {
        Vector2 origin = npc.Center + new Vector2(npc.direction * 12f, -6f);
        Vector2 dir = p.Center - origin;
        if (dir == Vector2.Zero)
            return;
        dir.Normalize();

        int damage = npc.GetAttackDamage_ForProjectiles(npc.damage, npc.defDamage);
        Projectile.NewProjectile(npc.GetSource_FromAI(), origin, dir * cfg.ShotSpeed, cfg.Projectile, damage, 1f, Main.myPlayer);
    }

    private static int FeetRow(NPC npc) => (int)((npc.position.Y + npc.height - 1f) / 16f);

    private static int Rows(NPC npc) => (int)Math.Ceiling(npc.height / 16f);

    private static int CoverHeight(NPC npc) => Math.Max(2, (int)(npc.height / 16f));

    private static bool Solid(int x, int y) => WorldGen.InWorld(x, y, 2) && WorldGen.SolidTile(x, y);

    private static bool IsCoverColumn(int x, int feetRow, int coverH)
    {
        for (int r = 0; r < coverH; r++)
        {
            if (!Solid(x, feetRow - r))
                return false;
        }
        return !Solid(x, feetRow - coverH) && !Solid(x, feetRow - coverH - 1);
    }

    private static bool IsStandable(int x, int feetRow, int rows)
    {
        if (!Solid(x, feetRow + 1))
            return false;
        for (int r = 0; r < rows + 3; r++)
        {
            if (Solid(x, feetRow - r))
                return false;
        }
        return true;
    }

    private bool FindCover(NPC npc, Player p, out int cover, out int hide)
    {
        cover = hide = 0;

        int feet = FeetRow(npc);
        int rows = Rows(npc);
        int coverH = CoverHeight(npc);
        int startX = (int)(npc.Center.X / 16f);
        int playerX = (int)(p.Center.X / 16f);
        int s = playerX > startX ? 1 : -1;

        for (int k = 1; k <= CoverConfig.MaxCoverSearch; k++)
        {
            int cx = startX + s * k;
            int hx = cx - s;

            if (!IsStandable(hx, feet, rows))
                break;

            if (!IsCoverColumn(cx, feet, coverH))
                continue;

            if (Math.Abs(playerX - hx) < 8)
                break;

            cover = cx;
            hide = hx;
            return true;
        }
        return false;
    }
}