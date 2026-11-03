namespace ForgottenSouls.Content.NPCs;

internal sealed class ShieldZombie : GlobalNPC
{
    private const float BothChance = 0.05f;
    private const float ShieldOnlyChance = 0.10f;
    private const float BucketOnlyChance = 0.10f;

    private const int MinHits = 3;
    private const int MaxHits = 4;

    private const int MinBucketHits = 3;
    private const int MaxBucketHits = 5;
    private const int BucketDefense = 6;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => lateInstantiation && NPCID.Sets.Zombies[entity.type];

    public int ShieldHits;
    public int BucketHits;

    private int lastProjectile = -1;
    private uint lastReflectTick;

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        float roll = Main.rand.NextFloat();
        bool shield = roll < BothChance + ShieldOnlyChance;
        bool bucket = roll < BothChance || (roll >= BothChance + ShieldOnlyChance
            && roll < BothChance + ShieldOnlyChance + BucketOnlyChance);

        if (shield)
            ShieldHits = Main.rand.Next(MinHits, MaxHits + 1);

        if (bucket)
            BucketHits = Main.rand.Next(MinBucketHits, MaxBucketHits + 1);

        if (shield || bucket)
            npc.netUpdate = true;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.Write((byte)ShieldHits);
        binaryWriter.Write((byte)BucketHits);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        int shield = binaryReader.ReadByte();
        int bucket = binaryReader.ReadByte();

        if (ShieldHits > 0 && shield == 0)
            BreakEffect(npc);

        if (BucketHits > 0 && bucket == 0)
            BucketFallEffect(npc);

        ShieldHits = shield;
        BucketHits = bucket;
    }

    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (BucketHits > 0)
            modifiers.Defense.Flat += BucketDefense;
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit)
    {
        if (BucketHits <= 0 || Main.netMode == NetmodeID.MultiplayerClient)
            return;

        BucketHits--;

        if (BucketHits <= 0)
            BucketFallEffect(npc);

        npc.netUpdate = true;
    }

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
    {
        if (ShieldHits <= 0 || projectile.owner != Main.myPlayer || !IsReflectable(projectile))
            return null;

        if (!projectile.Colliding(projectile.Hitbox, npc.Hitbox))
            return null;

        int side = Math.Sign(projectile.Center.X - npc.Center.X);

        if (side != 0 && side != npc.direction)
            return null;

        if (projectile.identity != lastProjectile || Main.GameUpdateCount - lastReflectTick > 10)
        {
            lastProjectile = projectile.identity;
            lastReflectTick = Main.GameUpdateCount;

            Reflect(npc, projectile);
            RegisterHit(npc);
        }

        return false;
    }

    private static bool IsReflectable(Projectile projectile)
    {
        if (projectile.minion || projectile.sentry || projectile.aiStyle == ProjAIStyleID.Spear
            || projectile.aiStyle == ProjAIStyleID.Explosive)
            return false;

        return !ProjectileID.Sets.IsAWhip[projectile.type] && !ProjectileID.Sets.Explosive[projectile.type];
    }

    private static void Reflect(NPC npc, Projectile projectile)
    {
        projectile.velocity = (-projectile.velocity).RotatedByRandom(0.3f);
        projectile.position += projectile.velocity * 2f;
        projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
        projectile.netUpdate = true;

        for (int i = 0; i < 6; i++)
            Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Torch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 0, default, 0.9f);

        SoundEngine.PlaySound(SoundID.Tink, npc.Center);
    }

    private void RegisterHit(NPC npc)
    {
        ShieldHits--;

        if (ShieldHits <= 0)
            BreakEffect(npc);

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            ModPacket packet = ModContent.GetInstance<ForgottenSouls>().GetPacket();
            packet.Write((byte)6);
            packet.Write((short)npc.whoAmI);
            packet.Send();
        }
        else
        {
            npc.netUpdate = true;
        }
    }

    internal static void ServerHit(int npcIndex)
    {
        if (npcIndex < 0 || npcIndex >= Main.maxNPCs)
            return;

        NPC npc = Main.npc[npcIndex];

        if (npc.active && npc.TryGetGlobalNPC(out ShieldZombie shield) && shield.ShieldHits > 0)
        {
            shield.ShieldHits--;
            npc.netUpdate = true;
        }
    }

    private static void BreakEffect(NPC npc)
    {
        for (int i = 0; i < 20; i++)
            Dust.NewDust(npc.position, npc.width, npc.height, DustID.Obsidian, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 0, default, 1.2f);

        SoundEngine.PlaySound(SoundID.Shatter, npc.Center);
    }

    private static void BucketFallEffect(NPC npc)
    {
        for (int i = 0; i < 10; i++)
            Dust.NewDust(npc.Top, npc.width, 8, DustID.Iron, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, 0f), 0, default, 1.1f);

        SoundEngine.PlaySound(SoundID.Item52, npc.Top);
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.IsABestiaryIconDummy)
            return;

        SpriteEffects effects = npc.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        if (ShieldHits > 0)
        {
            Texture2D texture = ModContent.Request<Texture2D>(
                Helper.TextureLoadItem("ObsidianShield")
            ).Value;

            Vector2 position = npc.Center - screenPos + new Vector2(npc.direction * 12f, 2f + npc.gfxOffY);

            spriteBatch.Draw(
                texture,
                position,
                null,
                drawColor,
                0f,
                texture.Size() / 2f,
                0.8f,
                effects,
                0f
            );
        }

        if (BucketHits > 0)
        {
            Main.instance.LoadItem(ItemID.EmptyBucket);
            Texture2D bucket = TextureAssets.Item[ItemID.EmptyBucket].Value;

            Vector2 position = npc.Top - screenPos + new Vector2(npc.direction * 1f, 8f + npc.gfxOffY);

            spriteBatch.Draw(
                bucket,
                position,
                null,
                drawColor,
                180f,
                bucket.Size() / 2f,
                0.9f,
                effects,
                0f
            );
        }
    }
}