namespace ForgottenSouls.Content;

/// <summary>
/// Своё окно диалога для NPC: у каждого NPC своя рамка, над ней табличка с именем, а текст печатается по символу со звуком.
/// Кнопки (магазин, закрыть и т.д.) остаются ванильными и работают как обычно.
///
/// Как это устроено: вокруг ванильного слоя диалога вставлены два наших слоя.
///  - Перед ним мы рисуем рамку, имя и печатающийся текст и подменяем текст диалога на пустые строки той же высоты
///    (так ванильное окно рисует кнопки на своих обычных местах);
///  - После него возвращаем настоящий текст.
/// Ванильный фон окна на время диалога делаем прозрачным.
///
/// Рамки: Content/UI/ChatBox_Default.png (запасная, 94x94) и по желанию Content/UI/ChatBox_&lt;ВнутреннееИмяNPC&gt;.png (например ChatBox_Guide.png).
/// </summary>
internal class NpcDialogueSystem : ModSystem
{
    // --- Положение и размеры (повторяют ванильное окно; при необходимости подстройте после запуска) ---
    private const int WrapWidth = 460;          // ширина, по которой переносится текст
    private const int MaxLines = 10;
    private const int LinePitch = 30;           // высота одной строки
    private const int TextOffsetX = 20;         // отступ текста от края рамки
    private const int TextOffsetY = 24;
    private const int FrameBorder = 33;         // ширина кромки в рамке 94x94, которая не растягивается
    private const int NameplateBorder = 9;      // кромка у таблички с именем

    // --- Печать текста ---
    private const float CharsPerTick = 0.6f;    // скорость печати (символов за тик, примерно 36 в секунду)
    private const int SoundEveryNChars = 2;     // звук на каждом n-м символе

    private static readonly Dictionary<string, Asset<Texture2D>> BoxCache = [];

    private static string _lastText;
    private static float _progress;
    private static int _lastSoundIndex;

    private static string _realText;            // настоящий текст диалога на время отрисовки ванильного слоя

    // Данные подмены прозрачного фона ванильного окна
    private static Color[] _originalPixels;
    private static bool _backgroundHidden;

    public override void Unload()
    {
        BoxCache.Clear();

        // Возвращаем фон, пока мод выгружается
        Main.QueueMainThreadAction(() => SetBackgroundHidden(false));
    }

    // ---------- Обновление: печать текста ----------

    public override void PostUpdateEverything()
    {
        if (Main.dedServ)
            return;

        if (!TryGetTalk(out NPC npc, out Texture2D box))
        {
            _lastText = null;
            SetBackgroundHidden(false);
            return;
        }

        SetBackgroundHidden(true);

        string text = Main.npcChatText;

        // Новая реплика: печатаем с начала
        if (text != _lastText)
        {
            _lastText = text;
            _progress = 0f;
            _lastSoundIndex = 0;
        }

        string flat = string.Concat(Wrap(text, out _));
        int total = flat.Length;

        // Клик по рамке пропускает печать
        if (_progress < total && Main.mouseLeft && Main.mouseLeftRelease && GetBoxRectangle(Wrap(text, out int count), count).Contains(Main.MouseScreen.ToPoint()))
            _progress = total;

        _progress = Math.Min(total, _progress + CharsPerTick);

        // Звук на печатаемых символах, у каждого NPC свой тембр
        int index = (int)_progress;

        if (index > _lastSoundIndex)
        {
            for (int i = _lastSoundIndex; i < index && i < total; i++)
            {
                if (!char.IsWhiteSpace(flat[i]) && i % SoundEveryNChars == 0)
                {
                    float pitch = (npc.type * 37 % 100) / 100f * 0.8f - 0.4f;
                    SoundEngine.PlaySound(SoundID.MenuTick with { Volume = 0.35f, Pitch = pitch, MaxInstances = 4 });
                    break;
                }
            }

            _lastSoundIndex = index;
        }
    }

    // ---------- Слои интерфейса ----------

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int index = layers.FindIndex(layer => layer.Name == "Vanilla: NPC / Sign Dialog");

        if (index < 0)
            return;

