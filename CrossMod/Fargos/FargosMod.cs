namespace CrossMod;

internal static class FargosMod
{
    public static bool IsLoaded => ModLoader.HasMod(Mods.FargowiltasSouls) && ModLoader.HasMod(Mods.Fargowiltas);

    public static bool EternityMode => CallBool("EternityMode");
    public static bool MasochistMod => CallBool("MasochistMod");

    public static bool GiftsReceived => CallBool("GiftsReceived");

    private static bool CallBool(string method)
    {
        if (!ModLoader.TryGetMod(Mods.FargowiltasSouls, out Mod souls))
            return false;

        try
        {
            return souls.Call(method) is true;
        }
        catch
        {
            return false;
        }
    }
}