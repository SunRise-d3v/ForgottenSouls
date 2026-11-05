namespace DisasterMod.Content.ModPlayers;

internal sealed class DeathEffect : ModSystem
{
    private const string FilterKey = "DisasterMod:DeathEffect";
    private readonly string AudioPath = Helper.LoadSFX("", "PlayerDeath");
    private readonly string TextSoundPath = Helper.LoadSFX("", "EmergingTextOfDeath");

    private static DisasterConfig Config => ModContent.GetInstance<DisasterConfig>();

    private static string Key(int i) => $"Mods.DisasterMod.DeathMessages.Message{i}";

    private const float DissolveStart = 15f;
    private const float DissolveEnd = 69f;
    private const float TextStart = 90f;
    private const float TextFade = 48f;
    private const float MinScale = 0.4f;

    private const int RevealDuration = 70;

    private static string _message = string.Empty;

    private enum State { None, Dead, Reveal }

    private static State _state;
    private static int _timer;
    private static float _scale = 1f;

    private static float _savedSound, _savedMusic, _savedAmbient;
    private static bool _muted;

    private static SoundEffectInstance _audio;
    private static SoundEffectInstance _textSound;

    private static bool _textSoundPlayed;
    private static bool _showText;

    public override void Load()
    {
        if (Main.dedServ)
            return;

        Asset<Effect> shader = ModContent.Request<Effect>(Helper.LoadShader("", "DeathEffect"));

        Filters.Scene[FilterKey] = new Filter(new ScreenShaderData(shader, "DeathEffect"), EffectPriority.VeryHigh);

        try
        {
            SoundEffect effect = ModContent.Request<SoundEffect>(AudioPath, AssetRequestMode.ImmediateLoad).Value;
            _audio = effect.CreateInstance();
        }
        catch
        {
            _audio = null;
        }

        try
        {
            _textSound = ModContent.Request<SoundEffect>(TextSoundPath, AssetRequestMode.ImmediateLoad).Value.CreateInstance();
        }
        catch
        {
            _textSound = null;
        }
    }

    public override void Unload()
    {
        RestoreVolumes();
        _audio?.Dispose();
        _audio = null;
        _textSound?.Dispose();
        _textSound = null;
        _state = State.None;
    }

    public static void Start()
    {
        if (Main.dedServ || _state == State.Dead || !Config.EnableDeathEffect)
            return;

        int target = QuickRespawn.TargetTicks();

        if (target >= 0 && target <= 60)
            return;

        _state = State.Dead;
        _timer = 0;
        _scale = 1f;

        _scale = target < 0
            ? 1f
            : MathHelper.Clamp(target / (float)QuickRespawn.DEFAULT_TICKS, MinScale, 1f);

        _textSoundPlayed = false;
        _showText = target < 0 || target >= QuickRespawn.DEFAULT_TICKS;
        _message = PickMessage();

        if (!Filters.Scene[FilterKey].IsActive())
            Filters.Scene.Activate(FilterKey);

        Filters.Scene[FilterKey].GetShader().UseProgress(0f);

        MuteGame();
        StartAudio();
    }

    private static string PickMessage()
    {
        List<string> list = [];

        for (int i = 1; Language.Exists(Key(i)); i++)
            list.Add(Language.GetTextValue(Key(i)));

        return list.Count > 0 ? list[Main.rand.Next(list.Count)] : string.Empty; /*"You have perished."*/
    }

