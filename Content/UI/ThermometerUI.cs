namespace ForgottenSouls.Content.UI;

internal sealed class ThermometerUI : ModSystem
{
    private readonly static string BorderPath = Helper.TextureLoadUI("ThermometerBorder"); 
    private readonly static string FillPath = Helper.TextureLoadUI("ThermometerFill");    

    private const int BorderWidth = 67;
    private const int BorderHeight = 28;
    private const int FillWidth = 61;
    private const int FillHeight = 22;

    private const int FillOffsetX = 3;
    private const int FillOffsetY = 3;

    private const float PositionX = 20f;
    private const float PositionY = 66f;

    private const float HiddenBelow = 0.02f;

    private static readonly Color ColdColor = new(20, 60, 150); 
    private static readonly Color WarmColor = new(255, 215, 60); 
    private static readonly Color HotColor = new(255, 80, 20); 

    private static Asset<Texture2D> border;
    private static Asset<Texture2D> fill;

    public static float Temperature { get; set; }

    public static bool Visible { get; set; } = true;

    public override void Unload()
    {
        border = null;
        fill = null;
        failed = false;
        Temperature = 0f;
        Visible = true;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name == "Vanilla: Inventory");

        if (index < 0)
            return;

        layers.Insert(index + 1, new LegacyGameInterfaceLayer(
            "ForgottenSouls: Thermometer",
            () =>
            {
                Draw(Main.spriteBatch);
                return true;
            },
            InterfaceScaleType.UI));
    }

    private static bool failed;

    private static bool TryLoad(ref Asset<Texture2D> asset, string path)
    {
        if (asset != null)
            return true;

        return ModContent.RequestIfExists(path, out asset, AssetRequestMode.ImmediateLoad);
    }

    private static void Draw(SpriteBatch spriteBatch)
    {
        if (failed)
            return;

        try
        {
            Player player = Main.LocalPlayer;

            if (!Visible || Main.gameMenu || Main.hideUI || player.dead)
                return;

            if (!TryLoad(ref border, BorderPath))
            {
                failed = true;
                ModContent.GetInstance<ForgottenSouls>().Logger.Error($"Thermometer: texture not found: {BorderPath}");
                return;
            }

            bool hasFill = TryLoad(ref fill, FillPath);

            if (!hasFill)
                ModContent.GetInstance<ForgottenSouls>().Logger.Warn($"Thermometer: texture not found: {FillPath}");

            Vector2 origin = new(PositionX, PositionY);

            spriteBatch.Draw(border.Value, new Rectangle((int)origin.X, (int)origin.Y, BorderWidth, BorderHeight), Color.White);

            float amount = MathHelper.Clamp(Math.Abs(Temperature), 0f, 1f);

            if (hasFill && amount >= HiddenBelow)
            {
                int width = Math.Max(1, (int)(FillWidth * amount));
                Rectangle source = new(0, 0, width, FillHeight);

                Color color = Temperature < 0f ? ColdColor : Color.Lerp(WarmColor, HotColor, amount);

                spriteBatch.Draw(fill.Value, origin + new Vector2(FillOffsetX, FillOffsetY), source, color);
            }
        }
        catch (Exception e)
        {
            failed = true;
            ModContent.GetInstance<ForgottenSouls>().Logger.Error("Thermometer: drawing failed, the thermometer is disabled", e);
        }
    }
}