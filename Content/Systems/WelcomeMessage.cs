namespace DisasterMod.Content;

internal sealed class WelcomeMessage : ModPlayer
{
    public override void OnEnterWorld()
    {
        Main.NewText("Hello");
    }
}