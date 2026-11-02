namespace ForgottenSouls.Content.Mechanics;

internal sealed class BetterReforge : GlobalItem
{
#if false
    private static bool evaluating;

    public override bool InstancePerEntity => true;

    private bool reforging;
    private int baseline;  
    private int best;

    public override void PreReforge(Item item)
    {
        reforging = true;
        baseline = Math.Max(0, Rank(item, item.prefix));
        best = baseline;

        int count = PrefixLoader.PrefixCount;

        for (int prefix = 1; prefix < count; prefix++)
            best = Math.Max(best, Rank(item, prefix));
    }

    public override void PostReforge(Item item) => reforging = false;

    public override bool AllowPrefix(Item item, int pre)
    {
        if (evaluating || !reforging || pre <= 0)
            return true;

        int rank = Rank(item, pre);

        if (rank < 0)
            return true;

        return best > baseline ? rank > baseline : rank >= baseline;
    }

    private static int Rank(Item source, int prefix)
    {
        evaluating = true;

        try
        {
            Item test = new();
            test.SetDefaults(source.type);

            if (prefix > 0 && (!test.Prefix(prefix) || test.prefix != prefix))
                return -1;

            return test.value;
        }
        finally
        {
            evaluating = false;
        }
    }
#endif
}