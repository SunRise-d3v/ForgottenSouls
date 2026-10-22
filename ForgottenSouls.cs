namespace ForgottenSouls;

public class ForgottenSouls : Mod
{
    public static bool Active => !Main.gameMenu && SadoDifficultySystem.Sadomasochist;
}