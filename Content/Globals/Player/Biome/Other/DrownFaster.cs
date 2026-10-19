namespace ForgottenSouls.Content.Biome;

internal class DrownFaster : ModPlayer
{
    public override void PostUpdate()
    {
        if (Player.wet && !Player.lavaWet && !Player.honeyWet)
        {
            if (Player.breath > 0 && Main.GameUpdateCount % 8f == 0)
            {
                Player.breath--;
            }
        }
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        if (modifiers.DamageSource.SourceOtherIndex == 4)
        {
            
        }
    }
}