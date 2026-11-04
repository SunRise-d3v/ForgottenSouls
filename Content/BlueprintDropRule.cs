namespace ForgottenSouls.Content.Globals;

internal sealed class BlueprintDropRule : IItemDropRule
{
    private readonly float _chance;
    private readonly int _min;
    private readonly int _max;
    private readonly IItemDropRuleCondition _condition;

    public List<IItemDropRuleChainAttempt> ChainedRules { get; } = new();

    public BlueprintDropRule(
        float chance,
        int min,
        int max,
        IItemDropRuleCondition condition = null)
    {
        _chance = chance;
        _min = min;
        _max = max;
        _condition = condition;
    }

    public bool CanDrop(DropAttemptInfo info)
    {
        return info.player != null
            && (_condition == null || _condition.CanDrop(info))
            && BlueprintAccess.Available(info.player).Count > 0;
    }

    public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
    {
        if (!CanDrop(info))
        {
            return new ItemDropAttemptResult
            {
                State = ItemDropAttemptResultState.DoesntFillConditions
            };
        }

        if (info.rng.NextFloat() >= _chance)
        {
            return new ItemDropAttemptResult
            {
                State = ItemDropAttemptResultState.FailedRandomRoll
            };
        }

        List<int> pool = BlueprintAccess.Available(info.player);

        int count = System.Math.Min(
            info.rng.Next(_min, _max + 1),
            pool.Count
        );

        for (int i = 0; i < count; i++)
        {
            int index = info.rng.Next(pool.Count);

            CommonCode.DropItem(info, pool[index], 1);

            pool.RemoveAt(index);
        }

        return new ItemDropAttemptResult
        {
            State = ItemDropAttemptResultState.Success
        };
    }

    public void ReportDroprates(
        List<DropRateInfo> drops,
        DropRateInfoChainFeed ratesInfo)
    {
        Chains.ReportDroprates(
            ChainedRules,
            1f,
            drops,
            ratesInfo
        );
    }
}