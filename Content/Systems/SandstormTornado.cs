namespace ForgottenSouls.Content.Systems;

internal sealed class SandstormTornado : ModSystem
{
    private const int MinInterval = 480;
    private const int MaxInterval = 900;
    private const int MinDistance = 20;
    private const int MaxDistance = 45;
    private const int MaxActive = 3;

    private int timer;

    public override void PostUpdateWorld()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !Sandstorm.Happening)
            return;

        if (--timer > 0)
            return;

        timer = Main.rand.Next(MinInterval, MaxInterval + 1);

        if (CountActive() >= MaxActive)
            return;

        List<Player> candidates = [];
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player p = Main.player[i];
            if (p.active && !p.dead && p.ZoneDesert && p.ZoneOverworldHeight)
                candidates.Add(p);
        }

        if (candidates.Count == 0)
            return;

        SpawnNear(candidates[Main.rand.Next(candidates.Count)]);
    }

    private static int CountActive()
    {
        int count = 0;
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile p = Main.projectile[i];
            if (p.active && (p.type == ProjectileID.SandnadoHostile || p.type == ProjectileID.SandnadoHostileMark))
                count++;
        }
        return count;
    }

    private static void SpawnNear(Player player)
    {
        int side = Main.rand.NextBool() ? 1 : -1;
        int x = (int)(player.Center.X / 16f) + side * Main.rand.Next(MinDistance, MaxDistance + 1);
        int startY = (int)(player.Center.Y / 16f) - 30;

        if (!WorldGen.InWorld(x, startY, 50))
            return;

        int groundY = -1;
        for (int y = startY; y < startY + 70; y++)
        {
            if (WorldGen.SolidTile(x, y) && !WorldGen.SolidTile(x, y - 1) && !WorldGen.SolidTile(x, y - 2) && !WorldGen.SolidTile(x, y - 3))
            {
                groundY = y;
                break;
            }
        }

        if (groundY < 0)
            return;

        int damage = Main.hardMode ? 45 : 20;
        Projectile tornado = Projectile.NewProjectileDirect(
            new EntitySource_WorldEvent("SandstormTornado"),
            new Vector2(x * 16f + 8f, groundY * 16f - 16f),
            Vector2.Zero,
            ProjectileID.SandnadoHostile,
            damage,
            0f,
            Main.myPlayer
        );

        tornado.timeLeft = 1200;
        tornado.alpha = 0;
        tornado.netUpdate = true;
    }
}