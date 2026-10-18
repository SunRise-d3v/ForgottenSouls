using Microsoft.Xna.Framework;

using Terraria.Audio;

namespace NoCompromises.Content.Globals;

internal class Dash : ModPlayer
{
    private readonly int _dashCooldown = 120;
    private readonly int _dashDuration = 25;

    private int _dashDir = -1;

    private int _dashDelay = 0;
    private int _dashTimer = 0;

    private const int DASH_RIGHT = 2;
    private const int DASH_LEFT = 3;

    private float _dashVelocity;

    public override void ResetEffects()
    {
        if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DASH_RIGHT] < 15)
            _dashDir = DASH_RIGHT;
        else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DASH_LEFT] < 15)
            _dashDir = DASH_LEFT;
        else
            _dashDir = -1;

        _dashVelocity = 6.15f;
    }

    public override void PreUpdateMovement()
    {
        if (CanUseDash())
        {
            Vector2 newVelocity = Player.velocity;

            switch (_dashDir)
            {
                case DASH_LEFT when Player.velocity.X > -_dashVelocity:
                case DASH_RIGHT when Player.velocity.X < _dashVelocity:
                    {
                        float dashDirection = _dashDir == DASH_RIGHT ? 1 : -1;
                        newVelocity.X = dashDirection * _dashVelocity;
                        break;
                    }
                default:
                    return;
            }

            _dashDelay = _dashCooldown;
            _dashTimer = _dashDuration;
            Player.velocity = newVelocity;
            SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Player.position);
        }

        if (_dashDelay > 0)
            _dashDelay--;

        if (_dashTimer > 0)
        {
            Player.eocDash = _dashTimer;
            _dashTimer--;
        }
    }

    public bool CanUseDash()
    {
        return Player.dashType == 0
        && !Player.setSolar
        && _dashDir != -1
        && _dashDelay == 0
        && !Player.mount.Active;
    }
}