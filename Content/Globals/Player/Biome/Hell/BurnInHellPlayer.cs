internal class BurnInHellPlayer : ModPlayer
{
    public override void PreUpdateBuffs()
    {
        if (Player.ZoneUnderworldHeight)
        {
            bool hasFireProtection =
                Player.lavaImmune ||
                Player.fireWalk ||
                Player.HasBuff(BuffID.ObsidianSkin);

            Main.buffNoTimeDisplay[BuffID.OnFire] = true;

            if (!hasFireProtection)
                Player.AddBuff(BuffID.OnFire, 60);
        }
        else
        {
            Main.buffNoTimeDisplay[BuffID.OnFire] = false;
        }
    }
}