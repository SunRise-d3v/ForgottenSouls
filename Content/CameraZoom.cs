namespace ForgottenSouls.Content.Systems;

internal sealed class CameraZoom : ModSystem
{
    public const float MinZoom = 0.93f;
    public const float MaxZoom = 2f;

    private const float ZoomSpeed = 0.011f;

    private static float targetZoom = 1f;
    private static float currentZoom = 1f;

    internal static void Adjust(float delta)
    {
        targetZoom = MathHelper.Clamp(targetZoom + delta, MinZoom, MaxZoom);
    }

    internal static void Reset()
    {
        targetZoom = 1f;
    }

    public override void Unload()
    {
        targetZoom = 1f;
        currentZoom = 1f;
    }

    public override void ModifyTransformMatrix(ref SpriteViewMatrix transform)
    {
        currentZoom = MathHelper.Clamp(
            currentZoom + Math.Sign(targetZoom - currentZoom) * ZoomSpeed,
            MinZoom,
            MaxZoom
        );

        if (Math.Abs(currentZoom - targetZoom) <= ZoomSpeed)
            currentZoom = targetZoom;

        transform.Zoom *= currentZoom;
    }
}

internal sealed class CameraZoomPlayer : ModPlayer
{
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (Player.whoAmI != Main.myPlayer)
            return;

        if (triggersSet.ViewZoomIn)
        {
            CameraZoom.Adjust(
                PlayerInput.Triggers.JustPressed.ViewZoomIn ? 0.05f : 0.012f
            );
        }

        if (triggersSet.ViewZoomOut)
        {
            CameraZoom.Adjust(
                PlayerInput.Triggers.JustPressed.ViewZoomOut ? -0.05f : -0.012f
            );
        }

        if (Main.keyState.IsKeyDown(Keys.LeftControl)
            && PlayerInput.ScrollWheelDelta != 0)
        {
            CameraZoom.Adjust(
                Math.Sign(PlayerInput.ScrollWheelDelta) * 0.1f
            );

            PlayerInput.ScrollWheelDelta = 0;
        }
    }
}