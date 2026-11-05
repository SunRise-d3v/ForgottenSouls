namespace DisasterMod;

internal static class Helper
{
    private const string Root = "DisasterMod/Assets";

    public static string LoadTexture(string directory, string fileName)
       => BuildAssetPath("Textures", directory == null ? "" : $"{directory}", fileName);

    public static string LoadNpcTexture(string directory, string fileName)
        => BuildAssetPath("Textures/NPCs", directory == null ? "" : $"{directory}", fileName);

    public static string LoadItemTexture(string directory, string fileName)
        => BuildAssetPath("Textures/Items", directory == null ? "" : $"{directory}", fileName);

    public static string LoadBuffTexture(string directory, string fileName)
        => BuildAssetPath("Textures/Buffs", directory == null ? "" : $"{directory}", fileName);

    public static string LoadUITexture(string directory, string fileName)
        => BuildAssetPath("Textures/UI", directory == null ? "" : $"{directory}", fileName);

    public static string LoadSound(string directory, string fileName)
        => BuildAssetPath("Sounds", directory == null ? "" : $"{directory}", fileName);

    public static string LoadSFX(string directory, string fileName)
        => BuildAssetPath("Sounds/SFX", directory == null ? "" : $"{directory}", fileName);

    public static string LoadShader(string directory, string fileName)
        => BuildAssetPath("Shaders/.xnb", directory == null ? "" : $"{directory}", fileName);

    private static string BuildAssetPath(string path, string directory, string fileName)
        => string.Join("/", new[] { Root, path, directory, fileName }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Replace('\\', '/').Trim('/')));
}