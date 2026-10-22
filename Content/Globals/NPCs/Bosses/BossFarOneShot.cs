namespace ForgottenSouls.Content.Globals;

internal class BossFarOneShot : GlobalProjectile
{
    private const float FarDistance = 100f * 16f;

    private int _bossIndex = -1;

    public override bool InstancePerEntity => true;

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (source is EntitySource_Parent { Entity: NPC npc } && IsBoss(npc))
            _bossIndex = npc.whoAmI;
    }

    public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
    {
        if (_bossIndex < 0 || target.dead)
            return;

        NPC boss = Main.npc[_bossIndex];

        if (!boss.active || !IsBoss(boss))
            return;

        if (boss.Distance(target.Center) <= FarDistance)
            return;

        //SoundEngine.PlaySound(SoundID.Roar);

        target.KillMe(
            PlayerDeathReason.ByNPC(_bossIndex),
            target.statLifeMax2 + 100,
            info.HitDirection);
    }

    private static bool IsBoss(NPC npc) => npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type];
}