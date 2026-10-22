namespace ForgottenSouls.Content.Globals;

internal class GraniteGolemRoll : GlobalNPC
{
    private const float MinRange = 120f;
    private const float MaxRange = 600f;
    private const int AttemptChance = 90;
    private const int CooldownMin = 300;
    private const int CooldownMax = 480;

    private const int CurlTicks = 30;
    private const int RollMinTicks = 4 * 60;
    private const int RollMaxTicks = 8 * 60;
    private const float RollSpeed = 9f;
    private const float BounceHop = -4f;
    private const int BounceCooldownTicks = 15;
    private const int UncurlTicks = 45;

    private const float ContactDamageMultiplier = 1.6f;
    private const int RamDamage = 40;
    private const float RamKnockback = 8f;
    private const int RamHitCooldownTicks = 20;

    private static readonly Color Glow = new(70, 140, 255, 0);

    private enum State
    {
        Walk,
        Curl,
        Roll,
        Uncurl,
    }

    private State _state;
    private int _timer;
    private int _cooldown = 120;
    private int _direction;
    private int _bounceCooldown;
    private readonly Dictionary<int, int> _ramCooldowns = [];

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => npc.type == NPCID.GraniteGolem;

    public override void PostAI(NPC npc)
    {
        if (!Main.dedServ)
            Lighting.AddLight(npc.Center, 0.12f, 0.28f, 0.65f);

        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        npc.damage = _state == State.Roll ? (int)(npc.defDamage * ContactDamageMultiplier) : npc.defDamage;

        switch (_state)
        {
            case State.Walk:
                UpdateWalk(npc);
                break;

            case State.Curl:
                UpdateCurl(npc);
                break;

            case State.Roll:
                UpdateRoll(npc);
                break;

            case State.Uncurl:
                UpdateUncurl(npc);
                break;
        }
    }

    private void UpdateWalk(NPC npc)
    {
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

        _state = State.Curl;
        _timer = CurlTicks;
        _direction = target.Center.X > npc.Center.X ? 1 : -1;
    }

    private void UpdateCurl(NPC npc)
    {
        npc.velocity.X = 0f;
        npc.rotation += _direction * 0.15f;

        if (!Main.dedServ && Main.rand.NextBool(2))
            Dust.NewDust(npc.position, npc.width, npc.height, DustID.Stone, 0f, -1.5f);

        if (--_timer > 0)
            return;

        _state = State.Roll;
        _timer = Main.rand.Next(RollMinTicks, RollMaxTicks + 1);
        _bounceCooldown = 0;
    }

    private void UpdateRoll(NPC npc)
    {
        npc.velocity.X = _direction * RollSpeed;
        npc.direction = npc.spriteDirection = _direction;
        npc.rotation += _direction * RollSpeed * 0.06f;

        if (_bounceCooldown > 0)
            _bounceCooldown--;

        if (npc.collideX && _bounceCooldown <= 0)
        {
            _direction = -_direction;
            _bounceCooldown = BounceCooldownTicks;
            npc.velocity.Y = BounceHop;

            SoundEngine.PlaySound(SoundID.NPCHit3, npc.Center);

            if (!Main.dedServ)
            {
                for (int i = 0; i < 10; i++)
                    Dust.NewDust(npc.position, npc.width, npc.height, DustID.Stone, -_direction * 2f, Main.rand.NextFloat(-3f, -0.5f));
            }

            npc.netUpdate = true;
        }

        RamNearbyNpcs(npc);

        if (--_timer > 0)
            return;

        _state = State.Uncurl;
        _timer = UncurlTicks;
    }

    private void UpdateUncurl(NPC npc)
    {
        npc.velocity.X = 0f;
        npc.rotation = MathHelper.Lerp(npc.rotation, 0f, 0.2f);

        if (--_timer > 0)
            return;

        npc.rotation = 0f;
        _state = State.Walk;
        _cooldown = Main.rand.Next(CooldownMin, CooldownMax + 1);
    }

    private void RamNearbyNpcs(NPC npc)
    {
        foreach (int key in new List<int>(_ramCooldowns.Keys))
        {
            if (--_ramCooldowns[key] <= 0)
                _ramCooldowns.Remove(key);
        }

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC other = Main.npc[i];

            if (!other.active || other.whoAmI == npc.whoAmI || other.type == npc.type)
                continue;

            if (other.boss || other.dontTakeDamage || other.immortal)
                continue;

            if (_ramCooldowns.ContainsKey(i) || !npc.Hitbox.Intersects(other.Hitbox))
                continue;

            _ramCooldowns[i] = RamHitCooldownTicks;

            other.SimpleStrikeNPC(RamDamage, _direction, false, RamKnockback);
        }
    }

    public override void FindFrame(NPC npc, int frameHeight)
    {
        if (_state != State.Walk)
            npc.frame.Y = 0;
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.IsABestiaryIconDummy)
            return true;

        Main.instance.LoadNPC(npc.type);
        Texture2D texture = TextureAssets.Npc[npc.type].Value;
        Rectangle frame = npc.frame;

        Vector2 origin = frame.Size() / 2f;
        Vector2 position = npc.Bottom - screenPos + new Vector2(0f, npc.gfxOffY - frame.Height / 2f + 2f);
        SpriteEffects effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        float pulse = 0.65f + 0.35f * (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 4f);
        float strength = _state == State.Roll ? 1f : 0.75f;
        Color outline = Glow * (pulse * strength);

        for (int i = 0; i < 8; i++)
        {
            Vector2 offset = (i * MathHelper.PiOver4).ToRotationVector2() * 2f;

            Main.EntitySpriteDraw(texture, position + offset, frame, outline, npc.rotation, origin, npc.scale, effects, 0);
        }

        return true;
    }
}