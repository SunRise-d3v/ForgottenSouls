using NoCompromises.Content.Buffs;

namespace NoCompromises.Content.Globals.NPCs;

public class DiscountBuffShop : GlobalNPC
{
    public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
    {
        Player player = Main.LocalPlayer;

        float discountMultiplier = player switch
        {
            _ when player.HasBuff(ModContent.BuffType<HeroVillage4>()) => 0.80f, // ARank
            _ when player.HasBuff(ModContent.BuffType<HeroVillage3>()) => 0.87f, // BRank
            _ when player.HasBuff(ModContent.BuffType<HeroVillage2>()) => 0.92f, // CRank
            _ when player.HasBuff(ModContent.BuffType<HeroVillage1>()) => 0.95f, // FRank
            _ => 1.0f
        };

        if (discountMultiplier < 1.0f)
        {
            foreach (Item item in items)
            {
                if (item == null || item.IsAir)
                    continue;

                int currentPrice = item.shopCustomPrice ?? item.value;
                item.shopCustomPrice = (int)(currentPrice * discountMultiplier);
            }
        }
    }
}