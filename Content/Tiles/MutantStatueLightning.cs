namespace ForgottenSouls.Content;

internal sealed class MutantStatueLightning : GlobalItem
{
    private const int LightningDamage = 60;
    private const float StartHeight = 900f;
    private const float StartSpeed = 12f;

    public override void OnSpawn(Item item, IEntitySource source)
    {
        if (item.type != ModContent.ItemType<Masochist>())
            return;

        bool fromTile = source is EntitySource_TileInteraction or EntitySource_TileBreak or EntitySource_TileUpdate;

        Mod.Logger.Info($"Mutant's Gift появился, источник: {source.GetType().Name}, от тайла: {fromTile}");

        if (!fromTile || Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
            return;

        if (!ModContent.TryFind(Mods.FargowiltasSouls, ModContent.ProjectileType<HostileLightning>().ToString(), out ModProjectile lightning))
        {
            Mod.Logger.Warn("Снаряд FargowiltasSouls/HostileLightning не найден");
            return;
        }

        Vector2 strikePoint = item.Center;
        Vector2 start = strikePoint - new Vector2(0f, StartHeight);

        Projectile.NewProjectile(
            Main.LocalPlayer.GetSource_Misc("MutantStatueLightning"),
            start,
            Vector2.UnitY * StartSpeed,
            lightning.Type,
            LightningDamage,
            0f,
            Main.myPlayer,
            ai0: MathHelper.PiOver2,
            ai1: Main.rand.Next(100));
    }
}