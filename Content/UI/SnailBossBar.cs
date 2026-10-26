namespace ForgottenSouls.Content.UI;

internal sealed class SnailBossBar : ModBossBar
{
    public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) => false;
}