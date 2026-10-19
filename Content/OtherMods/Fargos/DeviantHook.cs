namespace OtherMods;

public class DevianttGift : ModSystem
{
    private const string Deviantt = $"{Mods.Fargowiltas}.NPCs.Deviantt";

    internal static bool SuppressGifts;

    public override void PostSetupContent()
    {
        try
        {
            if (!ModLoader.TryGetMod(Mods.Fargowiltas, out Mod fargo))
                return;

            Type deviType = fargo.Code.GetType(Deviantt);
            MethodInfo getChat = deviType?.GetMethod("GetChat", BindingFlags.Public | BindingFlags.Instance);

            if (getChat == null)
            {
                Mod.Logger.Warn("Deviantt.GetChat не найден, хук не установлен");
                return;
            }

            MonoModHooks.Add(getChat, GetChatDetour);
        }
        catch (Exception e)
        {
            Mod.Logger.Error("Не удалось установить хук на Deviantt.GetChat", e);
        }
    }

    private static string GetChatDetour(Func<ModNPC, string> orig, ModNPC self)
    {
        bool giftsPending = false;

        if (ModLoader.TryGetMod(Mods.FargowiltasSouls, out Mod souls))
        {
            giftsPending = (bool)souls.Call("EternityMode") && !(bool)souls.Call("GiftsReceived");
        }

        SuppressGifts = giftsPending;
        string result;

        try
        {
            result = orig(self);
        }
        finally
        {
            SuppressGifts = false;
        }

        if (giftsPending)
        {
            Main.LocalPlayer.QuickSpawnItem(
                self.NPC.GetSource_Misc("DevianttStarterBag"),
                ModContent.ItemType<StarterBag>());
        }

        return result;
    }
}

public class DevianttGiftSuppress : GlobalItem
{
    public override void OnSpawn(Item item, IEntitySource source)
    {
        if (DevianttGift.SuppressGifts)
            item.TurnToAir();
    }
}