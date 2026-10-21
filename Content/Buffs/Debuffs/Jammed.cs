namespace ForgottenSouls.Content.Debuffs;

internal sealed class Jammed : ModBuff
{
    public override string Texture => Helper.TextureLoad("Buffs/Debuff", "Jammed");

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoSave[Type] = true;
    }
}