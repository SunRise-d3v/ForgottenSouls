namespace NoCompromises.Content.Systems;

internal class HungerPlayer : ModPlayer
{
    private const int PeckishDuration = 5 * 60 * 60;

    private static readonly int[] HungerBuffs =
    [
        BuffID.NeutralHunger, BuffID.Hunger, BuffID.Starving,
    ];

    private static readonly int[] FoodBuffs =
    [
        BuffID.WellFed, BuffID.WellFed2, BuffID.WellFed3,
    ];

    private bool _savedFlag;

    public override void PreUpdateBuffs()
    {
        _savedFlag = Main.dontStarveWorld;
        Main.dontStarveWorld = true;
    }

    public override void PostUpdateBuffs() => Main.dontStarveWorld = _savedFlag;

    public override void PostUpdate()
    {
        if (Player.dead || Player.whoAmI != Main.myPlayer)
            return;

        if (HasAny(HungerBuffs) || HasAny(FoodBuffs))
            return;

        Player.AddBuff(BuffID.NeutralHunger, PeckishDuration);
    }

    private bool HasAny(int[] buffs)
    {
        foreach (int buff in buffs)
            if (Player.HasBuff(buff))
                return true;

        return false;
    }
}