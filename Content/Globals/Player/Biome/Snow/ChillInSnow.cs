namespace NoCompromises.Content.Globals;

internal class ChillInSnow : ModPlayer
{
    public override void PreUpdateBuffs()
    {
        if (Player.ZoneSnow)
        {
            Player.AddBuff(BuffID.Chilled, 60);
            Main.buffNoTimeDisplay[BuffID.Chilled] = true;
        }
        else
        {
            Main.buffNoTimeDisplay[BuffID.Chilled] = false;
        }
    }
}