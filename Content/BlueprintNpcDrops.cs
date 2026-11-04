namespace ForgottenSouls.Content.Globals;

internal sealed class NormalModeBossCondition : IItemDropRuleCondition
{
    public bool CanDrop(DropAttemptInfo info)
    {
        return !Main.expertMode
            && info.npc != null
            && new Terraria.GameContent.ItemDropRules.Conditions.LegacyHack_IsABoss().CanDrop(info);
    }

    public bool CanShowItemDropInUI()
    {
        return true;
    }

    public string GetConditionDescription()
    {
        return "Normal mode";
    }
}

internal sealed class BlueprintNpcDrops : GlobalNPC
{
    private const float MobChance = 0.01f;

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type == NPCID.None
            || npc.friendly
            || npc.townNPC
            || NPCID.Sets.CountsAsCritter[npc.type])
        {
            return;
        }

        if (npc.boss)
        {
            npcLoot.Add(
                new BlueprintDropRule(
                    1f,
                    1,
                    2,
                    new NormalModeBossCondition()
                )
            );

            return;
        }

        if (npc.damage <= 0)
            return;

        npcLoot.Add(
            new BlueprintDropRule(
                MobChance,
                1,
                1
            )
        );
    }
}