namespace ForgottenSouls.Content.Systems;

internal sealed class DisarmPlayer : ModPlayer
{
    public const float DisarmChance = 0.005f; // 0.5%
    public const int HotbarSlots = 10;
    public const int WeaponDamageThreshold = 15;

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
    {
        TryDisarm();
    }

    private void TryDisarm()
    {
        if (Player.whoAmI != Main.myPlayer || Main.rand.NextFloat() >= DisarmChance)
            return;

        List<int> slots = [];
        for (int s = 0; s < HotbarSlots; s++)
        {
            Item item = Player.inventory[s];
            if (s != Player.selectedItem && (item.IsAir || item.damage <= WeaponDamageThreshold))
                slots.Add(s);
        }

        if (slots.Count == 0)
            return;

        Player.selectedItem = slots[Main.rand.Next(slots.Count)];

        Player.itemAnimation = 0;
        Player.itemTime = 0;

        Main.NewText("Оружие выбито!", Color.OrangeRed);
        SoundEngine.PlaySound(SoundID.Item7, Player.Center);
    }
}