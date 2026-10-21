namespace ForgottenSouls.Content.NPCs;

internal sealed class BossAnnounce : GlobalNPC
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
            #region Mini-Boss
            case NPCID.DD2Betsy:
                name = "BETSY";
                subtitle = "Mother of Dragons";
                mainColor = Color.Red;
                break;

            case NPCID.Pumpking:
                name = "PUMP KING";
                subtitle = "Garden Scarecrow";
                mainColor = Color.Orange;
                break;

            case NPCID.IceQueen:
                name = "ICE QUEEN";
                subtitle = "Frozen Heart";
                mainColor = Color.CadetBlue;
                break;

            case NPCID.MartianSaucer:
                name = "MARTIAN SAUCER";
                subtitle = "Martian Invention";
                mainColor = Color.Gray;
                break;

            case NPCID.BloodNautilus:
                name = "DREAD NAUTILUS";
                subtitle = "Mollusk Overgrown";
                mainColor = Color.DarkRed;
                break;
            #endregion

            #region Pre-Hardmode
            case NPCID.KingSlime:
                name = "KING SLIME";
                subtitle = "Horrible Slush";
                mainColor = Color.WhiteSmoke;
                break;

            case NPCID.EyeofCthulhu:
                name = "EYE OF CTHULHU";
                subtitle = "Lonely Monster";
                mainColor = Color.Cyan;
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
                mainColor = Color.YellowGreen;
                break;

            case NPCID.SkeletronHead:
                name = "SKELETRON";
                subtitle = "Dungeon Guardian";
                mainColor = Color.Purple;
                break;

            case NPCID.Deerclops:
                name = "CYCLOPS DEER";
                subtitle = "Through The Worlds";
                mainColor = Color.IndianRed;
                break;

            case NPCID.WallofFlesh:
                name = "WALL OF FLESH";
                subtitle = "Keeper Of Darkness";
                mainColor = Color.LightGray;
                break;
            #endregion

            #region Hardmode
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
                mainColor = Color.LightSlateGray;
                break;

            case NPCID.SkeletronPrime:
                name = "SKELETRON PRIME";
                subtitle = "Search Engine";
                mainColor = new Color(122, 127, 128);
                break;

            case NPCID.Plantera:
                name = "PLANTERA";
                subtitle = "Child Of The Jungle";
                mainColor = Color.RoyalBlue;
                break;

            case NPCID.HallowBoss:
                name = "EMPRESS OF LIGHT";
                subtitle = "Eclipsing The Moon";
                mainColor = Color.Black;
                break;

            case NPCID.Golem:
                name = "GOLEM";
                subtitle = "Locked God";
                mainColor = Color.DarkSlateGray;
                break;

            case NPCID.DukeFishron:
                name = "DUKE FISHRON";
                subtitle = "Ancient Dragon";
                mainColor = new Color(102, 205, 170);
                break;

            case NPCID.CultistBoss:
                name = "CULTIST LUNATIC";
                subtitle = "Unique Among People";
                mainColor = Color.PaleGoldenrod;
                break;

            case NPCID.MoonLordCore:
                name = "MOON LORD";
                subtitle = "Mighty Evil";
                mainColor = Color.DarkOliveGreen;
                break;
                #endregion
        }

        #region Mods-Boss
        //Pre-Hardmode
        if (npc.type == ModContent.NPCType<TrojanSquirrel>())
        {
            BossTitleSystem.ShowBossTitle
                (
                    name: "TROJAN SQUIRREL",
                    subtitle: "Wooden Mechanism",
                    mainColor: Color.BurlyWood
                );
        }
        if (npc.type == ModContent.NPCType<CursedCoffin>())
        {
            BossTitleSystem.ShowBossTitle
                (
                    name: "CURSED COFFIN",
                    subtitle: "The Curse of the Pyramid",
                    mainColor: Color.Gold
                );
        }
        if (npc.type == ModContent.NPCType<DeviBoss>())
        {
            BossTitleSystem.ShowBossTitle
                (
                    name: "DEVIANTT",
                    subtitle: "Thrill-Seeker",
                    mainColor: Color.HotPink
                );
        }
        //Hardmode
        if (npc.type == ModContent.NPCType<BanishedBaron>())
        {
            BossTitleSystem.ShowBossTitle
                (
                    name: "BANISED BARON",
                    subtitle: "Degenerate",
                    mainColor: Color.CornflowerBlue
                );
        }
        if (npc.type == ModContent.NPCType<LifeChallenger>())
        {
            BossTitleSystem.ShowBossTitle
                (
                    name: "LIFELIGHT",
                    subtitle: "The Sole Ray",
                    mainColor: Color.Gold
                );
        }
        //Post-Moonlord
        if (npc.type == ModContent.NPCType<TimberChampion>())
        {
            BossTitleSystem.ShowBossTitle
                (
                    name: "TIMBER",
                    subtitle: "Champion",
                    mainColor: Color.LightYellow
                );
        }
        if (npc.type == ModContent.NPCType<TerraChampion>())
        {
            BossTitleSystem.ShowBossTitle
                (
                    name: "TERRA",
                    subtitle: "Champion",
                    mainColor: Color.DarkGray
                );
        }
        if (npc.type == ModContent.NPCType<EarthChampion>())
        {
            BossTitleSystem.ShowBossTitle
                (
                    name: "EARTH",
                    subtitle: "Champion",
                    mainColor: Color.DarkOrange
                );
        }
        if (npc.type == ModContent.NPCType<NatureChampion>())
        {
            BossTitleSystem.ShowBossTitle
                (
                    name: "NATURE",
                    subtitle: "Champion",
                    mainColor: Color.DarkCyan
                );
        }
        if (npc.type == ModContent.NPCType<LifeChampion>())
        {
            BossTitleSystem.ShowBossTitle
                (
                    name: "LIFE",
                    subtitle: "Champion",
                    mainColor: Color.Gold
                );
        }
        if (npc.type == ModContent.NPCType<ShadowChampion>())
        {
            BossTitleSystem.ShowBossTitle
                (
                    name: "SHADOW",
                    subtitle: "Champion",
                    mainColor: Color.Purple
                );
        }
        if (npc.type == ModContent.NPCType<SpiritChampion>())
        {
            BossTitleSystem.ShowBossTitle
                (
                    name: "SPIRIT",
                    subtitle: "Champion",
                    mainColor: Color.MediumPurple
                );
        }
        if (npc.type == ModContent.NPCType<WillChampion>())
        {
            BossTitleSystem.ShowBossTitle
                (
                    name: "WILL",
                    subtitle: "Champion",
                    mainColor: Color.LightGoldenrodYellow
                );
        }
        if (npc.type == ModContent.NPCType<CosmosChampion>())
        {
            BossTitleSystem.ShowBossTitle
                (
                    name: "GUARDIAN OF THE FOREST",
                    subtitle: "Champion",
                    mainColor: Color.Purple
                );
        }
        if (npc.type == ModContent.NPCType<AbomBoss>())
        {
            BossTitleSystem.ShowBossTitle
                (
                    name: "ABOMINATЕION",
                    subtitle: "The Slush of the World",
                    mainColor: Color.Orange
                );
        }
        if (npc.type == ModContent.NPCType<MutantBoss>())
        {
            BossTitleSystem.ShowBossTitle
                (
                    name: "MUTANT",
                    subtitle: "Progenitor of Eternity",
                    mainColor: Color.Cyan
                );
        }
        #endregion

        if (!string.IsNullOrEmpty(name))
            BossTitleSystem.ShowBossTitle(name, subtitle, mainColor);
    }
}