namespace ForgottenSouls.Content.Biome;

internal class FreezeInSnow : ModPlayer
{
    private int freezeTimer = 0;
    private int freezeCooldown = 0;

    public override void PostUpdate()
    {
        if (Player.ZoneSnow)
        {
            freezeCooldown--;

            if (freezeCooldown <= 0)
            {
                freezeTimer = 60;
                freezeCooldown = Main.rand.Next(1000, 1501);
            }

            Main.buffNoTimeDisplay[BuffID.Frozen] = true;
        }
        else
        {
            freezeCooldown = 0;
            freezeTimer = 0;

            Main.buffNoTimeDisplay[BuffID.Frozen] = false;
        }

        if (freezeTimer > 0)
        {
            freezeTimer--;

            Player.velocity = Vector2.Zero;
            Player.controlLeft = false;
            Player.controlRight = false;
            Player.controlJump = false;
            Player.controlDown = false;
            Player.controlUp = false;
            Player.controlUseItem = false;
            Player.controlUseTile = false;
            Player.controlHook = false;
            Player.controlMount = false;
            Player.controlQuickHeal = false;
            Player.controlQuickMana = false;

            Player.AddBuff(BuffID.Frozen, 2);
        }
    }
}