using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoCompromises.Content.Globals;

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