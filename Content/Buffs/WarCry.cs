namespace ForgottenSouls.Content.Buffs;

internal class WarCry : ModBuff
{
    public override string Texture => Helper.TextureLoad("Buffs", "WarCry");

    public override void SetStaticDefaults()
    {
        Main.buffNoSave[Type] = true;
    }
}

internal class WarCryNpc : GlobalNPC
{
    // Кто может издавать клич
    private static readonly HashSet<int> Criers =
    [
        //NPCID.Zombie,
        //NPCID.BaldZombie,
        //NPCID.Skeleton,
        //NPCID.AngryBones,
        //NPCID.GoblinWarrior,
        //NPCID.GoblinPeon,
    ];

    private const int CryChanceDenominator = 600;
    private const float CryRange = 1000f;
    private const int CryDurationTicks = 30 * 60;
    private const int CryCooldownTicks = 20 * 60;

    private const float CallRadius = 1400f;
    private const int RallyTicks = 8 * 60;
    private const float RallyMinDistance = 100f;

    private const float AuraRadius = 240f;
    private const int AuraRefreshInterval = 6;
    private const int AuraBuffTicks = 90;

    private const float DamageMultiplier = 1.2f;
    private const float DamageReduction = 0.12f;
    private const float SpeedBonus = 0.2f;
    private const float BaseAggroRange = 700f;
    private const float AggressionMultiplier = 2f;
    private const float BasePull = 0.08f;
    private const float MaxPullSpeed = 4.5f;

    private bool _crying;
    private int _cryCooldown = 300;
    private int _rallyTicks;
    private Vector2 _rallyPoint;
    private int _rallyTarget = -1;

    public override bool InstancePerEntity => true;

    public override void PostAI(NPC npc)
    {
        if (!IsAffectable(npc) || Main.netMode == NetmodeID.MultiplayerClient)
            return;

        bool empowered = npc.HasBuff<WarCry>();

        if (Criers.Contains(npc.type))
        {
            if (_crying)
            {
                if (empowered)
                    EmitAura(npc);
                else
                {
                    _crying = false;
                    _cryCooldown = CryCooldownTicks;
                }
            }
            else if (_cryCooldown > 0)
            {
                _cryCooldown--;
            }
            else
            {
                TryCry(npc);
            }
        }

        if (_rallyTicks > 0)
        {
            _rallyTicks--;

            if (_rallyTarget >= 0)
                npc.target = _rallyTarget;

            Pull(npc, _rallyPoint, BasePull * AggressionMultiplier, RallyMinDistance);
        }

        if (empowered)
            ApplyEmpowerment(npc);
    }

    private void TryCry(NPC npc)
    {
        if (!npc.HasValidTarget)
            return;

        Player target = Main.player[npc.target];

        if (npc.Distance(target.Center) > CryRange || !Main.rand.NextBool(CryChanceDenominator))
            return;

        _crying = true;
        npc.AddBuff(ModContent.BuffType<WarCry>(), CryDurationTicks);

        SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.3f }, npc.Center);

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC other = Main.npc[i];

            if (other.whoAmI == npc.whoAmI || !IsAffectable(other) || npc.Distance(other.Center) > CallRadius)
                continue;

            other.GetGlobalNPC<WarCryNpc>().StartRally(target.Center, target.whoAmI);
        }

        if (!Main.dedServ)
        {
            for (int i = 0; i < 66; i++)
            {
                Vector2 direction = (i / 40f * MathHelper.TwoPi).ToRotationVector2();
                Dust dust = Dust.NewDustPerfect(npc.Center, DustID.Torch, direction * 6f, 100, default, 1.8f);
                dust.noGravity = true;
            }
        }

        npc.netUpdate = true;
    }

    private void StartRally(Vector2 point, int playerIndex)
    {
        _rallyPoint = point;
        _rallyTarget = playerIndex;
        _rallyTicks = RallyTicks;
    }

    private static void EmitAura(NPC npc)
    {
        if (Main.GameUpdateCount % AuraRefreshInterval == 0)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];

                if (!IsAffectable(other) || npc.Distance(other.Center) > AuraRadius)
                    continue;

                other.AddBuff(ModContent.BuffType<WarCry>(), AuraBuffTicks);
            }
        }

        if (Main.dedServ)
            return;

        Lighting.AddLight(npc.Center, 0.6f, 0.2f, 0.1f);

        for (int i = 0; i < 7; i++)
        {
            Vector2 offset = Main.rand.NextVector2CircularEdge(AuraRadius, AuraRadius);
            Dust dust = Dust.NewDustPerfect(npc.Center + offset, DustID.Torch, offset.SafeNormalize(Vector2.Zero) * 0.3f, 100, default, 1.2f);
            dust.noGravity = true;
        }
    }

    private static void ApplyEmpowerment(NPC npc)
    {
        if (npc.velocity.Y == 0f && npc.velocity.X != 0f)
        {
            Vector2 extra = Collision.TileCollision(npc.position, new Vector2(npc.velocity.X * SpeedBonus, 0f), npc.width, npc.height);
            npc.position.X += extra.X;
        }

        Player target = Main.player[Player.FindClosest(npc.position, npc.width, npc.height)];

        if (!target.active || target.dead)
            return;

        if (npc.Distance(target.Center) > BaseAggroRange * AggressionMultiplier)
            return;

        npc.target = target.whoAmI;
        Pull(npc, target.Center, BasePull * AggressionMultiplier, RallyMinDistance);
    }

    public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
    {
        if (npc.HasBuff<WarCry>())
            modifiers.SourceDamage *= DamageMultiplier;
    }

    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (npc.HasBuff<WarCry>())
            modifiers.FinalDamage *= 1f - DamageReduction;
    }

    private static void Pull(NPC npc, Vector2 point, float acceleration, float minDistance)
    {
        if (npc.Distance(point) <= minDistance)
            return;

        Vector2 direction = (point - npc.Center).SafeNormalize(Vector2.Zero);

        npc.velocity.X = MathHelper.Clamp(npc.velocity.X + direction.X * acceleration, -MaxPullSpeed, MaxPullSpeed);

        if (npc.noGravity)
            npc.velocity.Y = MathHelper.Clamp(npc.velocity.Y + direction.Y * acceleration, -MaxPullSpeed, MaxPullSpeed);
    }

    private static bool IsAffectable(NPC npc)
        => npc.active
           && !npc.friendly
           && !npc.townNPC
           && !npc.boss
           && npc.damage > 0
           && npc.aiStyle != -1
           && !NPCID.Sets.CountsAsCritter[npc.type];
}
