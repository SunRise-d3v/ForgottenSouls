namespace ForgottenSouls.Content.Mechanics;

internal sealed class GnomeThief : GlobalNPC
{
    public const float StealFraction = 0.25f;
    public const int MinSteal = 100;       
    private const int FleeTicksMax = 8 * 60;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => entity.type == NPCID.Gnome;

    public int Stolen;
    private int fleeTicks;

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.Write(Stolen);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        Stolen = binaryReader.ReadInt32();
    }

    public void StartFleeing(NPC npc, int amount)
    {
        Stolen = amount;
        npc.netUpdate = true;
    }

    public override bool PreAI(NPC npc)
    {
        if (Stolen <= 0)
            return true;

        npc.TargetClosest(false);
        Player p = Main.player[npc.target];

        int dir = npc.Center.X >= p.Center.X ? 1 : -1;
        npc.velocity.X = dir * 3.5f;
        npc.direction = npc.spriteDirection = dir;

        if (npc.collideX && npc.velocity.Y == 0f)
            npc.velocity.Y = -7f;

        if (++fleeTicks > FleeTicksMax && Main.netMode != NetmodeID.MultiplayerClient)
        {
            Vector2 center = npc.Center;

            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = ModContent.GetInstance<ForgottenSouls>().GetPacket();
                packet.Write((byte)3);
                packet.Write(center.X);
                packet.Write(center.Y);
                packet.Send();
            }
            else
            {
                SpawnPoof(center);
            }

            npc.active = false;
            npc.netUpdate = true;
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
        }

        return false;
    }

    public override void OnKill(NPC npc)
    {
        if (Stolen <= 0 || Main.netMode == NetmodeID.MultiplayerClient)
            return;

        int amount = Stolen;
        int[] ids = [ItemID.PlatinumCoin, ItemID.GoldCoin, ItemID.SilverCoin, ItemID.CopperCoin];
        int[] values = [1000000, 10000, 100, 1];

        for (int i = 0; i < ids.Length; i++)
        {
            int count = amount / values[i];
            amount %= values[i];

            if (count > 0)
                Item.NewItem(npc.GetSource_Death(), npc.getRect(), ids[i], count);
        }
    }

    public static void SpawnPoof(Vector2 center)
    {
        for (int i = 0; i < 40; i++)
        {
            Vector2 pos = center + Main.rand.NextVector2Circular(12f, 12f);
            Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);

            Dust smoke = Dust.NewDustPerfect(pos, DustID.Smoke, vel, 100, Color.Gray, Main.rand.NextFloat(1.4f, 2.2f));
            smoke.noGravity = true;
        }

        for (int i = 0; i < 12; i++)
        {
            Dust spark = Dust.NewDustPerfect(center, DustID.MagicMirror, Main.rand.NextVector2Circular(3f, 3f), 0, default, 1.2f);
            spark.noGravity = true;
        }

        SoundEngine.PlaySound(SoundID.Item8, center);
    }
}

internal sealed class GnomeThiefPlayer : ModPlayer
{
    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer || Player.dead)
            return;

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.active || npc.type != NPCID.Gnome)
                continue;

            GnomeThief thief = npc.GetGlobalNPC<GnomeThief>();
            if (thief.Stolen > 0 || !npc.Hitbox.Intersects(Player.Hitbox))
                continue;

            long total = Utils.CoinsCount(out _, Player.inventory);
            if (total <= 0)
                continue;

            long want = Math.Max(GnomeThief.MinSteal, (long)(total * GnomeThief.StealFraction));
            int amount = (int)Math.Min(Math.Min(want, total), int.MaxValue);

            if (!Player.BuyItem(amount))
                continue;

            thief.Stolen = amount;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = ModContent.GetInstance<ForgottenSouls>().GetPacket();
                packet.Write((byte)2);
                packet.Write((short)npc.whoAmI);
                packet.Write(amount);
                packet.Send();
            }
            else
            {
                thief.StartFleeing(npc, amount);
            }

            break;
        }
    }
}