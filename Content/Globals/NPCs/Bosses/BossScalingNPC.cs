namespace ForgottenSouls.Content.Globals;

internal class BossScalingNPC : GlobalNPC
{
    private const int MaxScalingKills = 100;

    private const float LifePerKill = 0.04f;      // +4%
    private const float DamagePerKill = 0.07f;    // +7% 
    private const int DefensePerKill = 2;         // +2 
    private const float KnockbackPerKill = 0.06f; // +6%
    private const float SpeedPerKill = 0.015f;     // +1.5% 
    private const float ValuePerKill = 0.11f;     // +11% 

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => lateInstantiation && entity.boss;

    private bool scaled;
    private int scaleKills;

    private static void ApplyScaling(NPC npc, int kills, bool scaleLife)
    {
        if (scaleLife)
        {
            float lifeMult = 1f + kills * LifePerKill;
            npc.lifeMax = (int)(npc.lifeMax * lifeMult);
            npc.life = (int)(npc.life * lifeMult);
        }

        npc.damage = (int)(npc.damage * (1f + kills * DamagePerKill));
        npc.defDamage = npc.damage;
        npc.defense += kills * DefensePerKill;
        npc.defDefense = npc.defense;
        npc.knockBackResist *= Math.Max(0f, 1f - kills * KnockbackPerKill);
        npc.value *= 1f + kills * ValuePerKill;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.Write((byte)scaleKills);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        int kills = binaryReader.ReadByte();
        if (!scaled && Main.netMode == NetmodeID.MultiplayerClient)
        {
            scaled = true;
            scaleKills = kills;

            if (kills > 0)
                ApplyScaling(npc, kills, false);
        }
    }

    public override bool PreAI(NPC npc)
    {
        if (!scaled && Main.netMode != NetmodeID.MultiplayerClient)
        {
            scaled = true;
            scaleKills = Math.Min(MaxScalingKills, BossMemorySystem.MaxKills(npc));
            if (scaleKills > 0)
            {
                ApplyScaling(npc, scaleKills, true);
                npc.netUpdate = true;
            }
        }

        return true;
    }

    public override void PostAI(NPC npc)
    {
        if (scaleKills > 0 && Main.netMode != NetmodeID.MultiplayerClient && (npc.noTileCollide || npc.noGravity))
            npc.position += npc.velocity * (scaleKills * SpeedPerKill);
    }
}