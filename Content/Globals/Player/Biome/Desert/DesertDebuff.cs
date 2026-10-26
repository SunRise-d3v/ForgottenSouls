namespace ForgottenSouls.Content.Biomes;

internal sealed class DesertDebuff : ModPlayer
{
    public override void PreUpdateBuffs()
    {
        if (Player.ZoneDesert)
        {
            Player.AddBuff(BuffID.Slow, 60);
            Main.buffNoTimeDisplay[BuffID.Slow] = true;
        }
        else
        {
            Main.buffNoTimeDisplay[BuffID.Slow] = false;
        }
    }
}