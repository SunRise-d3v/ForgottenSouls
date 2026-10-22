namespace ForgottenSouls.Content.Globals;

internal class DoctorBonesShotgun : GlobalNPC
{
    private const int GunItem = ItemID.Boomstick;

    private const float MinRange = 150f;
    private const float MaxRange = 520f;
    private const int AttemptChance = 60;
    private const int AimTicks = 25;
    private const int CooldownMin = 240;
    private const int CooldownMax = 420;

    private const int Pellets = 5;
    private const float PelletSpread = 0.22f;
    private const float PelletSpeed = 12f;
    private const int PelletDamage = 14;
    private const float Recoil = 2.5f;

    private enum State
    {
        Walk,
        Aim,
    }

    private State _state;
    private int _timer;
    private int _cooldown = 90;
    private float _aim;
    private bool _aiming;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => npc.type == NPCID.DoctorBones;

    public override void PostAI(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (_state == State.Walk)
            UpdateWalk(npc);
        else
            UpdateAim(npc);
    }

    private void UpdateWalk(NPC npc)
    {
        _aiming = false;

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

        _state = State.Aim;
        _timer = AimTicks;
    }

    private void UpdateAim(NPC npc)
    {
        npc.velocity.X = 0f;
        SoundEngine.PlaySound(SoundID.Item149, npc.Center);

        if (!npc.HasValidTarget)
        {
            _state = State.Walk;
            _aiming = false;
            return;
        }

        Player target = Main.player[npc.target];

        _aiming = true;
        _aim = (target.Center - GunPivot(npc)).ToRotation();
        npc.direction = npc.spriteDirection = target.Center.X > npc.Center.X ? 1 : -1;

        if (--_timer > 0)
            return;

        Fire(npc);

        _state = State.Walk;
        _aiming = false;
        _cooldown = Main.rand.Next(CooldownMin, CooldownMax + 1);
    }

    private void Fire(NPC npc)
    {
        Vector2 muzzle = GunPivot(npc) + _aim.ToRotationVector2() * 26f;

        SoundEngine.PlaySound(SoundID.Item36, npc.Center);

        for (int i = 0; i < Pellets; i++)
        {
            float angle = _aim + Main.rand.NextFloat(-PelletSpread, PelletSpread);
            Vector2 velocity = angle.ToRotationVector2() * PelletSpeed * Main.rand.NextFloat(0.9f, 1.1f);

            int index = Projectile.NewProjectile(
                npc.GetSource_FromAI(),
                muzzle,
                velocity,
                ProjectileID.Bullet,
                PelletDamage,
                1f,
                Main.myPlayer);

            if (index >= 0 && index < Main.maxProjectiles)
            {
                Projectile pellet = Main.projectile[index];
                pellet.friendly = false;
                pellet.hostile = true;
                pellet.netUpdate = true;
            }
        }

        if (!Main.dedServ)
        {
            for (int i = 0; i < 8; i++)
                Dust.NewDustPerfect(muzzle, DustID.Smoke, _aim.ToRotationVector2() * Main.rand.NextFloat(1f, 4f), 100, default, 1.3f);
        }

        npc.velocity.X -= (float)Math.Cos(_aim) * Recoil;
        npc.netUpdate = true;
    }

    private static Vector2 GunPivot(NPC npc) => npc.Center + new Vector2(npc.direction * 6f, 3.8f);

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.IsABestiaryIconDummy)
            return;

        Main.instance.LoadItem(GunItem);
        Texture2D texture = TextureAssets.Item[GunItem].Value;

        // В покое ствол смотрит вперёд и чуть вниз, при прицеливании на игрока
        float rotation = _aiming
            ? _aim
            : (npc.direction == 1 ? 0.25f : MathHelper.Pi - 0.25f);

        bool facingLeft = Math.Cos(rotation) < 0;

        SpriteEffects effects = facingLeft ? SpriteEffects.FlipVertically : SpriteEffects.None;
        Vector2 origin = new(texture.Width * 0.25f, texture.Height / 2f);

        spriteBatch.Draw(
            texture,
            GunPivot(npc) - screenPos,
            null,
            drawColor,
            rotation,
            origin,
            0.865f,
            effects,
            0f);
    }
}