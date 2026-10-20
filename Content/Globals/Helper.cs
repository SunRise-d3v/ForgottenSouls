namespace ForgottenSouls;

internal static class Helper
{
    public static string TextureLoad(string directory, string fileName)
        => "ForgottenSouls/Assets/Textures/" + directory + '/' + fileName;
    
    public static string SoundLoad(string directory, string fileName)
        => "ForgottenSouls/Assets/Sounds/" + directory + '/' + fileName;
}