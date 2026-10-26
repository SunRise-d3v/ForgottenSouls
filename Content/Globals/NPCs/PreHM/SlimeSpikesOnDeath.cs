namespace ForgottenSouls.Content.NPCs;

internal sealed class SlimeDeathSpikes : GlobalNPC
{
    private const int SpikeDamage = 20;
    private const float MinSpeed = 4f;
    private const float MaxSpeed = 8f;

    internal static readonly HashSet<int> LargeSlimes =
    [
        NPCID.UmbrellaSlime,
        NPCID.ToxicSludge,
        NPCID.SpikedJungleSlime,
        NPCID.SpikedIceSlime,
        NPCID.MotherSlime,
        NPCID.DungeonSlime,
        NPCID.CorruptSlime,
        NPCID.Crimslime,
        NPCID.IlluminantSlime,
        NPCID.RainbowSlime
    ];

    internal static bool IsHostileSlime(NPC npc)
        => npc.aiStyle == NPCAIStyleID.Slime && !npc.friendly && !npc.townNPC;

    public override void OnKill(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || !IsHostileSlime(npc))
            return;

        int count = LargeSlimes.Contains(npc.type)
            ? Main.rand.Next(5, 11)   // 5-10
            : Main.rand.Next(3, 6);  // 3-5

        float startAngle = Main.rand.NextFloat(MathHelper.TwoPi);

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.25f, 0.25f);
            Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(MinSpeed, MaxSpeed);

            Projectile.NewProjectile(
                npc.GetSource_Death(),
                npc.Center,
                velocity,
                ProjectileID.SpikedSlimeSpike,
                SpikeDamage,
                1f,
                Main.myPlayer);
        }
    }
}