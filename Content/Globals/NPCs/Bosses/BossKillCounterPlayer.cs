namespace ForgottenSouls.Content.Globals;

internal sealed class BossKillCounterPlayer : ModPlayer
{
    public int EyeOfCthulhuKills = 0;

    public override void SaveData(TagCompound tag)
    {
        tag["EyeKills"] = EyeOfCthulhuKills;
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.ContainsKey("EyeKills"))
        {
            EyeOfCthulhuKills = tag.GetInt("EyeKills");
        }
    }
}