namespace ForgottenSouls.Content.Debuffs;

internal class ArachnophobiaDebuff : ModBuff
{
    public override string Texture => Helper.TextureLoad("Buffs/Debuff", "ArachnophobiaDebuff");

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