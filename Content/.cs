internal static class Helper
{
    private const string _ROOT = "ForgottenSouls/Assets";

    public static string TextureLoadNPC(string directory, string fileName)
        => Build($"{_ROOT}/Textures", directory, fileName);

    public static string TextureLoadNPC(string fileName)
        => Build($"{_ROOT}/Textures", null, fileName);

    public static string TextureLoadBuff(string directory, string fileName)
        => Build($"{_ROOT}/Textures/Buffs", directory, fileName);

    public static string TextureLoadBuff(string fileName)
        => Build($"{_ROOT}/Textures/Buffs", null, fileName);

    public static string SoundLoad(string directory, string fileName)
        => Build($"{_ROOT}/Sounds", directory, fileName);

    public static string SoundLoad(string fileName)
        => Build($"{_ROOT}/Sounds", null, fileName);

    private static string Build(string root, string directory, string fileName)
    {
        fileName = fileName.Trim('/', '\\');

        if (string.IsNullOrWhiteSpace(directory))
            return $"{root}/{fileName}";

        directory = directory.Replace('\\', '/').Trim('/');
        return $"{root}/{directory}/{fileName}";
    }
}