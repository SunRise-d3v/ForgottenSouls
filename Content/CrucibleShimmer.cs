namespace ForgottenSouls.Content;

internal class CrucibleShimmer : GlobalItem
{
#if false
    private const string CrucibleMod = "Fargowiltas";
    private const string CrucibleName = "CrucibleCosmos";
 
    private const string ResultItemName = "StarterBag";
 
    private const double WindowStart = 4.5 * 3600;
    private const double WindowEnd = 5.5 * 3600;
 
    private static int _crucibleType = -1;
 
    public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
    {
        if (!item.shimmerWet || Main.netMode == NetmodeID.MultiplayerClient)
            return;
 
        if (!IsCrucible(item) || !IsRightTime())
            return;
 
        if (!Mod.TryFind(ResultItemName, out ModItem result))
        {
            Mod.Logger.Warn($"CrucibleShimmer: предмет {ResultItemName} не найден");
            return;
        }
 
        int stack = item.stack;
        Vector2 position = item.Center;
 
        int index = Item.NewItem(
            new EntitySource_Misc("CrucibleShimmer"),
            (int)position.X,
            (int)position.Y,
            0,
            0,
            result.Type,
            stack);
 
        if (index >= 0 && index < Main.maxItems)
            Main.item[index].velocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), -5f);
 
        if (!Main.dedServ)
        {
            for (int i = 0; i < 20; i++)
                Dust.NewDust(position - new Vector2(12f), 24, 24, DustID.MagicMirror, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, 0f));
        }
 
        item.TurnToAir();
    }
 
    private static bool IsRightTime()
        => Main.bloodMoon && !Main.dayTime && Main.time >= WindowStart && Main.time < WindowEnd;
 
    private static bool IsCrucible(Item item)
    {
        if (_crucibleType < 0 && ModContent.TryFind(CrucibleMod, CrucibleName, out ModItem crucible))
            _crucibleType = crucible.Type;
 
        return _crucibleType >= 0 && item.type == _crucibleType;
    }
#endif
}