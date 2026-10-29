namespace ForgottenSouls.Content.NPCs;

internal sealed class GraniteElementalProjectile : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SapphireBolt;

    public override void SetDefaults()
    {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 240;
        Projectile.tileCollide = true;
        Projectile.aiStyle = -1;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        Lighting.AddLight(Projectile.Center, 0.1f, 0.35f, 0.9f);

        Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GemSapphire, -Projectile.velocity * 0.1f, 100, default, 1.1f);
        d.noGravity = true;
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 12; i++)
        {
            Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.GemSapphire, Main.rand.NextVector2Circular(3f, 3f), 100, default, 1.2f);
            d.noGravity = true;
        }
        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
    }
}

internal sealed class GraniteElemental : GlobalNPC
{
    private enum State : byte { Normal, Telegraph, Curling, Falling, Uncurling }

    private const int AttackInterval = 220;
    private const int TelegraphTicks = 45;
    private const float AttackRange = 650f;
    private const float BoltSpeed = 10f;

    private const int CurlCooldown = 480;
    private const int GroundHoldTicks = 60;
    private const int MaxFallTicks = 360;

    private readonly string TexturePath = Helper.TextureLoadNPC("GraniteElementalInvulnerable");

    private const int FrameW = 20;
    private const int FrameH = 32;
    private const int FrameGap = 2;
    private const int FrameCount = 3;
    private const int FrameTicks = 8;
    private const int CurlAnimTicks = FrameCount * FrameTicks;

    private static Asset<Texture2D> curledTexture;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => entity.type == NPCID.GraniteFlyer;

    private State state = State.Normal;
    private int timer;
    private int attackTimer;
    private int curlCooldown;
    private int lastLife = -1;
    private int groundTicks;

    private bool flagsSaved;
    private bool origNoGravity;
    private bool origNoTileCollide;

