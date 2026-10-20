namespace ForgottenSouls.Content.Systems;

internal class Darkness : ModPlayer
{
    private const int HitTick = 4 * 60;
    private const int PauseBeforeHitTicks = 60;
    private const int WarningLengthTicks = 2 * 60;

    private const int WarningStartTick = HitTick - PauseBeforeHitTicks - WarningLengthTicks;

    private const float DarknessThreshold = 0.08f;
    private const float Damage = 0.51f;
    private const int FadeOutTicks = 30;

    private int _darkTicks;
    private SlotId _warningSlot;

    private SlotId _fadingSlot = SlotId.Invalid;
    private int _fadeTicksLeft;
    private float _fadeStartVolume;

    private static readonly SoundStyle AttackSound = new("ForgottenSouls/Assets/Sounds/CharlieAttack")
    {
        Volume = 1f,
        PitchVariance = 0.1f,
    };

    private static readonly SoundStyle WarningSound = new("ForgottenSouls/Assets/Sounds/CharlieWarning")
    {
        Volume = 1f,
        PitchVariance = 0.1f,
    };

    public override void PostUpdate()
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        UpdateFade();

        if (Player.dead)
            return;

        if (!IsInDarkness())
        {
            if (_darkTicks > 0)
                StopWarning();

            _darkTicks = 0;
            return;
        }

        _darkTicks++;

        if (_darkTicks == WarningStartTick)
            _warningSlot = SoundEngine.PlaySound(WarningSound, Player.Center);

        if (_darkTicks >= HitTick)
        {
            DealDarknessDamage();

            _darkTicks = 0;
        }
    }

    public override void OnRespawn()
    {
        _darkTicks = 0;
        StopWarning();
    }

    private void StopWarning()
    {
        FinishFade();

        if (SoundEngine.TryGetActiveSound(_warningSlot, out ActiveSound sound))
        {
            _fadingSlot = _warningSlot;
            _fadeStartVolume = sound.Volume;
            _fadeTicksLeft = FadeOutTicks;
        }

        _warningSlot = SlotId.Invalid;
    }

    private void FinishFade()
    {
        if (_fadeTicksLeft > 0 && SoundEngine.TryGetActiveSound(_fadingSlot, out ActiveSound sound))
            sound.Stop();

        _fadeTicksLeft = 0;
    }

    private void UpdateFade()
    {
        if (_fadeTicksLeft <= 0)
            return;

        if (!SoundEngine.TryGetActiveSound(_fadingSlot, out ActiveSound sound))
        {
            _fadeTicksLeft = 0;
            return;
        }

        _fadeTicksLeft--;
        sound.Volume = _fadeStartVolume * _fadeTicksLeft / FadeOutTicks;

        if (_fadeTicksLeft <= 0)
            sound.Stop();
    }

    private bool IsInDarkness()
    {
        Point tile = Player.Center.ToTileCoordinates();
        return Lighting.Brightness(tile.X, tile.Y) < DarknessThreshold;
    }

    private void DealDarknessDamage()
    {
        int damage = Math.Max(1, (int)(Player.statLifeMax2 * Damage));

        SoundEngine.PlaySound(AttackSound, Player.Center);

        Player.statLife -= damage;
        CombatText.NewText(Player.Hitbox, CombatText.DamagedFriendly, damage);

        if (Player.statLife <= 0)
        {
            Player.statLife = 0;
            Player.KillMe(
                PlayerDeathReason.ByOther(6),
                damage,
                0);
        }
    }
}