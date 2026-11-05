namespace DisasterMod.Content.NPCs;

internal sealed class NPCProperties : GlobalNPC
{
    private const float BOSS_MULT = 1.2f;
    private const float MOB_MULT = 1.1f;

    public override void SetDefaults(NPC npc)
    {
        if (npc.type == NPCID.None || npc.friendly || npc.townNPC || NPCID.Sets.CountsAsCritter[npc.type])
            return;

        if (npc.lifeMax < 10)
            return;

        if (npc.boss)
        {
            Scale(npc, BOSS_MULT);
            npc.knockBackResist = 0f;
            npc.lavaImmune = true;
        }
        else if (npc.damage > 0)
        {
            Scale(npc, MOB_MULT);
            npc.knockBackResist /= MOB_MULT;
        }
    }

    private static void Scale(NPC npc, float mult)
    {
        npc.lifeMax = (int)(npc.lifeMax * mult);
        npc.damage = (int)(npc.damage * mult);
        npc.defense = (int)(npc.defense * mult);
        npc.value *= mult;
    }
}