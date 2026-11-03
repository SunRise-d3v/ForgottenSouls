namespace NoCompromises.Content.Entities.PreHM;

internal class KingSlimeSize : GlobalNPC
{
    private const float SizeMultiplier = 1.3f;

    private float _appliedScale;
    private int _appliedWidth;
    private int _appliedHeight;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => npc.type == NPCID.KingSlime;

    public override void PostAI(NPC npc)
    {
        bool sizeAlreadyOurs = npc.scale == _appliedScale && npc.width == _appliedWidth && npc.height == _appliedHeight;

        if (sizeAlreadyOurs)
            return;

        Vector2 bottom = npc.Bottom;

        npc.scale *= SizeMultiplier;
        npc.width = (int)(npc.width * SizeMultiplier);
        npc.height = (int)(npc.height * SizeMultiplier);
        npc.Bottom = bottom;

        _appliedScale = npc.scale;
        _appliedWidth = npc.width;
        _appliedHeight = npc.height;
    }
}