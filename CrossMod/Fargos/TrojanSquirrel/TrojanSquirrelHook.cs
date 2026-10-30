namespace ForgottenSouls.Content;

internal class TrojanSquirrelHook : GlobalNPC
{
    private const float Spacing = 90f;   
    private const float Behind = 70f; 
    private const float LiftY = 50f;  
    private const int MaxPerTick = 6;  

    private float _lastX;
    private int _direction;
    private bool _active;

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
            _active = false;
            return;
        }

        int direction = npc.velocity.X != 0f ? System.Math.Sign(npc.velocity.X) : npc.direction;

        if (!_active || direction != _direction)
        {
            _active = true;
            _direction = direction;
            _lastX = npc.Center.X;
            return;
        }

        for (int i = 0; i < MaxPerTick && (npc.Center.X - _lastX) * direction >= Spacing; i++)
        {
            _lastX += direction * Spacing;
            SpawnExplosion(npc, _lastX - direction * Behind);
        }
    }

    private static void SpawnExplosion(NPC npc, float x)
    {
        Vector2 position = new(x, npc.Bottom.Y - LiftY);

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
}