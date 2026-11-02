namespace ForgottenSouls.Content.NPCs;

internal sealed class HallowCamouflage : GlobalNPC
{
    private const float CycleDuration = 6f;

    public override void DrawEffects(NPC npc, ref Color drawColor)
    {
        if (Main.gameMenu || !Main.LocalPlayer.ZoneHallow)
            return;

        if (!IsHallowEnemy(npc))
            return;

        float time = Main.GlobalTimeWrappedHourly % CycleDuration;

        // 65% — маскировка
        if (time < 3.9f)
        {
            float pulse = (float)System.Math.Sin(time * 2.5f) * 0.04f;

            ApplyAlpha(ref drawColor, 0.32f + pulse);
        }
        // 25% — перенасыщение цветов
        else if (time < 5.4f)
        {
            float progress = (time - 3.9f) / 1.5f;

            Color vividColor = Color.Lerp(
                new Color(255, 70, 220),
                new Color(70, 240, 255),
                (float)System.Math.Sin(progress * MathHelper.Pi) * 0.5f + 0.5f
            );

            drawColor = Color.Lerp(drawColor, vividColor, 0.8f);
        }
        // 10% — мерцание
        else
        {
            float progress = (time - 5.4f) / 0.6f;

            float flicker = (float)System.Math.Sin(progress * 35f);

            float alpha = flicker > 0f ? 0.15f : 0.65f;

            ApplyAlpha(ref drawColor, alpha);

            drawColor = Color.Lerp(
                drawColor,
                Color.White,
                0.35f
            );
        }
    }

    private static void ApplyAlpha(ref Color color, float opacity)
    {
        byte alpha = (byte)MathHelper.Clamp(
            color.A * opacity,
            0f,
            255f
        );

        color = new Color(
            color.R,
            color.G,
            color.B,
            alpha
        );
    }

    private static bool IsHallowEnemy(NPC npc)
    {
        if (!npc.active || npc.friendly || npc.townNPC || npc.boss)
            return false;

        switch (npc.type)
        {
            case NPCID.Pixie:
            case NPCID.Unicorn:
            case NPCID.Gastropod:
            case NPCID.ChaosElemental:
            case NPCID.IlluminantBat:
            case NPCID.IlluminantSlime:
            case NPCID.EnchantedSword:
            case NPCID.BigMimicHallow:
                return true;

            default:
                return false;
        }
    }
}
