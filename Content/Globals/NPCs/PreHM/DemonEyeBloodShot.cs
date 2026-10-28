namespace ForgottenSouls.Content.NPCs;

internal sealed class DemonEyeBloodShot : GlobalNPC
{
 
    private const float MinRange = 200f;             
    private const float MaxRange = 650f;
    private const int AttemptChance = 45;     
    private const int AimTicks = 30;          
    private const int CooldownMin = 290;      
    private const int CooldownMax = 500;
    private const float ShotSpeed = 15f;
    private const int ShotDamage = 45;
    private const float DashSpeedThreshold = 6f;

    private const float ChargeRadius = 70f;      
    private const float AimLineLength = 220f;    

    private bool _aiming;
    private int _timer;
    private int _cooldown = 120;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => Pool.Eyes.Contains(npc.type);

    public override void PostAI(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (_aiming)
        {
            UpdateAim(npc);
            return;
        }

        if (_cooldown > 0)
        {
            _cooldown--;
            return;
        }

        if (!npc.HasValidTarget)
            return;

        Player target = Main.player[npc.target];
        float distance = npc.Distance(target.Center);

        if (distance < MinRange || distance > MaxRange)
            return;

        if (npc.velocity.Length() > DashSpeedThreshold)
            return;

        if (!Collision.CanHitLine(npc.position, npc.width, npc.height, target.position, target.width, target.height))
            return;

        if (!Main.rand.NextBool(AttemptChance))
            return;

        _aiming = true;
        _timer = AimTicks;
    }

    private void UpdateAim(NPC npc)
    {
        if (!npc.HasValidTarget)
        {
            _aiming = false;
            return;
        }

        npc.velocity *= 0.92f;

        float progress = 1f - _timer / (float)AimTicks;
        Player target = Main.player[npc.target];

        SpawnChargeEffect(npc, target, progress);

        if (--_timer > 0)
            return;

        Fire(npc, target);

        _aiming = false;
        _cooldown = Main.rand.Next(CooldownMin, CooldownMax + 1);
    }

    private static void SpawnChargeEffect(NPC npc, Player target, float progress)
    {
        if (Main.dedServ)
            return;

        Lighting.AddLight(npc.Center, 0.2f + 0.8f * progress, 0.03f, 0.03f);

        int count = 1 + (int)(progress * 4f);

        for (int i = 0; i < count; i++)
        {
            Vector2 direction = Main.rand.NextVector2CircularEdge(1f, 1f);
            Vector2 position = npc.Center + direction * ChargeRadius;

            Dust dust = Dust.NewDustPerfect(position, DustID.Blood, -direction * (ChargeRadius / 10f), 120, default, 1.1f + progress * 0.6f);
            dust.noGravity = true;
        }

        Vector2 aim = (target.Center - npc.Center).SafeNormalize(Vector2.UnitY);

        if (Main.rand.NextBool(2))
        {
            Vector2 position = npc.Center + aim * Main.rand.NextFloat(30f, AimLineLength);
            Dust dust = Dust.NewDustPerfect(position, DustID.Blood, Vector2.Zero, 150, default, 0.8f);
            dust.noGravity = true;
        }
    }

    private static void Fire(NPC npc, Player target)
    {
        Vector2 velocity = (target.Center - npc.Center).SafeNormalize(Vector2.UnitY) * ShotSpeed;

        int index = Projectile.NewProjectile(
            npc.GetSource_FromAI(),
            npc.Center,
            velocity,
            ProjectileID.BloodShot,
            ShotDamage,
            1f,
            Main.myPlayer);

        if (index >= 0 && index < Main.maxProjectiles)
        {
            Projectile shot = Main.projectile[index];
            shot.GetGlobalProjectile<StraightEyeShot>().Lock(velocity);
            shot.netUpdate = true;
        }

        if (!Main.dedServ)
        {
            for (int i = 0; i < 16; i++)
            {
                Vector2 direction = (i / 16f * MathHelper.TwoPi).ToRotationVector2();
                Dust dust = Dust.NewDustPerfect(npc.Center, DustID.Blood, direction * 3f, 100, default, 1.4f);
                dust.noGravity = true;
            }
        }
    }
}

internal sealed class StraightEyeShot : GlobalProjectile
{
    private bool _locked;
    private Vector2 _velocity;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.type == ProjectileID.BloodShot;

    internal void Lock(Vector2 velocity)
    {
        _locked = true;
        _velocity = velocity;
    }

    public override void PostAI(Projectile projectile)
    {
        if (_locked)
            projectile.velocity = _velocity;
    }
}