namespace NoCompromises.Content.Global;

internal class BossDeathStrike : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
    {
        if (npc.boss && Main.rand.NextBool(100))
        {
            int fatalDamage = target.statLife + target.statDefense * 2;
            target.Hurt(PlayerDeathReason.ByCustomReason($"{target.name} был уничтожен мощью босса {npc.FullName}!"), fatalDamage, 0);

            SoundEngine.PlaySound(SoundID.NPCDeath6, target.Center);
        }
    }
}