    public static void Reveal(bool enterWorld = false)
    {
        if (Main.dedServ || _state == State.Reveal)
            return;

        if (enterWorld ? !Config.EnableEnterWorldEffect : !Config.EnableDeathEffect)
            return;

        if (_state == State.None && ModContent.GetInstance<DisasterConfig>().RespawnSeconds <= 1f)
            return;

        if (_state != State.Dead)
            _showText = false;

        if (_state == State.None)
            Filters.Scene.Activate(FilterKey);

        _state = State.Reveal;
        _timer = 0;

        StopAudio();
        RestoreVolumes();
        SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.7f, Pitch = -0.2f }, Main.LocalPlayer.Center);
    }

    private static void Stop()
    {
        _state = State.None;
        _timer = 0;

        StopAudio();
        RestoreVolumes();

        if (!Main.dedServ && Filters.Scene[FilterKey].IsActive())
            Filters.Scene.Deactivate(FilterKey);
    }

    private static void MuteGame()
    {
        if (_muted)
            return;

        _savedSound = Main.soundVolume;
        _savedMusic = Main.musicVolume;
        _savedAmbient = Main.ambientVolume;
        _muted = true;

        SoundEngine.StopTrackedSounds();

        Main.soundVolume = 0f;
        Main.musicVolume = 0f;
        Main.ambientVolume = 0f;
    }

    private static void RestoreVolumes()
    {
        if (!_muted)
            return;

        Main.soundVolume = _savedSound;
        Main.musicVolume = _savedMusic;
        Main.ambientVolume = _savedAmbient;
        _muted = false;
    }

    private static void StartAudio()
    {
        if (_audio == null)
            return;

        try
        {
            _audio.Stop();
            _audio.Volume = MathHelper.Clamp(_savedSound, 0f, 1f);
            _audio.Play();
        }
        catch { }
    }

    private static void StopAudio()
    {
        try { _audio?.Stop(); } catch { }
        try { _textSound?.Stop(); } catch { }
    }

    private static void PlayTextSound()
    {
        if (_textSound == null)
            return;

        try
        {
            _textSound.Stop();
            _textSound.Volume = MathHelper.Clamp(_savedSound, 0f, 1f);
            _textSound.Play();
        }
        catch { }
    }

    public override void PostUpdateEverything()
    {
        if (_state == State.None || Main.dedServ)
            return;

        if (_state == State.Dead && !Config.EnableDeathEffect)
        {
            Stop();
            return;
        }

        _timer++;

        Player player = Main.LocalPlayer;

        if (_state == State.Dead)
        {
            if (!player.dead)
            {
                Reveal();
                return;
            }

            Filters.Scene[FilterKey].GetShader().UseProgress(DeathProgress());

            if (_showText && !_textSoundPlayed && _timer >= TextStart * _scale)
            {
                _textSoundPlayed = true;
                PlayTextSound();
            }

            return;
        }

        float t = _timer / (float)RevealDuration;

        if (t >= 1f)
        {
            Burst(player);
            Stop();
            return;
        }

        Filters.Scene[FilterKey].GetShader().UseProgress(1f - t);
        GatherLight(player, t);
    }

    private static float DeathProgress()
        => MathHelper.Clamp((_timer - DissolveStart * _scale) / ((DissolveEnd - DissolveStart) * _scale), 0f, 1f);

    private static float Blackness()
        => _state == State.Dead ? DeathProgress() : _state == State.Reveal ? 1f - _timer / (float)RevealDuration : 0f;

    private static void GatherLight(Player player, float t)
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2 dir = Main.rand.NextVector2Unit();
            float radius = Main.rand.NextFloat(120f, 220f) * (1f - t * 0.5f);

            Dust dust = Dust.NewDustPerfect(player.Center + dir * radius, DustID.WhiteTorch, -dir * (radius / 18f), 100, default, Main.rand.NextFloat(1.2f, 1.9f));
            dust.noGravity = true;
        }

        Lighting.AddLight(player.Center, 0.8f * t, 0.9f * t, 1.0f * t);
    }

    private static void Burst(Player player)
    {
        for (int i = 0; i < 40; i++)
        {
            Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 8f);
            Dust dust = Dust.NewDustPerfect(player.Center, DustID.WhiteTorch, velocity, 100, default, Main.rand.NextFloat(1.4f, 2.4f));
            dust.noGravity = true;
        }

        for (int i = 0; i < 24; i++)
        {
            Dust dust = Dust.NewDustPerfect(player.Center, DustID.Cloud, Main.rand.NextVector2Circular(4f, 4f), 150, default, 1.4f);
            dust.noGravity = true;
        }

        Lighting.AddLight(player.Center, 1.5f, 1.6f, 1.8f);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (_state == State.None)
            return;

        layers.RemoveAll(l => l.Name == "Vanilla: Death Text");

        layers.Add(new LegacyGameInterfaceLayer("DisasterMod: Death Cover", () =>
        {
            float alpha = MathHelper.Clamp(Blackness(), 0f, 1f);

            if (alpha > 0f)
            {
                Main.spriteBatch.Draw(
                    TextureAssets.MagicPixel.Value,
                    new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                    Color.Black * alpha);
            }

            return true;
        }, InterfaceScaleType.None));

        layers.Add(new LegacyGameInterfaceLayer("DisasterMod: Death Text", () =>
        {
            DrawMessage(Main.spriteBatch);
            return true;
        }, InterfaceScaleType.None));
    }

    private static void DrawMessage(SpriteBatch spriteBatch)
    {
        if (_state == State.None || !_showText)
            return;

        float opacity = _state == State.Dead
            ? MathHelper.Clamp((_timer - TextStart * _scale) / (TextFade * _scale), 0f, 1f)
            : MathHelper.Clamp(1f - _timer / 20f, 0f, 1f);

        if (opacity <= 0f || string.IsNullOrEmpty(_message))
            return;

        DynamicSpriteFont font = FontAssets.DeathText.Value;
        const float scale = 0.7f;

        Vector2 position = new(Main.screenWidth / 2f, Main.screenHeight / 2f);
        Vector2 origin = font.MeasureString(_message) / 2f;

        float pulse = 0.85f + 0.15f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2f);
        Color glow = new Color(200, 0, 0) * (opacity * 0.05f * pulse);

        for (int ring = 1; ring <= 3; ring++)
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 offset = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * i / 12f) * ring * 3f;
                spriteBatch.DrawString(font, _message, position + offset, glow, 0f, origin, scale, SpriteEffects.None, 0f);
            }
        }

        spriteBatch.DrawString(font, _message, position, new Color(170, 0, 0) * opacity, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    public override void OnWorldUnload() => Stop();
}