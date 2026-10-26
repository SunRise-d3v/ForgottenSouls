namespace ForgottenSouls.Content.Debuffs;

internal sealed class ArachnophobiaDebuff : ModBuff
{
    public override string Texture => Helper.TextureLoadBuff("Debuffs", "ArachnophobiaDebuff");

    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
        Main.pvpBuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.statDefense -= 10;
        player.confused = true;
    }
}