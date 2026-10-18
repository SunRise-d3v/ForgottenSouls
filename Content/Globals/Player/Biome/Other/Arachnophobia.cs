using NoCompromises.Content.Buffs.DeBuffs;

namespace NoCompromises.Content.Globals;

internal class Arachnophobia : ModPlayer
{
    public override void PostUpdate()
    {
        bool surroundedBySpiders = false;

        if (!IsInSpiderNest())
        {
            int spiderCount = 0;
            float detectionRadius = 600f;

            foreach (NPC npc in Main.npc)
            {
                if (npc.active && npc.Distance(Player.Center) <= detectionRadius)
                {
                    if (IsSpider(npc))
                    {
                        spiderCount++;
                        if (spiderCount >= 3)
                        {
                            surroundedBySpiders = true;
                            break;
                        }
                    }
                }
            }
        }

        if (IsInSpiderNest() || surroundedBySpiders)
        {
            Player.AddBuff(ModContent.BuffType<ArachnophobiaDebuff>(), 60);
            //Main
        }
    }

    private static bool IsSpider(NPC npc)
    {
        return npc.type == NPCID.WallCreeper ||
               npc.type == NPCID.WallCreeperWall ||
               npc.type == NPCID.BlackRecluse ||
               npc.type == NPCID.BlackRecluseWall;
    }

    private bool IsInSpiderNest()
    {
        int tileX = (int)(Player.Center.X / 16f);
        int tileY = (int)(Player.Center.Y / 16f);

        if (WorldGen.InWorld(tileX, tileY, 10))
        {
            ushort wall = Main.tile[tileX, tileY].WallType;
            return Player.ZoneDirtLayerHeight && (wall == WallID.SpiderUnsafe);
        }

        return false;
    }
}