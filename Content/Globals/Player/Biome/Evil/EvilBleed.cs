namespace ForgottenSouls.Content.Biome;

internal class EvilBleed : ModPlayer
{
    public override void PreUpdateBuffs()
    {
        if (Player.ZoneCorrupt || Player.ZoneCrimson)
        {
            Player.AddBuff(BuffID.Bleeding, 60);
            Main.buffNoTimeDisplay[BuffID.Bleeding] = true;
        }
        else
        {
            Main.buffNoTimeDisplay[BuffID.Bleeding] = false;
        }
    }
}