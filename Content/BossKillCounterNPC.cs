namespace ForgottenSouls.Content;

internal class BossKillCounterNPC : GlobalNPC
{
    public override void OnKill(NPC npc)
    {
        if (npc.type == NPCID.EyeofCthulhu)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];

                if (player.active && !player.dead && npc.playerInteraction[i])
                {
                    var modPlayer = player.GetModPlayer<BossKillCounterPlayer>();

                    modPlayer.EyeOfCthulhuKills++;

                    if (Main.netMode == NetmodeID.SinglePlayer || Main.myPlayer == i)
                    {
                        Main.NewText($"Вы убили Глаза Ктулху {modPlayer.EyeOfCthulhuKills} раз!");

                        if (modPlayer.EyeOfCthulhuKills >= 2)
                        {
                            //Main.NewText("Молодец!");
                        }
                    }
                }
            }
        }
    }
}