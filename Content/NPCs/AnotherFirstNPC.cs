namespace ForgottenSouls.Content;

internal class AnotherFirstNPC : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        int index = tasks.FindIndex(t => t.Name == "Guide");
        if (index == -1)
            return;

        tasks[index] = new PassLegacy("Guide", SpawnNPC);
    }

    private static void SpawnNPC(GenerationProgress progress, GameConfiguration config)
    {
        int i = NPC.NewNPC(
            new EntitySource_WorldGen(),
            Main.spawnTileX * 16,
            Main.spawnTileY * 16,
            NPCID.Demolitionist);

        if (i < 0 || i >= Main.maxNPCs)
            return;

        NPC npc = Main.npc[i];
        npc.homeTileX = Main.spawnTileX;
        npc.homeTileY = Main.spawnTileY;
        npc.direction = 1;
        npc.homeless = true;
    }
}