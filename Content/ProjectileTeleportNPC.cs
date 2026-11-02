namespace ForgottenSouls.Content.Globals;

/// <summary>
/// Caster NPCs (Tim, Rune Wizard, Necromancer...) react to projectiles fired by players:
/// when one is about to hit them there is a 50% chance they blink away to another spot.
/// </summary>
internal sealed class ProjectileTeleportNPC : GlobalNPC
{
    // ---- Tuning ----
    private const float Chance = 0.5f;         // chance to react to each dangerous projectile
    private const int Cooldown = 90;           // ticks between two teleports
    private const float DetectRange = 240f;    // projectiles farther than this are ignored (px)
    private const int PredictTicks = 8;        // how far ahead the flight of a projectile is checked
    private const int MinDistance = 8;         // teleport distance range (tiles)
    private const int MaxDistance = 16;
    private const float MinPlayerDistance = 120f; // do not appear right on top of a player (px)
    private const float MinProjectileDistance = 160f; // do not appear in the line of fire (px)
    private const int Attempts = 40;

    private static readonly HashSet<int> Casters =
    [
        NPCID.Tim,
        NPCID.RuneWizard,
        NPCID.Necromancer,
        NPCID.NecromancerArmored,
        NPCID.DarkCaster
    ];

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => Casters.Contains(entity.type);

    private int cooldown;
    private int lastProjectile = -1;
    private uint lastRollTick;
    private Vector2? lastCenter;

    public override void PostAI(NPC npc)
    {
        // Any sudden jump in position (ours or a vanilla teleport) gets a visual effect on every client
        Vector2 now = npc.Center;

        if (!Main.dedServ && lastCenter is Vector2 previous && Vector2.Distance(previous, now) > 120f)
            TeleportEffect(previous, now);

        lastCenter = now;

        if (cooldown > 0)
            cooldown--;

        // The decision is made by the server (or single-player)
        if (Main.netMode == NetmodeID.MultiplayerClient || cooldown > 0)
            return;

        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile projectile = Main.projectile[i];

            if (!IsThreat(npc, projectile))
                continue;

            // Roll once per projectile, not every tick it spends flying towards us
            if (projectile.identity == lastProjectile && Main.GameUpdateCount - lastRollTick < 120)
                continue;

            lastProjectile = projectile.identity;
            lastRollTick = Main.GameUpdateCount;

            if (Main.rand.NextFloat() >= Chance)
                continue;

            if (TryTeleport(npc, projectile))
            {
                cooldown = Cooldown;
                return;
            }
        }
    }

    // A player's projectile that will reach this NPC within a few ticks
    private static bool IsThreat(NPC npc, Projectile projectile)
    {
        if (!projectile.active || !projectile.friendly || projectile.damage <= 0 || projectile.minion || projectile.sentry)
            return false;

        if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
            return false;

        if (Vector2.Distance(projectile.Center, npc.Center) > DetectRange)
            return false;

        Rectangle target = npc.Hitbox;
        target.Inflate(8, 8);

        // Held spears barely move: react when they are already close to the target
        bool heldSpear = projectile.aiStyle == ProjAIStyleID.Spear;

        if (heldSpear)
            return Vector2.Distance(projectile.Center, npc.Center) < 90f;

        if (projectile.velocity.LengthSquared() < 1f)
            return false;

        for (int k = 0; k <= PredictTicks; k++)
        {
            Rectangle predicted = new(
                (int)(projectile.position.X + projectile.velocity.X * k),
                (int)(projectile.position.Y + projectile.velocity.Y * k),
                projectile.width,
                projectile.height);

            if (predicted.Intersects(target))
                return true;
        }

        return false;
    }

    private static bool TryTeleport(NPC npc, Projectile threat)
    {
        for (int attempt = 0; attempt < Attempts; attempt++)
        {
            float dx = (Main.rand.NextBool() ? 1 : -1) * Main.rand.Next(MinDistance, MaxDistance + 1) * 16f;
            float dy = Main.rand.Next(-6, 7) * 16f;
            Vector2 position = npc.position + new Vector2(dx, dy);

            if (!WorldGen.InWorld((int)(position.X / 16f), (int)(position.Y / 16f), 40))
                continue;

            if (Collision.SolidCollision(position, npc.width, npc.height))
                continue;

            // Fall down to the ground so the NPC does not appear in mid-air
            if (!SettleOnGround(ref position, npc.width, npc.height))
                continue;

            Vector2 center = position + new Vector2(npc.width, npc.height) / 2f;

            if (Collision.WetCollision(position, npc.width, npc.height))
                continue;

            if (Vector2.Distance(center, threat.Center) < MinProjectileDistance || NearAnyPlayer(center))
                continue;

            npc.position = position;
            npc.velocity = Vector2.Zero;
            npc.netUpdate = true;
            return true;
        }

        return false;
    }

    // Moves the spot down until there is floor under it (gives up after 16 tiles)
    private static bool SettleOnGround(ref Vector2 position, int width, int height)
    {
        for (int step = 0; step < 64; step++)
        {
            if (Collision.SolidCollision(position + new Vector2(0f, 4f), width, height))
                return true;

            position.Y += 4f;
        }

        return false;
    }

    private static bool NearAnyPlayer(Vector2 center)
    {
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player p = Main.player[i];

            if (p.active && !p.dead && Vector2.Distance(p.Center, center) < MinPlayerDistance)
                return true;
        }

        return false;
    }

    private static void TeleportEffect(Vector2 from, Vector2 to)
    {
        foreach (Vector2 spot in new[] { from, to })
        {
            for (int i = 0; i < 20; i++)
            {
                Dust d = Dust.NewDustPerfect(spot + Main.rand.NextVector2Circular(14f, 20f), DustID.PurpleTorch, Main.rand.NextVector2Circular(3f, 3f), 0, default, 1.4f);
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.Item8, spot);
        }
    }
}