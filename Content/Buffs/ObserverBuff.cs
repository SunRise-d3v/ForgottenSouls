namespace ForgottenSouls.Content.Buffs;

internal sealed class ObserverBuff : ModBuff
{
    public override string Texture => Helper.TextureLoadBuff("ObserverBuff0");

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
        Main.buffNoSave[Type] = false;
        Main.buffNoTimeDisplay[Type] = true;
    }
}