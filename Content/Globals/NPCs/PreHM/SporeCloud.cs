namespace ForgottenSouls.Content.NPCs;

internal sealed class SporeCloud : ModProjectile
{
    private const int PoisonTicks = 5 * 60;

    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.TruffleSpore;

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 3;
    }

    public override void SetDefaults()
    {
        Projectile.width = 14;
        Projectile.height = 18;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 240;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.knockBack = 0f;
        Projectile.alpha = 155;
    }

    public override void AI()
    {
        Projectile.velocity *= 0.96f;

        Lighting.AddLight(Projectile.Center, 0.08f, 0.15f, 0.35f);

        if (++Projectile.frameCounter >= 6)
        {
            Projectile.frameCounter = 0;
            if (++Projectile.frame >= 3)
            {
                Projectile.frame = 0;
            }
        }

        if (Main.dedServ)
            return;

        float fade = Projectile.timeLeft / 240f;

        if (Main.rand.NextFloat() < 0.6f * fade + 0.1f)
        {
            Vector2 position = Projectile.Center + Main.rand.NextVector2Circular((Projectile.width * 2) / 2f, (Projectile.height * 2) / 2f);
            Dust dust = Dust.NewDustPerfect(position, DustID.GlowingMushroom, new Vector2(0f, -0.4f), 100, default, 1.1f);
            dust.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (!Sadomasochist.Active)
            return false;

        Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

        int frameWidth = 14;
        int frameHeight = 18;
        int gap = 2;

        int frameX = Projectile.frame * (frameWidth + gap);

        Rectangle sourceRectangle = new(frameX, 0, frameWidth, frameHeight);

        Vector2 origin = sourceRectangle.Size() / 2f;

        Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);

        Main.EntitySpriteDraw(
            texture,
            drawPos,
            sourceRectangle,
            lightColor,
            Projectile.rotation,
            origin,
            Projectile.scale,
            SpriteEffects.None,
            0
        );

        return false; 
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        if (!Sadomasochist.Active)
            return;

        target.AddBuff(BuffID.Poisoned, PoisonTicks);
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        modifiers.HitDirectionOverride = 0;
        modifiers.Knockback *= 0f;
    }
}

internal sealed class MushroomSpores : GlobalNPC
{
    private const int CloudIntervalTicks = 24;
    private const int MaxClouds = 60;
    private const int CloudDamage = 10;
    private const int DeathCloudCount = 4;

    private int _timer;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => IsMushroomCreature(npc.type);

    public override void PostAI(NPC npc)
    {
        if (!Sadomasochist.Active)
            return;

        if (npc.friendly || npc.townNPC)
            return;

        if (!Main.dedServ && Main.rand.NextBool(6))
        {
            Vector2 position = npc.Center + Main.rand.NextVector2Circular(npc.width / 2f, npc.height / 2f);
            Dust dust = Dust.NewDustPerfect(position, DustID.GlowingMushroom, Main.rand.NextVector2Circular(0.6f, 0.6f), 120, default, 0.9f);
            dust.noGravity = true;
        }

        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        if (++_timer < CloudIntervalTicks)
            return;

        _timer = 0;
        SpawnCloud(npc, npc.GetSource_FromAI(), Vector2.Zero);
    }

    public override void OnKill(NPC npc)
    {
        if (!Sadomasochist.Active)
            return;

        if (npc.friendly || npc.townNPC || Main.netMode == NetmodeID.MultiplayerClient)
            return;

        for (int i = 0; i < DeathCloudCount; i++)
            SpawnCloud(npc, npc.GetSource_Death(), Main.rand.NextVector2Circular(2f, 2f), true);
    }

    private static void SpawnCloud(NPC npc, Terraria.DataStructures.IEntitySource source, Vector2 velocity, bool ignoreLimit = false)
    {
        if (!ignoreLimit && CountClouds() >= MaxClouds)
            return;

        Vector2 position = npc.Center + Main.rand.NextVector2Circular(npc.width / 3f, npc.height / 3f);

        Projectile.NewProjectile(
            source,
            position,
            velocity,
            ModContent.ProjectileType<SporeCloud>(),
            CloudDamage,
            0f,
            Main.myPlayer);
    }

    private static int CountClouds()
    {
        int type = ModContent.ProjectileType<SporeCloud>();
        int count = 0;

        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile projectile = Main.projectile[i];

            if (projectile.active && projectile.type == type)
                count++;
        }

        return count;
    }

    private static bool IsMushroomCreature(int type)
    {
        if (type >= NPCID.Count || !NPCID.Search.TryGetName(type, out string name))
            return false;

        if (name.Contains("Truffle"))
            return false;

        return name.Contains("Mushroom")
            || name.Contains("Fungi")
            || name.Contains("Fungus")
            || name.Contains("Fungo")
            || name.Contains("Spore")
            || name.Contains("Anomura")
            || name.Contains("Mushi");
    }
}