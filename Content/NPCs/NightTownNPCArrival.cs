namespace ForgottenSouls.Content;

internal class NightTownNPCArrival : ModSystem
{
    private static Action _spawnTownNPCs;

    public override void Load()
    {
        MethodInfo method = typeof(Main).GetMethod(
            "UpdateTime_SpawnTownNPCs",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        if (method != null)
            _spawnTownNPCs = (Action)Delegate.CreateDelegate(typeof(Action), method);
        else
            Mod.Logger.Warn("Main.UpdateTime_SpawnTownNPCs не найден: ночные жители не будут приходить");
    }

    public override void Unload() => _spawnTownNPCs = null;

    public override void PostUpdateWorld()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || Main.dayTime || _spawnTownNPCs == null)
            return;

        Main.dayTime = true;
        try
        {
            _spawnTownNPCs();
        }
        finally
        {
            Main.dayTime = false;
        }
    }
}