namespace ForgottenSouls.Content;

internal static partial class Pool
{
    public static readonly HashSet<int> Bats =
    [
        NPCID.CaveBat,
        NPCID.GiantBat,
        NPCID.JungleBat,
        NPCID.IceBat,
        NPCID.Hellbat,
        NPCID.Lavabat,
        NPCID.SporeBat,
        NPCID.VampireBat,
        NPCID.IlluminantBat,
        NPCID.GiantFlyingFox
    ];

    public static readonly HashSet<int> Excluded =
    [
        NPCID.DungeonGuardian,
        NPCID.Tim,
        NPCID.RuneWizard,
        NPCID.SkeletonMerchant,
        NPCID.SkeletronHand,
        NPCID.SkeletronHead,
        NPCID.SkeletronPrime
    ];

    public static readonly HashSet<int> ExtraSkeletons =
[
        NPCID.Skeleton,
        NPCID.SkeletonAlien,
        NPCID.SkeletonArcher,
        NPCID.SkeletonAstonaut,
        NPCID.SkeletonCommando,
        NPCID.SkeletonSniper,
        NPCID.SkeletonTopHat,
        NPCID.ArmoredSkeleton,
        NPCID.BigHeadacheSkeleton,
        NPCID.BigMisassembledSkeleton,
        NPCID.BigPantlessSkeleton,
        NPCID.BigSkeleton,
        NPCID.BoneThrowingSkeleton,
        NPCID.BoneThrowingSkeleton2,
        NPCID.BoneThrowingSkeleton3,
        NPCID.BoneThrowingSkeleton4,
        NPCID.DD2SkeletonT1,
        NPCID.DD2SkeletonT3,
        NPCID.GreekSkeleton,
        NPCID.HeadacheSkeleton,
        NPCID.HeavySkeleton,
        NPCID.MisassembledSkeleton,
        NPCID.PantlessSkeleton,
        NPCID.SmallHeadacheSkeleton,
        NPCID.SmallMisassembledSkeleton,
        NPCID.SmallPantlessSkeleton,
        NPCID.SmallSkeleton,
        NPCID.SporeSkeleton,
        NPCID.TacticalSkeleton
    ];

    public static readonly HashSet<int> Eyes =
    [
        NPCID.DemonEye,
        NPCID.CataractEye,
        NPCID.SleepyEye,
        NPCID.DialatedEye,
        NPCID.GreenEye,
        NPCID.PurpleEye,
        NPCID.DemonEyeOwl,
        NPCID.DemonEyeSpaceship
    ];

    public static readonly HashSet<int> Goblins =
[
        NPCID.GoblinPeon,
        NPCID.GoblinThief,
        NPCID.GoblinWarrior,
        NPCID.GoblinSorcerer,
        NPCID.GoblinArcher,
        NPCID.GoblinSummoner,
        NPCID.GoblinScout
    ];

    public static readonly HashSet<int> GoldCritters =
    [
        NPCID.GoldBird,
        NPCID.GoldBunny,
        NPCID.GoldButterfly,
        NPCID.GoldDragonfly,
        NPCID.GoldFrog,
        NPCID.GoldGoldfish,
        NPCID.GoldGrasshopper,
        NPCID.GoldLadyBug,
        NPCID.GoldMouse,
        NPCID.GoldSeahorse,
        NPCID.SquirrelGold,
        NPCID.GoldWorm,
    ];

    public static readonly HashSet<int> LargeSlimes =
    [
        NPCID.UmbrellaSlime,
        NPCID.ToxicSludge,
        NPCID.SpikedJungleSlime,
        NPCID.SpikedIceSlime,
        NPCID.MotherSlime,
        NPCID.DungeonSlime,
        NPCID.CorruptSlime,
        NPCID.Crimslime,
        NPCID.IlluminantSlime,
        NPCID.RainbowSlime
    ];

    public static readonly HashSet<int> Worms =
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

    public static readonly HashSet<int> Zombies =
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
        NPCID.ZombieXmas

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

    public static (bool Beaten, int Type)[] Bosses =
        [
            (NPC.downedSlimeKing, NPCID.KingSlime),
            (NPC.downedBoss1, NPCID.EyeofCthulhu),
            (NPC.downedQueenBee, NPCID.QueenBee),
            (NPC.downedBoss3, NPCID.SkeletronHead),
            (NPC.downedDeerclops, NPCID.Deerclops),

            (Main.hardMode && NPC.downedQueenSlime, NPCID.QueenSlimeBoss),
            (Main.hardMode && NPC.downedMechBoss1, NPCID.TheDestroyer),
            (Main.hardMode && NPC.downedMechBoss3, NPCID.SkeletronPrime),
            (Main.hardMode && NPC.downedPlantBoss, NPCID.Plantera),
            (Main.hardMode && NPC.downedGolemBoss, NPCID.Golem),
            (Main.hardMode && NPC.downedFishron, NPCID.DukeFishron),
            (Main.hardMode && NPC.downedEmpressOfLight, NPCID.EmpressButterfly),
            (Main.hardMode && NPC.downedMoonlord, NPCID.MoonLordCore)
    ];
}