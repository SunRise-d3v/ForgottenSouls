namespace ForgottenSouls.Content.NPCs;

internal sealed class SlimeSpikeShooter : GlobalNPC
{
    private const float MinRange = 150f;
    private const float MaxRange = 600f;
    private const int AttemptChance = 60;
    private const int ChargeTicks = 25;
    private const int CooldownMin = 8 * 60;
    private const int CooldownMax = 14 * 60;
    private const float ShotSpeed = 7f;
    private const int ShotDamage = 18;
    private const float Spread = 0.25f;

    private bool _charging;
    private int _chargeTimer;
    private int _cooldown = 180;

    public override bool InstancePerEntity => true;

    public override void PostAI(NPC npc)
    {
        if (!SlimeDeathSpikes.IsHostileSlime(npc) || Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (_charging)
        {
            UpdateCharge(npc);
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

        if (npc.velocity.Y != 0f || distance < MinRange || distance > MaxRange)
            return;

        if (!Collision.CanHitLine(npc.position, npc.width, npc.height, target.position, target.width, target.height))
            return;

        if (!Main.rand.NextBool(AttemptChance))
            return;

        _charging = true;
        _chargeTimer = ChargeTicks;
    }

    private void UpdateCharge(NPC npc)
    {
        if (!npc.HasValidTarget)
        {
            _charging = false;
            return;
        }

        if (!Main.dedServ && Main.rand.NextBool(2))
            Dust.NewDust(npc.position, npc.width, npc.height, DustID.t_Slime, 0f, -1f, 150);

        if (--_chargeTimer > 0)
            return;

        Fire(npc, Main.player[npc.target]);

        _charging = false;
        _cooldown = Main.rand.Next(CooldownMin, CooldownMax + 1);
    }

    private static void Fire(NPC npc, Player target)
    {
        Vector2 direction = (target.Center - npc.Center).SafeNormalize(Vector2.UnitY);
        int count = SlimeDeathSpikes.LargeSlimes.Contains(npc.type) ? 3 : 1;

        for (int i = 0; i < count; i++)
        {
            float offset = count == 1 ? 0f : (i - 1) * Spread;

            Projectile.NewProjectile(
                npc.GetSource_FromAI(),
                npc.Center,
                direction.RotatedBy(offset) * ShotSpeed,
                ProjectileID.SpikedSlimeSpike,
                ShotDamage,
                1f,
                Main.myPlayer);
        }
    }
}

internal sealed class StickySlimeSpike : GlobalProjectile
{
    private const int SlimedTicks = 5 * 60;

    private bool _fromSlime;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.type == ProjectileID.SpikedSlimeSpike;

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (source is EntitySource_Parent { Entity: NPC npc } && npc.aiStyle == NPCAIStyleID.Slime)
            _fromSlime = true;
    }

    public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
    {
        if (_fromSlime)
            target.AddBuff(BuffID.Slimed, SlimedTicks);
    }
}