    private bool IsProtected => state is State.Curling or State.Falling or State.Uncurling;

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.Write((byte)state);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        State received = (State)binaryReader.ReadByte();
        if (received != state)
        {
            state = received;
            timer = 0;
        }
    }

    private void SetState(NPC npc, State newState)
    {
        state = newState;
        timer = 0;
        groundTicks = 0;
        npc.netUpdate = true;
    }

    private void ApplyFlags(NPC npc)
    {
        bool grounded = state is State.Falling or State.Uncurling;

        if (grounded)
        {
            if (!flagsSaved)
            {
                origNoGravity = npc.noGravity;
                origNoTileCollide = npc.noTileCollide;
                flagsSaved = true;
            }

            npc.noGravity = false;
            npc.noTileCollide = false;
        }
        else if (flagsSaved)
        {
            npc.noGravity = origNoGravity;
            npc.noTileCollide = origNoTileCollide;
            flagsSaved = false;
        }
    }

    public override bool PreAI(NPC npc)
    {
        if (!Sadomasochist.Active)
            return false;

        ApplyFlags(npc);

        if (!IsProtected)
            return true;

        npc.rotation = 0f;

        if (state == State.Curling)
            npc.velocity *= 0.9f;
        else
            npc.velocity.X *= 0.85f;

        return false;
    }

    public override void PostAI(NPC npc)
    {
        if (!Sadomasochist.Active)
            return;

        timer++;

        if (Main.netMode != NetmodeID.MultiplayerClient)
            ServerLogic(npc);

        if (state == State.Telegraph)
            npc.velocity *= 0.88f;

        if (!Main.dedServ)
            Visuals(npc);
    }

    private void ServerLogic(NPC npc)
    {
        if (lastLife < 0)
            lastLife = npc.life;

        bool damaged = npc.life < lastLife;
        lastLife = npc.life;

        if (curlCooldown > 0)
            curlCooldown--;

        switch (state)
        {
            case State.Normal:
                attackTimer++;

                if (damaged && curlCooldown <= 0)
                    SetState(npc, State.Curling);
                else if (attackTimer >= AttackInterval && CanSeeTarget(npc))
                    SetState(npc, State.Telegraph);
                break;

            case State.Telegraph:
                if (damaged && curlCooldown <= 0)
                {
                    SetState(npc, State.Curling);
                }
                else if (timer >= TelegraphTicks)
                {
                    Fire(npc);
                    attackTimer = Main.rand.Next(-60, 1);
                    SetState(npc, State.Normal);
                }
                break;

            case State.Curling:
                if (damaged)
                    SetState(npc, State.Uncurling);
                else if (timer >= CurlAnimTicks)
                    SetState(npc, State.Falling);
                break;

            case State.Falling:
                if (npc.velocity.Y == 0f && npc.collideY)
                    groundTicks++;
                else
                    groundTicks = 0;

                if (damaged || groundTicks >= GroundHoldTicks || timer >= MaxFallTicks)
                    SetState(npc, State.Uncurling);
                break;

            case State.Uncurling:
                if (timer >= CurlAnimTicks)
                {
                    curlCooldown = CurlCooldown;
                    attackTimer = 0;
                    SetState(npc, State.Normal);
                }
                break;
        }
    }

    private static bool CanSeeTarget(NPC npc)
    {
        if (npc.target < 0 || npc.target >= Main.maxPlayers)
            return false;

        Player p = Main.player[npc.target];
        return p.active && !p.dead
            && Vector2.Distance(npc.Center, p.Center) <= AttackRange
            && Collision.CanHit(npc.position, npc.width, npc.height, p.position, p.width, p.height);
    }

    private static void Fire(NPC npc)
    {
        if (npc.target < 0 || npc.target >= Main.maxPlayers)
            return;

        Player p = Main.player[npc.target];
        Vector2 dir = p.Center - npc.Center;
        if (dir == Vector2.Zero)
            return;
        dir.Normalize();

        int damage = npc.GetAttackDamage_ForProjectiles(npc.damage * 0.8f, npc.defDamage * 0.8f);
        Projectile.NewProjectile(
            npc.GetSource_FromAI(),
            npc.Center,
            dir * BoltSpeed,
            ModContent.ProjectileType<GraniteElementalProjectile>(),
            damage,
            1f,
            Main.myPlayer);
    }

    private int CurrentFrame()
    {
        int step = Math.Min(FrameCount - 1, timer / FrameTicks);

        return state switch
        {
            State.Curling => step,
            State.Uncurling => FrameCount - 1 - step,
            _ => FrameCount - 1
        };
    }

    private void Visuals(NPC npc)
    {
        if (state != State.Telegraph)
            return;

        float progress = timer / (float)TelegraphTicks;

        if (timer == 1)
            SoundEngine.PlaySound(SoundID.Item29, npc.Center);

        for (int i = 0; i < 2; i++)
        {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float radius = 70f * (1f - progress) + 10f;
            Vector2 pos = npc.Center + angle.ToRotationVector2() * radius;
            Dust d = Dust.NewDustPerfect(pos, DustID.GemSapphire, (npc.Center - pos) * 0.06f, 100, default, 1.3f);
            d.noGravity = true;
        }

        if (timer % 2 == 0 && npc.target >= 0 && npc.target < Main.maxPlayers)
        {
            Player p = Main.player[npc.target];
            Vector2 dir = p.Center - npc.Center;
            if (dir != Vector2.Zero)
            {
                dir.Normalize();
                for (int s = 1; s <= 8; s++)
                {
                    Dust d = Dust.NewDustPerfect(npc.Center + dir * (24f + s * 20f), DustID.GemSapphire, Vector2.Zero, 100, default, 0.8f + progress * 0.8f);
                    d.noGravity = true;
                }
            }
        }
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (!Sadomasochist.Active)
            return true;

        if (!IsProtected)
            return true;

        curledTexture ??= ModContent.Request<Texture2D>(TexturePath, AssetRequestMode.ImmediateLoad);

        int frame = CurrentFrame();
        Rectangle source = new(frame * (FrameW + FrameGap), 0, FrameW, FrameH);

        Vector2 position = new Vector2(npc.Center.X, npc.Bottom.Y) - screenPos + new Vector2(0f, npc.gfxOffY);
        Vector2 origin = new(FrameW / 2f, FrameH);

        spriteBatch.Draw(curledTexture.Value, position, source, npc.GetAlpha(drawColor), 0f, origin, npc.scale, SpriteEffects.None, 0f);
        return false;
    }

    private static bool IsExplosive(Projectile proj)
        => ProjectileID.Sets.Explosive[proj.type] || proj.aiStyle == ProjAIStyleID.Explosive;

    public override bool? CanBeHitByItem(NPC npc, Player player, Item item)
    {
        if (!IsProtected)
            return null;

        if (player.whoAmI == Main.myPlayer && player.itemAnimation > 0 && Vector2.Distance(player.Center, npc.Center) < 140f)
            BlockEffect(npc);

        return false;
    }

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
    {
        if (!IsProtected || IsExplosive(projectile))
            return null;

        if (projectile.owner == Main.myPlayer && projectile.Colliding(projectile.Hitbox, npc.Hitbox))
            BlockEffect(npc);

        return false;
    }

    private static void BlockEffect(NPC npc)
    {
        if (Main.GameUpdateCount % 5 != 0)
            return;

        for (int i = 0; i < 4; i++)
            Dust.NewDust(npc.position, npc.width, npc.height, DustID.BlueTorch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 0, default, Main.rand.NextFloat(0f, 1.2f));

        SoundEngine.PlaySound(SoundID.Tink, npc.Center);
    }
}