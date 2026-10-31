namespace ForgottenSouls.Content.UI;

internal sealed class Thermometer : ModSystem
{
#if false
    private readonly static string BorderPath = Helper.TextureLoadUI("ThermometerBorder");

    private const int BorderWidth = 67;
    private const int BorderHeight = 28;

    private const float PositionX = 20f;
    private const float PositionY = 66f;

    private const int FillX = 24;
    private const int FillY = 9;
    private const int FillWidth = 38;
    private const int FillHeight = 10;

    private static readonly Color ColdColor = new(80, 160, 255);
    private static readonly Color HotColor = new(255, 70, 40);

    private static Asset<Texture2D> border;

    public static float Temperature { get; set; }

    public static bool Visible { get; set; } = true;

    public override void Unload()
    {
        border = null;
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

    private static void Draw(SpriteBatch spriteBatch)
    {
        Player player = Main.LocalPlayer;

        if (!Visible || Main.gameMenu || Main.hideUI || player.dead)
            return;

        border ??= ModContent.Request<Texture2D>(BorderPath, AssetRequestMode.ImmediateLoad);

        Vector2 origin = new(PositionX, PositionY);

        float t = MathHelper.Clamp((Temperature + 1f) * 0.5f, 0f, 1f);
        int width = Math.Max(1, (int)(FillWidth * t));

        Rectangle fill = new((int)origin.X + FillX, (int)origin.Y + FillY, width, FillHeight);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, fill, Color.Lerp(ColdColor, HotColor, t));

        spriteBatch.Draw(border.Value, new Rectangle((int)origin.X, (int)origin.Y, BorderWidth, BorderHeight), Color.White);
    }
#endif
}