        // Перед ванильным слоем и после него
        layers.Insert(index, new LegacyGameInterfaceLayer("ForgottenSouls: NPC Dialogue (before)", DrawBefore, InterfaceScaleType.UI));
        layers.Insert(index + 2, new LegacyGameInterfaceLayer("ForgottenSouls: NPC Dialogue (after)", RestoreText, InterfaceScaleType.UI));
    }

    private static bool DrawBefore()
    {
        _realText = null;

        if (!TryGetTalk(out NPC npc, out Texture2D box))
            return true;

        SpriteBatch spriteBatch = Main.spriteBatch;
        var font = FontAssets.MouseText.Value;

        string text = Main.npcChatText;
        string[] lines = Wrap(text, out int lineCount);
        Rectangle area = GetBoxRectangle(lines, lineCount);

        // 1. Рамка
        DrawNineSlice(spriteBatch, box, area, FrameBorder, Color.White);

        // 2. Табличка с именем над рамкой
        string name = string.IsNullOrEmpty(npc.GivenName) ? npc.TypeName : npc.GivenName;
        Vector2 nameSize = font.MeasureString(name);
        Rectangle plate = new(area.X + 18, area.Y - 22, (int)nameSize.X + 28, 32);

        DrawNineSlice(spriteBatch, box, plate, NameplateBorder, Color.White);
        Utils.DrawBorderStringFourWay(spriteBatch, font, name, plate.X + 14, plate.Y + (plate.Height - nameSize.Y) / 2f + 4f, Color.White, Color.Black, Vector2.Zero);

        // 3. Уже напечатанная часть текста
        int remaining = (int)_progress;

        for (int i = 0; i < lines.Length && remaining > 0; i++)
        {
            string line = lines[i];

            if (string.IsNullOrEmpty(line))
                continue;

            string shown = remaining >= line.Length ? line : line[..remaining];

            Utils.DrawBorderStringFourWay(spriteBatch, font, shown, area.X + TextOffsetX, area.Y + i * LinePitch + TextOffsetY, Color.White, Color.Black, Vector2.Zero);

            remaining -= line.Length;
        }

        _realText = text;

        System.Text.StringBuilder hiddenText = new();

        for (int i = 0; i < lineCount; i++)
        {
            hiddenText.Append(' ');

            if (i < lineCount - 1)
                hiddenText.Append('\n');
        }

        Main.npcChatText = hiddenText.ToString();

        return true;
    }

    private static bool RestoreText()
    {
        if (_realText != null)
        {
            Main.npcChatText = _realText;
            _realText = null;
        }

        return true;
    }

    private static bool TryGetTalk(out NPC npc, out Texture2D box)
    {
        npc = null;
        box = null;

        if (Main.gameMenu || Main.dedServ || string.IsNullOrEmpty(Main.npcChatText))
            return false;

        Player player = Main.LocalPlayer;

        if (player.talkNPC < 0 || player.sign >= 0)
            return false;

        npc = Main.npc[player.talkNPC];

        if (!npc.active)
            return false;

        Asset<Texture2D> asset = GetBox(npc);

        if (asset == null)
            return false;

        box = asset.Value;
        return true;
    }

    /// <summary>Рамка NPC: ChatBox_&lt;имя&gt;.png, если такого файла нет, то ChatBox_Default.png. null, если нет и его.</summary>
    private static Asset<Texture2D> GetBox(NPC npc)
    {
        string name = npc.ModNPC?.Name
            ?? (npc.type < NPCID.Count && NPCID.Search.TryGetName(npc.type, out string vanillaName) ? vanillaName : null);

        string key = name ?? "Default";

        if (BoxCache.TryGetValue(key, out Asset<Texture2D> cached))
            return cached;

        Asset<Texture2D> asset = Load(name) ?? Load("Default");

        BoxCache[key] = asset;
        return asset;
    }

    private static Asset<Texture2D> Load(string name)
    {
        if (name == null)
            return null;

        //string path = Helper.TextureLoadUI($"DialogueBox_{name}");
        string path = Helper.TextureLoadUI($"DialogueBox_Neb");

        return ModContent.HasAsset(path)
            ? ModContent.Request<Texture2D>(path, AssetRequestMode.ImmediateLoad)
            : null;
    }

    /// <summary>Разбивает текст на строки так же, как это делает ваниль (иначе кнопки съедут). lineCount это число строк.</summary>
    private static string[] Wrap(string text, out int lineCount)
    {
        string[] lines = Utils.WordwrapString(text, FontAssets.MouseText.Value, WrapWidth, MaxLines, out int lastLine);

        lineCount = lastLine + 1;
        return lines;
    }

    /// <summary>Прямоугольник окна: те же положение и размер, что у ванильного.</summary>
    private static Rectangle GetBoxRectangle(string[] lines, int lineCount)
    {
        int width = TextureAssets.ChatBack.Width();
        int x = Main.screenWidth / 2 - width / 2;
        int y = Main.screenHeight < 700 ? 50 : 100;

        return new Rectangle(x, y, width, (lineCount + 2) * LinePitch);
    }

    /// <summary>Рисует растягиваемую рамку: углы остаются как есть, стороны и середина растягиваются.</summary>
    private static void DrawNineSlice(SpriteBatch spriteBatch, Texture2D texture, Rectangle area, int border, Color color)
    {
        int w = texture.Width;
        int h = texture.Height;

        // Для маленьких прямоугольников кромка уменьшается, чтобы углы не перекрывались
        int b = Math.Min(border, Math.Min(area.Width, area.Height) / 2);

        int[] sourceX = [0, border, w - border, w];
        int[] sourceY = [0, border, h - border, h];
        int[] destX = [area.X, area.X + b, area.Right - b, area.Right];
        int[] destY = [area.Y, area.Y + b, area.Bottom - b, area.Bottom];

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Rectangle destination = new(destX[i], destY[j], destX[i + 1] - destX[i], destY[j + 1] - destY[j]);
                Rectangle source = new(sourceX[i], sourceY[j], sourceX[i + 1] - sourceX[i], sourceY[j + 1] - sourceY[j]);

                spriteBatch.Draw(texture, destination, source, color);
            }
        }
    }

    /// <summary>Делает ванильный фон окна диалога прозрачным и возвращает его обратно.</summary>
    private static void SetBackgroundHidden(bool hidden)
    {
        if (hidden == _backgroundHidden)
            return;

        Texture2D background = TextureAssets.ChatBack.Value;

        if (hidden)
        {
            _originalPixels = new Color[background.Width * background.Height];
            background.GetData(_originalPixels);
            background.SetData(new Color[_originalPixels.Length]);
        }
        else if (_originalPixels != null)
        {
            background.SetData(_originalPixels);
            _originalPixels = null;
        }

        _backgroundHidden = hidden;
    }
}