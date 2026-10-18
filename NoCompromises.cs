namespace NoCompromises;

public sealed class NoCompromises : Mod
{
    public override void Load()
    {
        if (Main.dedServ)
            throw new Exception("No compromises mod работает только в одиночной игре!");
    }

    public override void PostSetupContent()
    {
        if (ModLoader.HasMod("CalamityMod"))
            throw new Exception("No compromises mod несовместим с Calamity Mod! Отключите один из модов.");
    }
}