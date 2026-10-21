namespace ForgottenSouls.Content.Tiles;

internal sealed class TreeGrenade : ModSystem
{
    private const float Chance = 0.05f;
    private const int GrenadeDamage = 40;
    private const int FuseTicks = 150;

    private static readonly HashSet<Point> ShakenTrees = new();

    public override void Load()
    {
        On_WorldGen.ShakeTree += ShakeTreeDetour;
    }

    public override void Unload()
    {
        On_WorldGen.ShakeTree -= ShakeTreeDetour;
        ShakenTrees.Clear();
    }

    public override void OnWorldLoad() => ShakenTrees.Clear();

    public override void OnWorldUnload() => ShakenTrees.Clear();

    private static void ShakeTreeDetour(On_WorldGen.orig_ShakeTree orig, int i, int j)
    {
        orig(i, j);

        /*if (Main.netMode == NetmodeID.MultiplayerClient)
            return;*/

        if (!IsTrunk(i, j))
            return;

        if (!ShakenTrees.Add(GetTreeBottom(i, j)))
            return;

        if (Main.rand.NextFloat() >= Chance)
            return;

        Point top = GetTreeTop(i, j);
        SpawnGrenade(top.X, top.Y);
    }

    private static bool IsTrunk(int x, int y)
        => WorldGen.InWorld(x, y) && Main.tile[x, y].HasTile && TileID.Sets.IsATreeTrunk[Main.tile[x, y].TileType];

    private static Point GetTreeBottom(int x, int y)
    {
        while (IsTrunk(x, y + 1))
            y++;

        return new Point(x, y);
    }

    private static Point GetTreeTop(int x, int y)
    {
        while (IsTrunk(x, y - 1))
            y--;

        return new Point(x, y);
    }

    private static void SpawnGrenade(int tileX, int tileY)
    {
        Vector2 spawnPosition = new(tileX * 16 + 8, tileY * 16 + 8);

        int index = Projectile.NewProjectile(
            new EntitySource_TileBreak(tileX, tileY),
            spawnPosition,
            Vector2.Zero,
            ProjectileID.Grenade,
            GrenadeDamage,
            4f,
            Main.myPlayer);

        if (index < 0 || index >= Main.maxProjectiles)
            return;

        Projectile grenade = Main.projectile[index];
        grenade.friendly = false;
        grenade.hostile = true;
        grenade.timeLeft = FuseTicks;

        grenade.rotation = Main.rand.NextFloat(MathHelper.TwoPi);

        grenade.netUpdate = true;
    }
}