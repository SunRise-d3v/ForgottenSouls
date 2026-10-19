namespace NoCompromises.Content.Biome;

internal class JungleSpawnRate : GlobalNPC
{
    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        if (player.ZoneJungle)
        {
            spawnRate = (int)(spawnRate / 3f);
            maxSpawns = (int)(maxSpawns * 3f);
        }
    }
}