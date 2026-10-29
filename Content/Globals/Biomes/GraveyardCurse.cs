namespace ForgottenSouls.Content.Biomes;

internal sealed class GraveyardCurse : ModPlayer
{
    internal const float SpawnBonus = 0.15f;

    public override void PostUpdate()
    {
        if (Player.ZoneGraveyard && Sadomasochist.Active)
            Player.AddBuff(BuffID.Weak, 60);
    }
}

internal sealed class GraveyardSpawnNPC : GlobalNPC
{
    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        if (!Sadomasochist.Active)
            return;

        if (!player.ZoneGraveyard)
            return;

        spawnRate = Math.Max(1, (int)(spawnRate / (1f + GraveyardCurse.SpawnBonus)));
        maxSpawns = (int)(maxSpawns * (1f + GraveyardCurse.SpawnBonus));
    }
}