namespace NoCompromises.Content.NPCs;

internal class GlobalNPCProperties : GlobalNPC
{
    public override void SetDefaults(NPC npc)
    {
        if (npc.friendly || npc.townNPC || NPCID.Sets.CountsAsCritter[npc.type] || npc.type == NPCID.None)
            return;

        if (!npc.friendly && npc.lifeMax >= 10)
        {
            if (npc.boss)
            {
                npc.lifeMax = (int)(npc.lifeMax * 1.4f);
                npc.damage = (int)(npc.damage * 1.33f);
                npc.defense = (int)(npc.defense * 1.22f);
                npc.knockBackResist = 0;
                npc.lavaImmune = true;
                npc.velocity *= (int)1.155f;
            }
            else if (npc.damage > 0)
            {
                npc.lifeMax = (int)(npc.lifeMax * 1.2f);
                npc.damage = (int)(npc.damage * 1.165f);
                npc.defense = (int)(npc.defense * 1.245f);
                npc.knockBackResist /= 1.1f;
                npc.value *= (int)1.2f;
                npc.velocity *= (int)1.2355f;
            }
        }
    }
}