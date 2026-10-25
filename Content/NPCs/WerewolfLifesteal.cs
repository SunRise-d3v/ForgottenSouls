namespace ForgottenSouls.Content.Globals;

internal class WerewolfLifesteal : GlobalNPC
{
    private const float HealFraction = 0.10f;

    public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => npc.type == NPCID.Werewolf;

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
    {
        int heal = Math.Max(1, (int)(hurtInfo.Damage * HealFraction));

        heal = Math.Min(heal, npc.lifeMax - npc.life);

        if (heal <= 0)
            return;

        npc.life += heal;
        npc.HealEffect(heal);
    }
}