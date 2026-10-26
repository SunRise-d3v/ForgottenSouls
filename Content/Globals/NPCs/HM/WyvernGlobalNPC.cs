namespace ForgottenSouls.Content.NPCs;

internal sealed class WyvernGlobalNPC : GlobalNPC
{
    public override void EditSpawnPool(
        IDictionary<int, float> pool,
        NPCSpawnInfo spawnInfo)
    {
        if (!Main.hardMode && spawnInfo.Player.ZoneSkyHeight)
            pool[NPCID.WyvernHead] = 0.1f;
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (!Main.hardMode && npc.type == NPCID.WyvernHead)
            npcLoot.RemoveWhere(rule => true);
    }
}