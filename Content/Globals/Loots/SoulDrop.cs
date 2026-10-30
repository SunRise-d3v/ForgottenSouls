using DropConditions = Terraria.GameContent.ItemDropRules.Conditions;

namespace ForgottenSouls.Content.Globals;

internal sealed class SoulDrop : GlobalNPC
{
    private static readonly (int Min, int Max) PreHardmodeMonster = (1, 10);
    private static readonly (int Min, int Max) HardmodeMonster = (10, 75);
    private static readonly (int Min, int Max) EvilMonster = (6, 15);
    private static readonly (int Min, int Max) BloodMoonMonster = (10, 25);
    private static readonly (int Min, int Max) PreHardmodeBoss = (50, 100);
    private static readonly (int Min, int Max) HardmodeBoss = (200, 1000);

    private static readonly SoulCondition InHardmode = new(() => Main.hardMode, "In Hardmode");
    private static readonly SoulCondition InPreHardmode = new(() => !Main.hardMode, "Before Hardmode");

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type == NPCID.None || npc.friendly || npc.townNPC || NPCID.Sets.CountsAsCritter[npc.type])
            return;

        int soul = ModContent.ItemType<Soul>();

        if (npc.boss)
        {
            if (Pool.PreHardmodeBosses.Contains(npc.type))
                AddBossDrop(npcLoot, soul, PreHardmodeBoss);
            else if (Pool.HardmodeBosses.Contains(npc.type))
                AddBossDrop(npcLoot, soul, HardmodeBoss);
            else
            {
                npcLoot.Add(ItemDropRule.ByCondition(InPreHardmode, soul, 1, PreHardmodeBoss.Min, PreHardmodeBoss.Max));
                npcLoot.Add(ItemDropRule.ByCondition(InHardmode, soul, 1, HardmodeBoss.Min, HardmodeBoss.Max));
            }

            return;
        }

        if (npc.damage <= 0)
            return;

        string name = GetName(npc.type);

        if (name != null && Pool.BloodMoonNames.Contains(name))
        {
            npcLoot.Add(ItemDropRule.Common(soul, 1, BloodMoonMonster.Min, BloodMoonMonster.Max));
            return;
        }

        if (Pool.EvilNPCs.Contains(npc.type) || IsEvilByName(name))
        {
            npcLoot.Add(ItemDropRule.Common(soul, 1, EvilMonster.Min, EvilMonster.Max));
            return;
        }

        npcLoot.Add(ItemDropRule.ByCondition(InPreHardmode, soul, 1, PreHardmodeMonster.Min, PreHardmodeMonster.Max));
        npcLoot.Add(ItemDropRule.ByCondition(InHardmode, soul, 1, HardmodeMonster.Min, HardmodeMonster.Max));
    }

    private static void AddBossDrop(NPCLoot npcLoot, int soul, (int Min, int Max) amount)
    => npcLoot.Add(ItemDropRule.ByCondition(new DropConditions.LegacyHack_IsABoss(), soul, 1, amount.Min, amount.Max));

    private static string GetName(int type)
        => type < NPCID.Count && NPCID.Search.TryGetName(type, out string name) ? name : null;

    private static bool IsEvilByName(string name)
    {
        if (name == null)
            return false;

        foreach (string part in Pool.EvilNameParts)
        {
            if (name.Contains(part, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}

internal sealed class SoulCondition(Func<bool> predicate, string description) : IItemDropRuleCondition
{
    public bool CanDrop(DropAttemptInfo info) => predicate();

    public bool CanShowItemDropInUI() => true;

    public string GetConditionDescription() => description;
}