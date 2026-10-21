namespace ForgottenSouls.Content.Biome;

internal sealed class DrownFaster : ModPlayer
{
    public override void PostUpdate()
    {
        if (Player.wet && !Player.lavaWet && !Player.honeyWet)
        {
            if (Player.breath > 0 && Main.GameUpdateCount % 7f == 0)
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