namespace ForgottenSouls.Content.Globals;

internal sealed class BlueprintFishing : ModPlayer
{
    private const float Chance = 0.05f;
    private const float HardmodeChance = 0.10f;

    public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
    {
        if (attempt.crate || npcSpawn > 0)
            return;

        float chance = Main.hardMode
            ? HardmodeChance
            : Chance;

        if (Main.rand.NextFloat() >= chance)
            return;

        List<int> available = BlueprintAccess.Available(Player);

        if (available.Count == 0)
            return;

        itemDrop = available[Main.rand.Next(available.Count)];
        npcSpawn = -1;
    }
}