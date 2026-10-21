namespace ForgottenSouls.Content.Globals;

internal sealed class NightChillPlayer : ModPlayer
{
    private const int DebuffTicks = 60;
    private const float LightThreshold = 0.55f;

    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer || Player.dead)
            return;

        if (Main.dayTime || !(Player.ZoneOverworldHeight || Player.ZoneSkyHeight))
            return;

        if (IsWarm())
            return;

        Player.AddBuff(BuffID.Chilled, DebuffTicks);
    }

    private bool IsWarm()
    {
        int heldTile = Player.HeldItem.createTile;
        if (heldTile >= 0 && TileID.Sets.Torch[heldTile])
            return true;

        if (Player.HasBuff(BuffID.Campfire) || Player.HasBuff(BuffID.Warmth))
            return true;

        Point tile = Player.Center.ToTileCoordinates();
        return Lighting.Brightness(tile.X, tile.Y) >= LightThreshold;
    }
}