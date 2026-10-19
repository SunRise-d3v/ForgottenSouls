namespace ForgottenSouls.Content.Buffs;

internal class ObserverBuff : ModBuff
{
    public override string Texture => ForgottenHelper.TextureLoad("Buffs", "ObserverBuff");

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
        Main.buffNoSave[Type] = false;
        Main.buffNoTimeDisplay[Type] = true;
    }
}