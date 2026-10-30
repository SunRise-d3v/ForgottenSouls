namespace ForgottenSouls.Content.Biomes;

internal static class SubBiomeMusic
{
    private static readonly Dictionary<string, int> Cache = [];

    internal static int GetSlot(string name)
    {
        if (Cache.TryGetValue(name, out int slot))
            return slot;

        string path = $"ForgottenSouls/Assets/Musics/{name}";

        if (!MusicLoader.MusicExists(path))
        {
            Main.NewText($"Music not found: {path}");
            slot = MusicID.Underground;
        }
        else
        {
            slot = MusicLoader.GetMusicSlot(path);
            Main.NewText($"Music loaded: {path}, slot: {slot}");
        }

        Cache[name] = slot;
        return slot;
    }
}

internal class MarbleCaveMusic : ModSceneEffect
{
    public override int Music =>
        SubBiomeMusic.GetSlot("MarbleCave");

    public override SceneEffectPriority Priority =>
        SceneEffectPriority.BiomeHigh;

    public override bool IsSceneEffectActive(Player player) =>
        player.ZoneMarble;
}

internal class GraniteCaveMusic : ModSceneEffect
{
    public override int Music =>
        SubBiomeMusic.GetSlot("GraniteCave");

    public override SceneEffectPriority Priority =>
        SceneEffectPriority.BiomeHigh;

    public override bool IsSceneEffectActive(Player player) =>
        player.ZoneGranite;
}

internal class SpiderNestMusic : ModSceneEffect
{
    public override int Music =>
        SubBiomeMusic.GetSlot("SpiderNest");

    public override SceneEffectPriority Priority =>
        SceneEffectPriority.BiomeHigh;

    public override bool IsSceneEffectActive(Player player) =>
        player.GetModPlayer<SpiderNest>().InNest;
}

internal class SpiderNest : ModPlayer
{
    private const int RangeX = 20;
    private const int RangeY = 20;
    private const int WallThreshold = 100;
    private const int ScanInterval = 45;

    public bool InNest;

    private int _timer;

    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        if (++_timer < ScanInterval)
            return;

        _timer = 0;

        Point center = Player.Center.ToTileCoordinates();
        int count = 0;

        for (int x = center.X - RangeX; x <= center.X + RangeX; x++)
        {
            for (int y = center.Y - RangeY; y <= center.Y + RangeY; y++)
            {
                if (!WorldGen.InWorld(x, y, 5))
                    continue;

                if (Main.tile[x, y].WallType == WallID.SpiderUnsafe)
                    count++;
            }
        }

        InNest = count >= WallThreshold;
    }
}