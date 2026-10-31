namespace ForgottenSouls.Content.Globals;

internal sealed class BetterReforge : GlobalItem
{
    public override int ChoosePrefix(Item item, UnifiedRandom rand)
    {
        if (item.prefix == 0)
            return -1;

        Item baseItem = new(item.type);

        float currentRating = GetRating(item);
        int bestPrefix = -1;
        float bestRating = currentRating;

        for (int prefix = 1; prefix < PrefixID.Count; prefix++)
        {
            if (!baseItem.CanApplyPrefix(prefix))
                continue;

            Item candidate = baseItem.Clone();

            if (!candidate.Prefix(prefix))
                continue;

            float rating = GetRating(candidate);

            if (rating > bestRating)
            {
                bestRating = rating;
                bestPrefix = prefix;
            }
        }

        return bestPrefix;
    }

    private static float GetRating(Item item)
    {
        float rating = 0f;

        rating += item.damage * 1.5f;
        rating += (100f - item.useTime) * 0.5f;
        rating += item.crit * 2f;
        rating += item.knockBack * 1.5f;
        rating += item.scale * 2f;
        rating += item.defense * 2f;

        return rating;
    }
}