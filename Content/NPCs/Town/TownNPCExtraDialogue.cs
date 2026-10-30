namespace ForgottenSouls.Content.Globals;

internal sealed class TownNPCExtraDialogue : GlobalNPC
{
    private const float CustomChance = 0.16f;  
    private const float ModeChance = CustomChance * 2f;  

    private static readonly Dictionary<int, string[]> Lines = new()
    {
        [NPCID.Guide] =
        [
            "Направляющая линия-заполнитель 1.",
            "Направляющая линия-заполнитель 2."
        ],
        [NPCID.Merchant] =
        [
            "Строка 1 для данных продавца (заполнитель).",
            "Строка 2 для данных продавца (заполнитель)."
        ],
        [NPCID.Nurse] =
        [
            "Линия для связи с медсестрой (заполнитель 1)."
        ]
    };

    private static readonly Dictionary<int, string[]> SadomasochistLines = new()
    {
        [NPCID.Guide] =
        [
            "Вспомогательная направляющая для садомазохиста."
        ],
        [NPCID.Nurse] =
        [
            "Временная реплика медсестры для персонажа-садомазохиста."
        ]
    };

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => lateInstantiation && entity.townNPC;

    public override void GetChat(NPC npc, ref string chat)
    {
        if (Main.rand.NextFloat() >= CustomChance)
            return;

        if (Sadomasochist.Active
            && Main.rand.NextFloat() < ModeChance
            && SadomasochistLines.TryGetValue(npc.type, out string[] modeLines)
            && modeLines.Length > 0)
        {
            chat = modeLines[Main.rand.Next(modeLines.Length)];
            return;
        }

        if (Lines.TryGetValue(npc.type, out string[] lines) && lines.Length > 0)
            chat = lines[Main.rand.Next(lines.Length)];
    }
}