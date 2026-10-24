namespace ForgottenSouls.Content;

internal class LightHunt
{
    public static readonly HashSet<int> Hunters =
    [
            NPCID.Wraith,
            NPCID.Ghost,
            NPCID.DungeonSpirit
    ];

    public const int HuntRangeX = 60;
    public const int HuntRangeY = 40;
    public const int SpawnLightThreshold = 12;
    public const bool TorchDropsItem = true;

    public static bool IsTorchItem(Item item)
        => item != null && !item.IsAir && item.createTile >= 0 && TileID.Sets.Torch[item.createTile];

    public static bool IsLightTile(int x, int y)
    {
        if (!WorldGen.InWorld(x, y, 5))
            return false;

        Tile t = Main.tile[x, y];
        if (!t.HasTile)
            return false;
        if (TileID.Sets.Torch[t.TileType])
            return true;

        return t.TileType == TileID.Campfire && t.TileFrameY < 36;
    }

    public static void DestroyLightsAround(int cx, int cy, int radius)
    {
        for (int x = cx - radius; x <= cx + radius; x++)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                if (!IsLightTile(x, y))
                    continue;

                Tile t = Main.tile[x, y];
                if (t.TileType == TileID.Campfire)
                {
                    int x0 = x - (t.TileFrameX % 54) / 18;
                    int y0 = y - (t.TileFrameY % 36) / 18;
                    for (int dx = 0; dx < 3; dx++)
                    {
                        for (int dy = 0; dy < 2; dy++)
                        {
                            Tile ct = Main.tile[x0 + dx, y0 + dy];
                            if (ct.HasTile && ct.TileType == TileID.Campfire && ct.TileFrameY < 36)
                                ct.TileFrameY += 36;
                        }
                    }
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendTileSquare(-1, x0, y0, 3, 2);
                }
                else
                {
                    WorldGen.KillTile(x, y, false, false, !TorchDropsItem);
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, x, y, 0f, 0);
                }
            }
        }
    }
}

public class LightHunterNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => LightHunt.Hunters.Contains(entity.type);

    private int timer;
    private bool hasTarget;
    private Vector2 targetPos;
    private Point targetTile = new(-1, -1);

    public override bool PreAI(NPC npc)
    {
        timer++;

        if (hasTarget && targetTile.X >= 0 && !LightHunt.IsLightTile(targetTile.X, targetTile.Y))
            hasTarget = false;

        if (!hasTarget || timer % 30 == 0)
            hasTarget = FindTarget(npc);

        if (!hasTarget)
            return true;

        if (targetTile.X < 0)
        {
            Player p = Main.player[(int)npc.localAI[3]];
            if (!p.active || p.dead || !LightHunt.IsTorchItem(p.HeldItem))
                hasTarget = false;
            else
                targetPos = p.Center;
        }

        if (!hasTarget)
            return true;

        Vector2 toTarget = targetPos - npc.Center;
        float dist = toTarget.Length();
        if (dist > 1f)
            toTarget /= dist;

        float speed = 4.5f;
        npc.velocity = Vector2.Lerp(npc.velocity, toTarget * speed, 0.06f);
        npc.direction = npc.spriteDirection = npc.velocity.X >= 0f ? 1 : -1;
        npc.rotation = npc.velocity.X * 0.05f;

        if (targetTile.X >= 0 && dist < 40f && Main.netMode != NetmodeID.MultiplayerClient)
        {
            Point c = npc.Center.ToTileCoordinates();
            LightHunt.DestroyLightsAround(c.X, c.Y, 3);
            hasTarget = false;
            timer = 0;
        }

        if (Main.netMode == NetmodeID.Server && timer % 20 == 0)
            npc.netUpdate = true;

        return false;
    }

    private bool FindTarget(NPC npc)
    {
        float best = float.MaxValue;
        bool found = false;

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player p = Main.player[i];
            if (!p.active || p.dead || !LightHunt.IsTorchItem(p.HeldItem))
                continue;

            float d = Vector2.DistanceSquared(npc.Center, p.Center);
            if (d < best && d < MathF.Pow(LightHunt.HuntRangeX * 16f, 2))
            {
                best = d;
                found = true;
                targetPos = p.Center;
                targetTile = new Point(-1, -1);
                npc.localAI[3] = i;
            }
        }

        Point c = npc.Center.ToTileCoordinates();
        for (int x = c.X - LightHunt.HuntRangeX; x <= c.X + LightHunt.HuntRangeX; x++)
        {
            for (int y = c.Y - LightHunt.HuntRangeY; y <= c.Y + LightHunt.HuntRangeY; y++)
            {
                if (!LightHunt.IsLightTile(x, y))
                    continue;

                Vector2 pos = new(x * 16f + 8f, y * 16f + 8f);
                float d = Vector2.DistanceSquared(npc.Center, pos);
                if (d < best)
                {
                    best = d;
                    found = true;
                    targetPos = pos;
                    targetTile = new Point(x, y);
                }
            }
        }

        return found;
    }

    public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        => false;
}

public class LightSpawnNPC : GlobalNPC
{
    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        if (!Main.hardMode)
            return;

        int score = player.GetModPlayer<LightHuntPlayer>().LightScore;
        if (score >= LightHunt.SpawnLightThreshold)
        {
            spawnRate = Math.Max(1, (int)(spawnRate * 0.5f));
            maxSpawns = (int)(maxSpawns * 1.5f);
        }
    }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        if (!Main.hardMode)
            return;

        int score = spawnInfo.Player.GetModPlayer<LightHuntPlayer>().LightScore;
        if (score < LightHunt.SpawnLightThreshold)
            return;

        float w = Math.Min(4f, score / (float)LightHunt.SpawnLightThreshold * 1.5f);
        pool[NPCID.Wraith] = w;
        pool[NPCID.Ghost] = w * 0.6f;
        pool[NPCID.DungeonSpirit] = w * 0.6f;
    }
}

public class LightHuntPlayer : ModPlayer
{
    public int LightScore;
    private int scanTimer;
    private int swapCooldown;

    public override void PostUpdate()
    {
        bool isMe = Player.whoAmI == Main.myPlayer;

        if ((isMe || Main.netMode == NetmodeID.Server) && ++scanTimer >= 30)
        {
            scanTimer = 0;
            LightScore = CountLight();
        }

        if (!isMe)
            return;

        if (swapCooldown > 0)
        {
            swapCooldown--;
            return;
        }

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.active || !LightHunt.Hunters.Contains(npc.type))
                continue;
            if (!npc.Hitbox.Intersects(Player.Hitbox))
                continue;

            SwapHotbarSlot();
            swapCooldown = 90;
            break;
        }
    }

    private int CountLight()
    {
        int score = 0;
        Point c = Player.Center.ToTileCoordinates();
        for (int x = c.X - 45; x <= c.X + 45; x++)
        {
            for (int y = c.Y - 28; y <= c.Y + 28; y++)
            {
                if (!LightHunt.IsLightTile(x, y))
                    continue;
                score += Main.tile[x, y].TileType == TileID.Campfire ? 1 : 1;
            }
        }
        return score;
    }

    private void SwapHotbarSlot()
    {
        var candidates = new List<int>();
        for (int s = 0; s < 10; s++)
        {
            if (s == Player.selectedItem)
                continue;
            if (LightHunt.IsTorchItem(Player.inventory[s]))
                continue;
            candidates.Add(s);
        }

        if (candidates.Count == 0)
            return;

        Player.selectedItem = candidates[Main.rand.Next(candidates.Count)];
        SoundEngine.PlaySound(SoundID.Item8, Player.Center);
    }
}