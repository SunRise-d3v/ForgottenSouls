namespace ForgottenSouls.Content.Globals;


public class BossMemory
{
    public const int SampleTicks = 10;
    public const int Bins = 9;
    public const int S = Bins * 2;

    public int Kills;
    public float[] Trans = new float[S * S];
    public float[] VelX = new float[S];
    public float[] VelY = new float[S];
    public float[] VelN = new float[S];

    public static int StateOf(Vector2 vel, bool playerRight)
    {
        int h = vel.X > 1.2f ? 2 : vel.X < -1.2f ? 0 : 1;
        int v = vel.Y > 1.5f ? 2 : vel.Y < -1.5f ? 0 : 1;
        return (playerRight ? Bins : 0) + h * 3 + v;
    }

    private static Vector2 DefaultVel(int state)
    {
        int b = state % Bins;
        int h = b / 3;
        int v = b % 3;
        return new Vector2((h - 1) * 4f, (v - 1) * 5f);
    }

    private Vector2 MeanVel(int state)
    {
        if (VelN[state] < 0.5f)
            return DefaultVel(state);
        return new Vector2(VelX[state], VelY[state]) / VelN[state];
    }

    public void Record(int prev, int next, Vector2 vel)
    {
        int row = prev * S;

        for (int j = 0; j < S; j++)
            Trans[row + j] *= 0.97f;
        Trans[row + next] += 1f;

        VelX[next] += vel.X;
        VelY[next] += vel.Y;
        VelN[next] += 1f;
    }

    public Vector2 PredictOffset(Vector2 playerCenter, Vector2 playerVel, Vector2 bossCenter, Vector2 bossVel)
    {
        float dist = Vector2.Distance(playerCenter, bossCenter);
        float travelTicks = dist / Math.Max(6f, bossVel.Length());
        int steps = (int)MathHelper.Clamp(travelTicks / SampleTicks, 1f, 6f);

        int s = StateOf(playerVel, playerCenter.X > bossCenter.X);
        float[] d = new float[S];
        d[s] = 1f;

        Vector2 offset = playerVel * SampleTicks;

        for (int k = 1; k < steps; k++)
        {
            float[] nd = new float[S];
            for (int a = 0; a < S; a++)
            {
                if (d[a] < 0.0001f)
                    continue;

                float sum = 0.5f;
                for (int b = 0; b < S; b++)
                    sum += Trans[a * S + b];

                nd[a] += d[a] * 0.5f / sum;
                for (int b = 0; b < S; b++)
                    nd[b] += d[a] * Trans[a * S + b] / sum;
            }
            d = nd;

            Vector2 expected = Vector2.Zero;
            for (int b = 0; b < S; b++)
                expected += d[b] * MeanVel(b);

            offset += expected * SampleTicks;
        }

        return offset;
    }
}

public class BossMemorySystem : ModSystem
{
    public static Dictionary<string, BossMemory> Memories = new Dictionary<string, BossMemory>();
    private static readonly Dictionary<int, int> lastState = new Dictionary<int, int>();

    public static string BossKey(NPC npc)
    {
        NPC n = (npc.realLife >= 0 && npc.realLife < Main.maxNPCs) ? Main.npc[npc.realLife] : npc;
        return n.ModNPC != null ? n.ModNPC.FullName : "Terraria/" + NPCID.Search.GetName(n.type);
    }

    public static string MemoryKey(NPC boss, Player p) => BossKey(boss) + "|" + p.name;

