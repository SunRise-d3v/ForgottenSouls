using Conditions = Terraria.GameContent.ItemDropRules.Conditions;

namespace ForgottenSouls.Content.Globals;

public class HardmodeProgression : ModSystem
{
    private static int oreStage;
    private static int[] oreTypes = new int[3];

    public override void Load()
    {
        On_WorldGen.SmashAltar += SmashAltarDetour;
    }

    public override void Unload()
    {
        On_WorldGen.SmashAltar -= SmashAltarDetour;
    }

    public override void ClearWorld()
    {
        oreStage = 0;
        oreTypes = new int[3];
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["oreStage"] = oreStage;
        tag["oreTypes"] = new List<int>(oreTypes);
    }

    public override void LoadWorldData(TagCompound tag)
    {
        oreStage = tag.GetInt("oreStage");
        var list = tag.GetList<int>("oreTypes");
        for (int i = 0; i < 3 && i < list.Count; i++)
            oreTypes[i] = list[i];
    }

    private static void SmashAltarDetour(On_WorldGen.orig_SmashAltar orig, int i, int j)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !Main.hardMode || WorldGen.noTileActions || WorldGen.gen)
            return;

        WorldGen.altarCount++;
        AchievementsHelper.NotifyProgressionEvent(6);

        var source = new EntitySource_TileBreak(i, j);

        Item.NewItem(source, i * 16, j * 16, 32, 32, ItemID.SoulofNight, Main.rand.Next(3, 6)); // 3..5

        for (int k = 0; k < 2; k++)
            NPC.NewNPC(source, i * 16 + 16 + Main.rand.Next(-24, 25), j * 16 - 16, NPCID.Wraith);
    }

    public override void PostUpdateWorld()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        int target = 0;
        if (Main.hardMode)
        {
            int mechs = (NPC.downedMechBoss1 ? 1 : 0) + (NPC.downedMechBoss2 ? 1 : 0) + (NPC.downedMechBoss3 ? 1 : 0);
            target = 1 + Math.Min(mechs, 2);
        }

        while (oreStage < target)
        {
            SpawnOreTier(oreStage);
            oreStage++;
        }
    }

    private static void SpawnOreTier(int tier)
    {
        if (oreTypes[tier] == 0)
        {
            bool alt = WorldGen.genRand.NextBool();
            switch (tier)
            {
                case 0: oreTypes[tier] = alt ? TileID.Palladium : TileID.Cobalt; break;
                case 1: oreTypes[tier] = alt ? TileID.Orichalcum : TileID.Mythril; break;
                default: oreTypes[tier] = alt ? TileID.Titanium : TileID.Adamantite; break;
            }
        }

        int type = oreTypes[tier];
        if (tier == 0) WorldGen.SavedOreTiers.Cobalt = type;
        else if (tier == 1) WorldGen.SavedOreTiers.Mythril = type;
        else WorldGen.SavedOreTiers.Adamantite = type;

        double count = Main.maxTilesX / 4200.0 * (310 - 85 * tier) * 0.85;
        double minDepth = Main.worldSurface;
        if (tier == 1) minDepth = Main.rockLayer;
        if (tier == 2) minDepth = (Main.rockLayer + Main.rockLayer + Main.maxTilesY) / 3.0;

        for (int k = 0; k < count; k++)
        {
            int x = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
            int y = WorldGen.genRand.Next((int)minDepth, Main.maxTilesY - 150);
            WorldGen.OreRunner(x, y, WorldGen.genRand.Next(5, 10), WorldGen.genRand.Next(5, 10), (ushort)type);
        }

        string name;
        switch (type)
        {
            case TileID.Cobalt: name = "кобальтом"; break;
            case TileID.Palladium: name = "палладием"; break;
            case TileID.Mythril: name = "мифрилом"; break;
            case TileID.Orichalcum: name = "орихалком"; break;
            case TileID.Adamantite: name = "адамантитом"; break;
            default: name = "титаном"; break;
        }

        string text = "Ваш мир благословлён " + name + "!";
        Color color = new(50, 255, 130);
        if (Main.netMode == NetmodeID.Server)
            ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), color);
        else
            Main.NewText(text, color);
    }
}

public class AllMechsDefeatedCondition : IItemDropRuleCondition
{
    public bool CanDrop(DropAttemptInfo info)
    {
        if (info.npc != null)
        {
            if (Main.expertMode)
                return false;

            int t = info.npc.type;
            bool destroyer = NPC.downedMechBoss1 || t == NPCID.TheDestroyer;
            bool twins = NPC.downedMechBoss2 || t == NPCID.Retinazer || t == NPCID.Spazmatism;
            bool prime = NPC.downedMechBoss3 || t == NPCID.SkeletronPrime;
            return destroyer && twins && prime;
        }

        return NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3;
    }

    public bool CanShowItemDropInUI() => true;

    public string GetConditionDescription() => "Только после победы над всеми тремя механическими боссами";
}

public static class MechLootHelper
{
    public static bool IsHallowedBarRule(IItemDropRule rule)
        => rule is CommonDrop drop && drop.itemId == ItemID.HallowedBar;
}

public class MechBossLootNPC : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        int t = npc.type;
        bool twin = t == NPCID.Retinazer || t == NPCID.Spazmatism;
        if (!twin && t != NPCID.TheDestroyer && t != NPCID.SkeletronPrime)
            return;

        npcLoot.RemoveWhere(MechLootHelper.IsHallowedBarRule);

        var bars = ItemDropRule.ByCondition(new AllMechsDefeatedCondition(), ItemID.HallowedBar, 1, 15, 30);

        if (twin)
        {
            var last = new LeadingConditionRule(new Conditions.MissingTwin());
            last.OnSuccess(bars);
            npcLoot.Add(last);
        }
        else
        {
            npcLoot.Add(bars);
        }
    }
}

public class MechBossBagLootItem : GlobalItem
{
    public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
    {
        if (item.type != ItemID.DestroyerBossBag && item.type != ItemID.TwinsBossBag && item.type != ItemID.SkeletronPrimeBossBag)
            return;

        itemLoot.RemoveWhere(MechLootHelper.IsHallowedBarRule);
        itemLoot.Add(ItemDropRule.ByCondition(new AllMechsDefeatedCondition(), ItemID.HallowedBar, 1, 15, 30));
    }
}