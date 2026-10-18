using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoCompromises.Content.Globals;

internal class DesertDebuff : ModPlayer
{
    public override void PreUpdateBuffs()
    {
        if (Player.ZoneDesert)
            Player.AddBuff(BuffID.Slow, 60);
    }
}