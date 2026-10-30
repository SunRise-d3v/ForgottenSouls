namespace ForgottenSouls.Content;

internal sealed class WelcomeMessage : ModPlayer
{
    public override void OnEnterWorld()
    {
        Main.NewText("[c/00ff00:А так же вики по ]" +
                     "[c/36454F:Forgotten] " +
                     "[c/305CDE:Souls], " +
                     "[c/00ff00:доступна на: ]" +
                     "[c/1E90FF:https://fargosmods.wiki.gg]" +
                     "[c/00ff00:!]");
    }
}
