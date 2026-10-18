using Microsoft.Xna.Framework;

using NoCompromises.Content.Systems;

using Terraria.DataStructures;

namespace NoCompromises.Content.Globals.NPCs;

internal class BossAnnounce : GlobalNPC
{
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (Main.dedServ)
            return;

        Color mainColor = Color.White;
        string name = null;
        string subtitle = "";

        switch (npc.type)
        {
            // Pre-Hardmode
            case NPCID.KingSlime:
                name = "KING SLIME";
                subtitle = "Horrible Slush";
                mainColor = new Color(0, 71, 171);
                break;

            case NPCID.EyeofCthulhu:
                name = "EYE OF CTHULHU";
                subtitle = "Lonely Monster";
                mainColor = new Color(207, 155, 143);
                break;

            case NPCID.EaterofWorldsHead:
                name = "WORLD EVOURER";
                subtitle = "Omnivorous Worm";
                mainColor = new Color(66, 49, 137);
                break;

            case NPCID.BrainofCthulhu:
                name = "BRAIN OF CTHULHU";
                subtitle = "Extraterrestrial Intelligence";
                mainColor = new Color(163, 59, 57);
                break;

            case NPCID.QueenBee:
                name = "QUEEN BEES";
                subtitle = "Awakened From Honey";
                mainColor = new Color(247, 217, 23);
                break;

            case NPCID.SkeletronHead:
                name = "SKELETRON";
                subtitle = "Dungeon Guardian";
                mainColor = new Color(192, 192, 192);
                break;

            case NPCID.Deerclops:
                name = "CYCLOPS DEER";
                subtitle = "Through The Worlds";
                mainColor = new Color(153, 153, 153);
                break;

            case NPCID.WallofFlesh:
                name = "WALL OF FLESH";
                subtitle = "Keeper Of Darkness";
                mainColor = new Color(83, 3, 25);
                break;

            // Hardmode
            case NPCID.QueenSlimeBoss:
                name = "SLUGS QUEEN";
                subtitle = "Arrogant Spawn";
                mainColor = new Color(249, 132, 229);
                break;

            case NPCID.Spazmatism:
                name = "TWINS";
                subtitle = "Retinazer & Spazmatism";
                mainColor = new Color(0, 168, 107);
                break;

            case NPCID.TheDestroyer:
                name = "DESTROYER";
                subtitle = "Earth Loosener";
                mainColor = new Color(0, 0, 32);
                break;

            case NPCID.SkeletronPrime:
                name = "SKELETRON PRIME";
                subtitle = "Search Engine";
                mainColor = new Color(122, 127, 128);
                break;

            case NPCID.Plantera:
                name = "PLANTERA";
                subtitle = "Child Of The Jungle";
                mainColor = new Color(255, 192, 203);
                break;

            case NPCID.HallowBoss:
                name = "EMPRESS OF LIGHT";
                subtitle = "Eclipsing The Sun";
                mainColor = new Color(241, 215, 98);
                break;

            case NPCID.Golem:
                name = "GOLEM";
                subtitle = "Locked God";
                mainColor = new Color(54, 21, 9);
                break;

            case NPCID.DukeFishron:
                name = "DUKE FISHRON";
                subtitle = "Ancient Dragon";
                mainColor = new Color(102, 205, 170);
                break;

            case NPCID.CultistBoss:
                name = "CULTIST LUNATIC";
                subtitle = "Unique Among People";
                mainColor = new Color(0, 0, 200);
                break;

            case NPCID.MoonLordCore:
                name = "MOON LORD";
                subtitle = "Mighty Evil";
                mainColor = new Color(17, 96, 98);
                break;
        }

        if (!string.IsNullOrEmpty(name))
            BossTitleSystem.ShowBossTitle(name, subtitle, mainColor);
    }
}