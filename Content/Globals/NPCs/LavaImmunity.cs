namespace DisasterMod.Content.NPCs;

internal sealed class LavaImmunity : GlobalNPC
{
    public override void SetDefaults(NPC npc)
    {
        if (!npc.friendly && !npc.townNPC)
            npc.lavaImmune = true;
    }
}