    public static int MaxKills(NPC boss)
    {
        int best = 0;
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player p = Main.player[i];
            if (!p.active)
                continue;

            BossMemory m = Get(MemoryKey(boss, p), false);
            if (m != null)
                best = Math.Max(best, m.Kills);
        }
        return best;
    }

    public static BossMemory Get(string key, bool create)
    {
        if (Memories.TryGetValue(key, out BossMemory m))
            return m;
        if (!create)
            return null;
        m = new BossMemory();
        Memories[key] = m;
        return m;
    }

    public override void ClearWorld()
    {
        Memories = [];
        lastState.Clear();
    }

    public override void PostUpdateNPCs()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;
        if (Main.GameUpdateCount % BossMemory.SampleTicks != 0)
            return;

        NPC boss = null;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC n = Main.npc[i];
            if (n.active && n.boss && n.life > 0)
            {
                boss = n;
                break;
            }
        }

        if (boss == null)
        {
            lastState.Clear();
            return;
        }

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player p = Main.player[i];
            if (!p.active || p.dead || Vector2.Distance(p.Center, boss.Center) > 3000f)
            {
                lastState.Remove(i);
                continue;
            }

            BossMemory m = Get(MemoryKey(boss, p), true);
            int s = BossMemory.StateOf(p.velocity, p.Center.X > boss.Center.X);
            if (lastState.TryGetValue(i, out int prev))
                m.Record(prev, s, p.velocity);
            lastState[i] = s;
        }
    }

    public override void SaveWorldData(TagCompound tag)
    {
        var list = new List<TagCompound>();
        foreach (var kv in Memories)
        {
            list.Add(new TagCompound
            {
                ["key"] = kv.Key,
                ["kills"] = kv.Value.Kills,
                ["trans"] = new List<float>(kv.Value.Trans),
                ["vx"] = new List<float>(kv.Value.VelX),
                ["vy"] = new List<float>(kv.Value.VelY),
                ["vn"] = new List<float>(kv.Value.VelN)
            });
        }
        tag["memories"] = list;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        Memories.Clear();
        foreach (TagCompound t in tag.GetList<TagCompound>("memories"))
        {
            var m = new BossMemory { Kills = t.GetInt("kills") };
            Copy(t.GetList<float>("trans"), m.Trans);
            Copy(t.GetList<float>("vx"), m.VelX);
            Copy(t.GetList<float>("vy"), m.VelY);
            Copy(t.GetList<float>("vn"), m.VelN);
            Memories[t.GetString("key")] = m;
        }
    }

    private static void Copy(IList<float> src, float[] dst)
    {
        for (int i = 0; i < dst.Length && i < src.Count; i++)
            dst[i] = src[i];
    }
}

public class BossMemoryNPC : GlobalNPC
{
    private const int MasteryKills = 8;

    private const float MaxLead = 640f;
    private const float MaxRange = 2500f;

    public static readonly HashSet<int> Excluded =
    [
        NPCID.MoonLordCore,
        NPCID.MoonLordHand,
        NPCID.MoonLordHead
    ];

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => lateInstantiation && entity.boss;

    private int shiftedPlayer = -1;
    private Vector2 savedPosition;

    public override bool PreAI(NPC npc)
    {
        shiftedPlayer = -1;

        if (Main.netMode == NetmodeID.MultiplayerClient || Excluded.Contains(npc.type))
            return true;

        if (npc.target < 0 || npc.target >= Main.maxPlayers)
            return true;

        Player p = Main.player[npc.target];
        if (!p.active || p.dead || Vector2.Distance(p.Center, npc.Center) > MaxRange)
            return true;

        BossMemory m = BossMemorySystem.Get(BossMemorySystem.MemoryKey(npc, p), false);
        if (m == null || m.Kills <= 0)
            return true;

        float mastery = Math.Min(1f, m.Kills / (float)MasteryKills);

        Vector2 offset = m.PredictOffset(p.Center, p.velocity, npc.Center, npc.velocity) * mastery;
        if (offset.Length() > MaxLead)
            offset = Vector2.Normalize(offset) * MaxLead;

        Vector2 target = p.Center + offset;
        for (int i = 0; i < 5 && Collision.SolidCollision(target - p.Size / 2f, p.width, p.height); i++)
            target = Vector2.Lerp(p.Center, target, 0.5f);

        savedPosition = p.position;
        shiftedPlayer = npc.target;
        p.position = target - p.Size / 2f;

        return true;
    }

    public override void PostAI(NPC npc)
    {
        if (shiftedPlayer >= 0)
        {
            Main.player[shiftedPlayer].position = savedPosition;
            shiftedPlayer = -1;
        }
    }

    public override void OnKill(NPC npc)
    {
        if (!npc.boss || Main.netMode == NetmodeID.MultiplayerClient)
            return;

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC n = Main.npc[i];
            if (n.active && n.boss && n.whoAmI != npc.whoAmI && n.life > 0)
                return;
        }

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player p = Main.player[i];
            if (!p.active || Vector2.Distance(p.Center, npc.Center) > 4000f)
                continue;

            BossMemorySystem.Get(BossMemorySystem.MemoryKey(npc, p), true).Kills++;
        }
    }
}