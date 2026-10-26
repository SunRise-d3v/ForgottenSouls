namespace ForgottenSouls.Content;

internal class VampireDagger : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.VampireKnife;

    public override void SetDefaults()
    {
        Projectile.width = 14;
        Projectile.height = 30;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 300;
        Projectile.tileCollide = true;
        Projectile.aiStyle = -1;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        if (Main.rand.NextBool(3))
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 100, default, 0.9f);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        int heal = info.Damage;
        int npcIndex = (int)Projectile.ai[0];

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)0);
            packet.Write((short)npcIndex);
            packet.Write(heal);
            packet.Send();
        }
        else
        {
            HealVampire(npcIndex, heal);
        }
    }

    public static void HealVampire(int npcIndex, int amount)
    {
        if (npcIndex < 0 || npcIndex >= Main.maxNPCs)
            return;

        NPC npc = Main.npc[npcIndex];
        if (!npc.active || npc.type != NPCID.Vampire)
            return;

        int heal = Math.Min(amount, npc.lifeMax - npc.life);
        if (heal <= 0)
            return;

        npc.life += heal;
        npc.HealEffect(heal);
        npc.netUpdate = true;
    }
}

internal class VampireDaggerNPC : GlobalNPC
{
    private const float AttackRange = 700f;
    private const float Speed = 9f;
    private const float Spread = 0.35f;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => entity.type == NPCID.Vampire;

    private int timer;

    public override void PostAI(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient || npc.target < 0 || npc.target >= Main.maxPlayers)
            return;

        Player target = Main.player[npc.target];
        if (!target.active || target.dead)
            return;

        if (++timer < 320) 
            return;

        if (Vector2.Distance(npc.Center, target.Center) > AttackRange ||
            !Collision.CanHit(npc.position, npc.width, npc.height, target.position, target.width, target.height))
            return;

        timer = Main.rand.Next(-60, 1);

        int count = Main.rand.Next(3, 6); // 3-5 кинжалов
        int damage = npc.GetAttackDamage_ForProjectiles(npc.damage * 0.6f, npc.defDamage * 0.6f);
        float baseAngle = (target.Center - npc.Center).ToRotation();

        for (int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0.5f : i / (float)(count - 1);
            float angle = baseAngle + MathHelper.Lerp(-Spread / 2f, Spread / 2f, t);

            Projectile.NewProjectile(
                npc.GetSource_FromAI(),
                npc.Center,
                angle.ToRotationVector2() * Speed,
                ModContent.ProjectileType<VampireDagger>(),
                damage,
                1f,
                Main.myPlayer,
                ai0: npc.whoAmI);
        }
    }
}