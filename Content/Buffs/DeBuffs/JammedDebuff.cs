namespace ForgottenSouls.Content.Debuffs;

internal sealed class JammedDebuff : ModBuff
{
    public override string Texture => Helper.TextureLoadBuff("Debuffs", "Jammed");

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }
}