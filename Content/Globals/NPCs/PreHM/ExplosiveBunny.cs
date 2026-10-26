namespace ForgottenSouls.Content.NPCs;

internal sealed class ExplosiveBunny : GlobalNPC
{
    private const int ChanceDenominator = 10;

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        Player player = spawnInfo.Player;

        if (!player.ZoneForest || !player.ZoneOverworldHeight)
            return;

        if (!Main.rand.NextBool(ChanceDenominator))
            return;

        pool[NPCID.ExplosiveBunny] = 1000f;
    }
}

internal sealed class ExplosiveBunnyBehavior : GlobalNPC
{
    private const float ChaseRange = 450f;
    private const float TriggerRange = 70f;
    private const float ChaseSpeed = 3.2f;
    private const int FuseTicks = 50;
    private const int ExplosionDamage = 60;

    private int _fuse;

    public override void SetDefaults(NPC npc)
    {
        npc.dontTakeDamageFromHostiles = true;
    }

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => npc.type == NPCID.ExplosiveBunny;

    public override void PostAI(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        Player target = Main.player[Player.FindClosest(npc.position, npc.width, npc.height)];

        if (CanSee(npc, target))
        {
            ChaseTarget(npc, target);

            if (_fuse == 0 && npc.Distance(target.Center) <= TriggerRange)
                _fuse = 1;
        }

        if (_fuse > 0)
            TickFuse(npc);
    }

    private static bool CanSee(NPC npc, Player target)
    {
        if (!target.active || target.dead)
            return false;

        if (npc.Distance(target.Center) > ChaseRange)
            return false;

        return Collision.CanHitLine(npc.position, npc.width, npc.height, target.position, target.width, target.height);
    }

    private static void ChaseTarget(NPC npc, Player target)
    {
        int direction = target.Center.X > npc.Center.X ? 1 : -1;

        npc.direction = npc.spriteDirection = direction;
        npc.velocity.X = MathHelper.Lerp(npc.velocity.X, direction * ChaseSpeed, 0.15f);

        if (npc.velocity.Y == 0f && npc.collideX)
            npc.velocity.Y = -5.5f;
    }

    private void TickFuse(NPC npc)
    {
        if (Main.rand.NextBool(3))
            Dust.NewDust(npc.position, npc.width, npc.height, DustID.Smoke, 0f, -1f);

        if (++_fuse >= FuseTicks)
            Explode(npc);
    }

    private static void Explode(NPC npc)
    {
        int index = Projectile.NewProjectile(
            npc.GetSource_Death(),
            npc.Center,
            Vector2.Zero,
            ProjectileID.Grenade,
            ExplosionDamage,
            6f,
            Main.myPlayer);

        if (index >= 0 && index < Main.maxProjectiles)
        {
            Projectile explosion = Main.projectile[index];
            explosion.friendly = false;
            explosion.hostile = true;
            explosion.timeLeft = 3;
            explosion.netUpdate = true;
        }

        npc.active = false;
        npc.netUpdate = true;
    }
}