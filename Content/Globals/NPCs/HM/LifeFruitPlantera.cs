namespace ForgottenSouls.Content;

internal sealed class LifeFruitPlantera : GlobalItem
{
    private const float Chance = 0.25f;

    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => entity.type == ItemID.LifeFruit;

    public override void OnConsumeItem(Item item, Player player)
    {
        if (Main.rand.NextFloat() >= Chance)
            return;

        if (NPC.AnyNPCs(NPCID.Plantera))
            return;

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: NPCID.Plantera);
        }
        else
        {
            NPC.SpawnOnPlayer(player.whoAmI, NPCID.Plantera);
        }
    }
}