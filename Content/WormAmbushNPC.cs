namespace ForgottenSouls.Content;

internal class WormAmbushNPC : GlobalNPC
{
    public static readonly HashSet<int> Heads =
        [
            NPCID.GiantWormHead,
            NPCID.DiggerHead,
            NPCID.DevourerHead,
            NPCID.SeekerHead,
            NPCID.TombCrawlerHead,
            NPCID.LeechHead,
            NPCID.BoneSerpentHead,
            NPCID.DuneSplicerHead
            //NPCID.StardustWormHead,
        ];

    private const float MaxRange = 1600f;
    private const float Depth = 260f;
    private const float SideOffset = 280f;
    private const float AlignWidth = 56f;
    private const float Overshoot = 220f;
    private const int MaxChargeTicks = 180;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        => Heads.Contains(entity.type);

    private bool charging;
    private int chargeTimer;
    private int shiftedPlayer = -1;
    private Vector2 savedPosition;

    public override bool PreAI(NPC npc)
    {
        shiftedPlayer = -1;

        npc.TargetClosest(true);
        if (npc.target < 0 || npc.target >= Main.maxPlayers)
            return true;

        Player p = Main.player[npc.target];
        if (!p.active || p.dead)
        {
            charging = false;
            return true;
        }

        Vector2 head = npc.Center;
        Vector2 pc = p.Center;

        if (Vector2.Distance(head, pc) > MaxRange)
        {
            charging = false;
            return true;
        }

        Vector2 target = GetAmbushPoint(head, pc);

        savedPosition = p.position;
        shiftedPlayer = npc.target;
        p.position = target - new Vector2(p.width / 2f, p.height / 2f);

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

    private Vector2 GetAmbushPoint(Vector2 head, Vector2 pc)
    {
        float dx = Math.Abs(head.X - pc.X);
        float below = head.Y - pc.Y;

        Vector2 chargeTarget = new(pc.X, pc.Y - Overshoot);

        if (charging)
        {
            chargeTimer++;
            if (below < -120f || chargeTimer > MaxChargeTicks)
                charging = false;
            else
                return chargeTarget;
        }

        if (below > 24f && dx < AlignWidth + below * 0.3f)
        {
            charging = true;
            chargeTimer = 0;
            return chargeTarget;
        }

        if (below > -32f)
            return new Vector2(pc.X, pc.Y + Depth);

        float side = head.X >= pc.X ? 1f : -1f;
        return new Vector2(pc.X + side * SideOffset, pc.Y + Depth);
    }
}