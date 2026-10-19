namespace ForgottenSouls.Content.NPCs;

internal class TortoiseOneShotPlayer : ModPlayer
{
    private int _tortoiseCooldown = 0;

    public override void PostUpdate()
    {
        if (_tortoiseCooldown > 0)
            _tortoiseCooldown--;

        foreach (NPC npc in Main.npc)
        {
            if (!npc.active)
                continue;

            if (npc.type == NPCID.GiantTortoise)
            {
                if (npc.Hitbox.Intersects(Player.Hitbox) && _tortoiseCooldown <= 0)
                {
                    if (Main.rand.NextFloat() < 0.4f)
                    {
                        Player.Hurt(PlayerDeathReason.ByNPC(153), Player.statLife + 9999, 0);
                    }

                    _tortoiseCooldown = 60;
                }
            }

            if (npc.type == NPCID.IceTortoise)
            {
                if (npc.Hitbox.Intersects(Player.Hitbox) && _tortoiseCooldown <= 0)
                {
                    if (Main.rand.NextFloat() < 0.4f)
                    {
                        Player.Hurt(PlayerDeathReason.ByNPC(154), Player.statLife + 9999, 0);
                    }

                    _tortoiseCooldown = 60;
                }
            }
        }
    }
}