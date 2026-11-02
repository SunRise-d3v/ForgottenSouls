using System.IO;

using Microsoft.Xna.Framework.Graphics;

using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader.IO;

namespace ForgottenSouls.Content.Globals;

/// <summary>
/// A zombie can spawn armored:
///  - with a shield (Obsidian Shield sprite): reflects 3-4 projectiles fired by players, then breaks;
///    it only protects from the side the zombie is facing;
///  - with a bucket on its head: extra defense until it has taken a few hits, then the bucket falls off;
///  - with both at once.
/// </summary>
internal sealed class ShieldZombie : GlobalNPC
{
    // One roll decides everything, so the chances add up: 5% both, 10% shield only, 10% bucket only.
    // That means 15% of zombies have a shield and 15% have a bucket.
    private const float BothChance = 0.05f;
    private const float ShieldOnlyChance = 0.10f;
    private const float BucketOnlyChance = 0.10f;

    private const int MinHits = 3;
    private const int MaxHits = 4;          // shield uses, inclusive

    private const int MinBucketHits = 3;
    private const int MaxBucketHits = 5;    // hits the bucket survives, inclusive
    private const int BucketDefense = 6;    // extra defense while the bucket is on

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => lateInstantiation && NPCID.Sets.Zombies[entity.type];

    /// <summary>How many projectiles the shield can still reflect (0 = no shield or already broken).</summary>
    public int ShieldHits;

    /// <summary>How many hits the bucket can still take (0 = no bucket or it has fallen off).</summary>
    public int BucketHits;

    private int lastProjectile = -1;
    private uint lastReflectTick;

    // The server decides who gets what
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        float roll = Main.rand.NextFloat();
        bool shield = roll < BothChance + ShieldOnlyChance;                       // both + shield only
        bool bucket = roll < BothChance || (roll >= BothChance + ShieldOnlyChance
            && roll < BothChance + ShieldOnlyChance + BucketOnlyChance);          // both + bucket only

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

        // Something broke on another client: show it here too
        if (ShieldHits > 0 && shield == 0)
            BreakEffect(npc);

        if (BucketHits > 0 && bucket == 0)
            BucketFallEffect(npc);

        ShieldHits = shield;
        BucketHits = bucket;
    }

    // ---- Bucket: more defense, and every hit knocks it a bit loose ----

    public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
    {
        if (BucketHits > 0)
            modifiers.Defense.Flat += BucketDefense;
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit)
    {
        // State changes only on the server (or single-player); clients learn it through the sync
        if (BucketHits <= 0 || Main.netMode == NetmodeID.MultiplayerClient)
            return;

        BucketHits--;

        if (BucketHits <= 0)
            BucketFallEffect(npc);

        npc.netUpdate = true;
    }

    // ---- Shield: reflecting projectiles (runs on the client that owns the projectile) ----

    public override bool? CanBeHitByProjectile(NPC npc, Projectile projectile)
    {
        if (ShieldHits <= 0 || projectile.owner != Main.myPlayer || !IsReflectable(projectile))
            return null;

        // The hook is called for every projectile in the world: only react to real contact
        if (!projectile.Colliding(projectile.Hitbox, npc.Hitbox))
            return null;

        // The shield covers only the side the zombie faces
        int side = Math.Sign(projectile.Center.X - npc.Center.X);

        if (side != 0 && side != npc.direction)
            return null;

        // One contact = one reflection (the projectile may overlap the zombie for a tick or two)
        if (projectile.identity != lastProjectile || Main.GameUpdateCount - lastReflectTick > 10)
        {
            lastProjectile = projectile.identity;
            lastReflectTick = Main.GameUpdateCount;

            Reflect(npc, projectile);
            RegisterHit(npc);
        }

        return false;
    }

    // Arrows, bullets, magic and similar. Minions, sentries, spears, whips and explosions are not reflected.
    private static bool IsReflectable(Projectile projectile)
    {
        if (projectile.minion || projectile.sentry || projectile.aiStyle == ProjAIStyleID.Spear
            || projectile.aiStyle == ProjAIStyleID.Explosive)
            return false;

        return !ProjectileID.Sets.IsAWhip[projectile.type] && !ProjectileID.Sets.Explosive[projectile.type];
    }

    private static void Reflect(NPC npc, Projectile projectile)
    {
        // Send it back with a little scatter and push it out of the zombie
        projectile.velocity = (-projectile.velocity).RotatedByRandom(0.3f);
        projectile.position += projectile.velocity * 2f;
        projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
        projectile.netUpdate = true;

        for (int i = 0; i < 6; i++)
            Dust.NewDust(projectile.position, projectile.width, projectile.height, DustID.Torch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 0, default, 0.9f);

        SoundEngine.PlaySound(SoundID.Tink, npc.Center);
    }

    // A reflection spends one use of the shield; the server is told so everyone sees the same state
    private void RegisterHit(NPC npc)
    {
        ShieldHits--;

        if (ShieldHits <= 0)
            BreakEffect(npc);

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            // Handled in Mod.HandlePacket (packet type 6)
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

    /// <summary>Server side of a reported reflection.</summary>
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

        SoundEngine.PlaySound(SoundID.Item52, npc.Top); // metal clank
    }

    // ---- Drawing the shield in the zombie's hand and the bucket on its head ----
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
            // The vanilla empty bucket sprite; swap it for your own texture if you have one
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