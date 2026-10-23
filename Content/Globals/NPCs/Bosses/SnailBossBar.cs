namespace ForgottenSouls.Content;

internal class SnailBossBar : ModBossBar
{
    public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) => false;
}