namespace NoCompromises.Content.Systems;

internal sealed class QuickRespawn : ModPlayer
{
    private const ushort RESPAWN_TIME = 300;

    public override void UpdateDead()
    {
        if (!Main.npc.Any(n => n.active && n.boss) && Player.respawnTimer > RESPAWN_TIME)
            Player.respawnTimer = RESPAWN_TIME;
    }
}