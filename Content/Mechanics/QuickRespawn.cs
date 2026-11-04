namespace DisasterMod.Content.Mechanics;

internal sealed class QuickRespawn : ModPlayer
{
    internal const int DEFAULT_TICKS = 180;

    private bool _adjusted;

    internal static bool IsBossAlive()
        => Main.npc.Any(npc => npc.active && npc.boss);

    internal static int TargetTicks()
    {
        if (IsBossAlive())
            return -1;

        float seconds = ModContent.GetInstance<DisasterConfig>().RespawnSeconds;

        return (int)(seconds * 60f);
    }

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        => _adjusted = false;

    public override void UpdateDead()
    {
        int target = TargetTicks();

        if (target < 0)
        {
            _adjusted = false;
            return;
        }

        if (_adjusted)
            return;

        _adjusted = true;
        Player.respawnTimer = Math.Min(Player.respawnTimer, target);
    }
}
