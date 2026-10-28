namespace ForgottenSouls.Content;

internal class TrojanSquirrelHook : GlobalNPC
{
#if false
    private const int IntervalTicks = 3;        
    private const float SpawnChance = 0.6f;  
    private const float MinBehind = 60f;     
    private const float MaxBehind = 120f;

    private int _timer;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => npc.type == ModContent.NPCType<TrojanSquirrel>();

    public override void PostAI(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        bool dashing = (int)npc.ai[0] == 0 && npc.localAI[0] > 0;

        bool firstStage = npc.life >= npc.lifeMax / 2;

        if (!dashing || !firstStage)
        {
            _timer = 0;
            return;
        }

        if (++_timer < IntervalTicks)
            return;

        _timer = 0;

        if (Main.rand.NextFloat() < SpawnChance)
            SpawnExplosion(npc);
    }

    private static void SpawnExplosion(NPC npc)
    {
        int direction = npc.velocity.X != 0f
            ? System.Math.Sign(npc.velocity.X)
            : npc.direction;

        Vector2 position = new(
            npc.Bottom.X - direction * Main.rand.NextFloat(MinBehind, MaxBehind),
            npc.Bottom.Y
        );

        Projectile explosion = Projectile.NewProjectileDirect(
            npc.GetSource_FromThis(),
            position,
            Vector2.Zero,
            ProjectileID.DD2ExplosiveTrapT3Explosion,
            FargoSoulsUtil.ScaledProjectileDamage(npc.defDamage),
            0f,
            Main.myPlayer
        );

        explosion.friendly = false;
        explosion.hostile = true;
        explosion.netUpdate = true;
    }
#endif
}