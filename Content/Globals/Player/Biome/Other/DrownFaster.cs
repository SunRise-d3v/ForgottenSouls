using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace NoCompromises.Content.Globals;

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