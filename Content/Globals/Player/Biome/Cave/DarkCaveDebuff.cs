using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace NoCompromises.Content.Globals;

internal class DarkCaveDebuff : ModPlayer
{
    public override void PostUpdate()
    {
        bool inCave = Player.ZoneRockLayerHeight || Player.ZoneDirtLayerHeight;

        if (inCave)
        {
            Vector2 center = Player.Center / 32f;
            int x = (int)center.X;
            int y = (int)center.Y;

            float light = Lighting.GetSubLight(Player.Center).Length();

            if (light < 0.2f)
            {
                Player.AddBuff(BuffID.Darkness, 60);
                Main.buffNoTimeDisplay[BuffID.Darkness] = true;
            }
            else
            {
                Main.buffNoTimeDisplay[BuffID.Darkness] = false;
            }
        }
    }
}