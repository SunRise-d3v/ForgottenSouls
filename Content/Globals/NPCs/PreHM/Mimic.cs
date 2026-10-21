namespace ForgottenSouls.Content.NPCs;

internal sealed class Mimic : GlobalNPC
{
    /*public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        if (!Main.hardMode && !spawnInfo.Player.ZoneDungeon && !spawnInfo.PlayerSafe)
            pool[NPCID.Mimic] = 0.01f;
    }*/

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type == NPCID.Mimic && !Main.hardMode)
            npcLoot.RemoveWhere(rule => true);
    }
}