namespace ForgottenSouls.Content.Systems;

internal sealed class ChallengePlayer : ModPlayer
{
    public bool IronHeart;

    public bool UnbearablePain;

    private int lastLife = -1;

    public override void SaveData(TagCompound tag)
    {
        if (IronHeart)
            tag["IronHeart"] = true;

        if (UnbearablePain)
            tag["UnbearablePain"] = true;
    }

    public override void LoadData(TagCompound tag)
    {
        IronHeart = tag.ContainsKey("IronHeart");
        UnbearablePain = tag.ContainsKey("UnbearablePain");
    }

    public override void OnEnterWorld() => lastLife = -1;

    public override void OnRespawn() => lastLife = -1;

    public override void UpdateLifeRegen()
    {
        if (!IronHeart)
            return;

        if (Player.lifeRegen > 0)
            Player.lifeRegen = 0;
    }

    public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
    {
        if (IronHeart)
            healValue = 0;
    }

    public override void PostUpdate()
    {
        if (!IronHeart || Player.dead)
        {
            lastLife = -1;
            return;
        }

        if (lastLife < 0)
            lastLife = Player.statLife;

        if (Player.statLife > lastLife)
            Player.statLife = lastLife;

        lastLife = Player.statLife;
    }

    public override void PostUpdateEquips()
    {
        if (!UnbearablePain)
            return;

        Player.statLifeMax2 = 1;

        if (Player.statLife > 1)
            Player.statLife = 1;
    }
}