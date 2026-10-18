using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoCompromises.Content.Globals;

internal class InstantLavaDeath : ModPlayer
{
    public override void PostUpdate()
    {
        if (Player.lavaWet && !HasLavaProtection())
        {
            Player.statLife = 0;

            Player.KillMe(Terraria.DataStructures.PlayerDeathReason.ByOther(2), 9999, 0);
        }
    }

    private bool HasLavaProtection()
    {
        return Player.lavaImmune || Player.HasBuff(BuffID.ObsidianSkin);
    }
}