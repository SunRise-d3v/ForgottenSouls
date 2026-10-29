namespace ForgottenSouls.Content.Difficulty;

internal sealed class SadoDifficultySystem : ModSystem
{
    public static bool Sadomasochist;

    /*private static readonly SoundStyle DeactivateSound = new("NoCompromises/Assets/Sounds/SadomasochistActivate")
    {
        Volume = 1f,
    };*/

    public override void ClearWorld() => Sadomasochist = false;

    public override void SaveWorldData(TagCompound tag)
    {
        if (Sadomasochist)
            tag["Sadomasochist"] = true;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        Sadomasochist = tag.ContainsKey("Sadomasochist");
    }

    public override void PostUpdateEverything()
    {
        if (Main.gameMenu || !Sadomasochist)
            return;

        if (!(Main.masterMode && WorldSavingSystem.ShouldBeEternityMode))
        {
            Sadomasochist = false;
            Main.NewText(Language.GetTextValue("Mods.NoCompromises.UI.SadomasochistDeactivated"), Color.Pink);
            //SoundEngine.PlaySound(DeactivateSound);
        }
    }
}

internal sealed class SadomasochistOption : DifficultyOption
{
    private const string LocPath = "Mods.NoCompromises.UI.";
    private static Asset<Texture2D> _icon;

    public override string NameKey => LocPath + "Sadomasochist";

    private static readonly SoundStyle ActivateSound = new("NoCompromises/Assets/Sounds/SadomasochistActivate")
    {
        Volume = 1f,
    };

    public override string TooltipText() => Language.GetTextValue(LocPath + "SadomasochistOption");

    public override void OnClicked()
    {
        if (!Masochist.CanToggleEternity())
            return;

        bool wasActive = SadoDifficultySystem.Sadomasochist;

        MasoDifficultyOption.EnableMasochist();
        SadoDifficultySystem.Sadomasochist = true;

        if (wasActive)
            return;

        //SoundEngine.PlaySound(ActivateSound);
        Main.NewText(Language.GetTextValue(LocPath + "SadomasochistActivated"), Color.Pink);
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 position, float scale)
    {
        _icon ??= ModContent.Request<Texture2D>(Helper.TextureLoadUI("Sadomasochist"));
        Texture2D texture = _icon.Value;

        Vector2 center = position + new Vector2(Width.Pixels / 2, Height.Pixels / 2) - scale * texture.Size() / 2;
        spriteBatch.Draw(texture, center, texture.Bounds, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);
    }
}

internal sealed class DifficultyMenuHook : ModSystem
{
    private const float Spacing = 60f;
    private const float OptionSize = 40f;

    public override void PostSetupContent()
    {
        MethodInfo updateElements = typeof(DifficultySelectionMenu).GetMethod(nameof(DifficultySelectionMenu.UpdateElements));

        if (updateElements == null)
        {
            Mod.Logger.Warn("DifficultySelectionMenu.UpdateElements не найден, четвёртая сложность не добавлена");
            return;
        }

        MonoModHooks.Add(updateElements, UpdateElementsDetour);
    }

    private static void UpdateElementsDetour(Action<DifficultySelectionMenu> orig, DifficultySelectionMenu self)
    {
        orig(self);

        var options = self.BackPanel.Children.OfType<DifficultyOption>().ToList();
        options.Add(self.AddNewOption(new SadomasochistOption(), 0));

        float start = -Spacing * (options.Count - 1) / 2f;
        for (int i = 0; i < options.Count; i++)
            options[i].Left.Set(start + i * Spacing - OptionSize / 2f, 0.5f);

        float panelWidth = Spacing * options.Count + 20f;
        self.BackPanel.Width.Set(panelWidth, 0);
        self.BackPanel.Left.Set(-panelWidth / 2f, 0.5f);
        self.BackPanel.Recalculate();
    }
}

internal sealed class Sadomasochist
{
    public static bool Active => !Main.gameMenu && SadoDifficultySystem.Sadomasochist;
}