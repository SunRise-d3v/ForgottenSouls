namespace ForgottenSouls.Content.Biomes;

internal sealed class InstantLavaDeath : ModPlayer
{
    public override void PostUpdate()
    {
        if (Player.lavaWet && !HasLavaProtection())
        {
            Player.statLife = 0;

            Player.KillMe(Terraria.DataStructures.PlayerDeathReason.ByOther(2), 9999, 0);
        }
    }

    private bool HasLavaProtection()
    {
        return Player.lavaImmune || Player.HasBuff(BuffID.ObsidianSkin);
    }
}