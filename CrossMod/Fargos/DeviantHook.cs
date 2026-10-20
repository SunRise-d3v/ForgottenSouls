namespace CrossMod;

public class DevianttGift : ModSystem
{
    internal static bool SuppressGifts;

    private delegate void OrigChatButton(UnconsciousDeviantt self, bool firstButton, ref string shopName);

    private const BindingFlags Declared = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    public override void Load()
    {
        MonoModHooks.Add(
            typeof(UnconsciousDeviantt).GetMethod(nameof(UnconsciousDeviantt.OnChatButtonClicked), Declared),
            ChatButtonDetour);

        MonoModHooks.Add(
            typeof(Deviantt).GetMethod(nameof(Deviantt.GetChat), Declared),
            GetChatDetour);
    }

    private static void ChatButtonDetour(OrigChatButton orig, UnconsciousDeviantt self, bool firstButton, ref string shopName)
    {
        if (!firstButton)
        {
            orig(self, firstButton, ref shopName);
            return;
        }

        SuppressGifts = true;
        try
        {
            orig(self, firstButton, ref shopName);
        }
        finally
        {
            SuppressGifts = false;
        }

        GiveStarterBag(self.NPC);
    }

    private static string GetChatDetour(Func<Deviantt, string> orig, Deviantt self)
    {
        bool giftsWereReceived = GiftsReceived;
        bool eternityBefore = WorldSavingSystem.EternityMode;

        SuppressGifts = !giftsWereReceived;

        string result;
        try
        {
            result = orig(self);
        }
        finally
        {
            SuppressGifts = false;
        }

        bool giftsJustGiven = !giftsWereReceived && (eternityBefore || GiftsReceived);

        if (giftsJustGiven)
            GiveStarterBag(self.NPC);

        return result;
    }

    private static bool GiftsReceived => Main.LocalPlayer.FargoSouls().ReceivedMasoGift;

    private static void GiveStarterBag(NPC npc)
    {
        Main.LocalPlayer.QuickSpawnItem(
            npc.GetSource_Misc("DevianttStarterBag"),
            ModContent.ItemType<StarterBag>());
    }
}

public class DevianttGiftSuppress : GlobalItem
{
    public override void OnSpawn(Item item, IEntitySource source)
    {
        if (DevianttGift.SuppressGifts && item.type != ModContent.ItemType<StarterBag>())
            item.TurnToAir();
    }
}