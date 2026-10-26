namespace ForgottenSouls.Content.Globals;

internal sealed class LavaImmunity : GlobalNPC
{
    public override void SetDefaults(NPC npc)
    {
        if (!npc.friendly && !npc.townNPC)
            npc.lavaImmune = true;
    }
}