namespace ForgottenSouls.Content.Mechanics;

internal sealed class BetterReforge : GlobalItem
{
#if true
    private const int MaxTries = 12;

    private static bool _sampling;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        => lateInstantiation && entity.maxStack == 1 && (entity.damage > 0 || entity.accessory);

    public int ReforgeCount;

    public override int ChoosePrefix(Item item, UnifiedRandom rand)
    {
        if (_sampling || !Main.InReforgeMenu || ReforgeCount <= 0)
            return 0;

        int tries = Math.Min(MaxTries, 1 + ReforgeCount);
        int bestPrefix = -1;
        int bestValue = int.MinValue;

        _sampling = true;

        try
        {
            for (int i = 0; i < tries; i++)
            {
                Item sample = item.Clone();
                sample.ResetPrefix();

                if (!sample.Prefix(-1) || sample.prefix <= 0)
                    continue;

                if (sample.value > bestValue)
                {
                    bestValue = sample.value;
                    bestPrefix = sample.prefix;
                }
            }
        }
        finally
        {
            _sampling = false;
        }

        return bestPrefix > 0 ? bestPrefix : 0;
    }

    public override void PostReforge(Item item)
    {
        ReforgeCount++;
    }

    public override void SaveData(Item item, TagCompound tag)
    {
        if (ReforgeCount > 0)
            tag["ReforgeCount"] = ReforgeCount;
    }

    public override void LoadData(Item item, TagCompound tag)
    {
        ReforgeCount = tag.GetInt("ReforgeCount");
    }
#endif
}