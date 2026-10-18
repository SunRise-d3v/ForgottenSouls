using System;
using System.Reflection;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NoCompromises.Content.Globals;

public class DevianttGift : ModSystem
{
    internal static bool SuppressGifts;

    public override void PostSetupContent()
    {
        try
        {
            if (!ModLoader.TryGetMod("Fargowiltas", out Mod fargo))
                return;

            Type deviType = fargo.Code.GetType("Fargowiltas.NPCs.Deviantt");
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

        if (ModLoader.TryGetMod("FargowiltasSouls", out Mod souls))
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