namespace ForgottenSouls.Content.NPCs;

internal sealed class TortoiseOneShot : ModPlayer
{
    private int _tortoiseCooldown = 0;
    private const float Chance = 0.4f;

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
                    if (Main.rand.NextFloat() < Chance)
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
                    if (Main.rand.NextFloat() < Chance)
                    {
                        Player.Hurt(PlayerDeathReason.ByNPC(154), Player.statLife + 9999, 0);
                    }

                    _tortoiseCooldown = 60;
                }
            }
        }
    }
}