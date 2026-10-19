namespace CrossMod;

internal static class Mods
{
    public const string FargowiltasSouls = "FargowiltasSouls";
    public const string Fargowiltas = "Fargowiltas";
    public const string MagicStorage = "MagicStorage";
    public const string BossChecklist = "BossChecklist";
    public const string Calamity = "CalamityMod";
}

internal static class ModCompat
{
    public static bool MagicStorage => ModLoader.HasMod(Mods.MagicStorage);
    public static bool BossChecklist => ModLoader.HasMod(Mods.BossChecklist);
    public static bool Calamity => ModLoader.HasMod(Mods.Calamity);
}