namespace ForgottenSouls.Content.Entities;

internal class ZombieLunge : GlobalNPC
{
    private const float SpeedBonus = 0.575f;
    private const float MinLungeDistance = 70f;
    private const float MaxLungeDistance = 260f;
    private const int WindupTicks = 35;
    private const int LungeTicks = 15;
    private const float LungeSpeed = 6.5f;
    private const float LungeHop = -3.5f;
    private const int CooldownMin = 275;
    private const int CooldownMax = 500;
    private const int AttemptChance = 30;

    private static readonly HashSet<int> Zombies =
    [
        NPCID.Zombie,
        NPCID.BaldZombie,
        NPCID.PincushionZombie,
        NPCID.SlimedZombie,
        NPCID.SwampZombie,
        NPCID.TwiggyZombie,
        NPCID.FemaleZombie,
        NPCID.ZombieDoctor,
        NPCID.ZombieSuperman,
        NPCID.ZombiePixie,
        NPCID.ZombieSweater,
        NPCID.BaldZombie,
        NPCID.BigBaldZombie,
        NPCID.SmallBaldZombie,
        NPCID.BigFemaleZombie,
        NPCID.BigPincushionZombie,
        NPCID.BigSlimedZombie,
        NPCID.BigRainZombie,
        NPCID.MaggotZombie,

        NPCID.BloodZombie,
        NPCID.ZombieRaincoat,
        NPCID.ZombieMushroom,
        NPCID.ZombieMushroomHat,
        NPCID.ZombieEskimo,
        NPCID.ZombieMerman,

        NPCID.ZombieElf,
        NPCID.ZombieElfBeard,
        NPCID.ZombieElfGirl,
        NPCID.ZombieXmas,

        /*NPCID.ArmedZombie,
        NPCID.ArmedZombieCenx,
        NPCID.ArmedZombiePinstripe,
        NPCID.ArmedZombieSlim,
        NPCID.ArmedZombieSwamp,
        NPCID.ArmedZombieTwiggy,
        NPCID.ArmedTorchZombie,*/

        //NPCID.ArmedZombieEskimo,
        //NPCID.ArmedZombieRaincoat,
    ];


    private enum State
    {
        Walk,
        Windup,
        Lunge,
    }

    private State _state;
    private int _timer;
    private int _cooldown = 60;
    private int _direction;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => Zombies.Contains(npc.type);

    public override void PostAI(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        switch (_state)
        {
            case State.Walk:
                UpdateWalk(npc);
                break;

            case State.Windup:
                UpdateWindup(npc);
                break;

            case State.Lunge:
                UpdateLunge(npc);
                break;
        }
    }

    private void UpdateWalk(NPC npc)
    {
        if (npc.velocity.Y == 0f && npc.velocity.X != 0f)
        {
            Vector2 extra = Collision.TileCollision(npc.position, new Vector2(npc.velocity.X * SpeedBonus, 0f), npc.width, npc.height);
            npc.position.X += extra.X;
        }

        if (_cooldown > 0)
        {
            _cooldown--;
            return;
        }

        if (!npc.HasValidTarget)
            return;

        Player target = Main.player[npc.target];
        float distance = npc.Distance(target.Center);

        if (npc.velocity.Y != 0f || distance < MinLungeDistance || distance > MaxLungeDistance)
            return;

        if (!Collision.CanHitLine(npc.position, npc.width, npc.height, target.position, target.width, target.height))
            return;

        if (!Main.rand.NextBool(AttemptChance))
            return;

        _state = State.Windup;
        _timer = WindupTicks;
        _direction = target.Center.X > npc.Center.X ? 1 : -1;
    }

    private void UpdateWindup(NPC npc)
    {
        npc.velocity.X = 0f;

        if (npc.HasValidTarget)
            _direction = Main.player[npc.target].Center.X > npc.Center.X ? 1 : -1;

        npc.direction = npc.spriteDirection = _direction;

        SpitBlood(npc);

        if (--_timer > 0)
            return;

        _state = State.Lunge;
        _timer = LungeTicks;

        if (npc.velocity.Y == 0f)
            npc.velocity.Y = LungeHop;
    }

    private void UpdateLunge(NPC npc)
    {
        npc.velocity.X = _direction * LungeSpeed;
        npc.direction = npc.spriteDirection = _direction;

        if (--_timer > 0)
            return;

        _state = State.Walk;
        _cooldown = Main.rand.Next(CooldownMin, CooldownMax + 1);
    }

    private void SpitBlood(NPC npc)
    {
        if (Main.dedServ)
            return;

        Vector2 mouth = npc.Center + new Vector2(_direction * npc.width * 0.45f, -npc.height * 0.2f);

        for (int i = 0; i < 2; i++)
        {
            Vector2 velocity = new(_direction * Main.rand.NextFloat(-2f, 0.3f), Main.rand.NextFloat(1.25f, 2.85f));
            Dust.NewDustPerfect(mouth, DustID.Blood, velocity, 0, default, Main.rand.NextFloat(0.85f, 1.2f));
        }
    }
}