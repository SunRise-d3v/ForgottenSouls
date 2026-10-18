namespace NoCompromises.Content.Buffs.DeBuffs;

internal class ArachnophobiaDebuff : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
        Main.pvpBuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    {
        player.statDefense -= 5;
        player.AddBuff(BuffID.Confused, 60);
    }
}