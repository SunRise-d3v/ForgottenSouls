namespace ForgottenSouls.Content.NPCs;

internal sealed class SniperOneShot : GlobalProjectile
{
    private bool _fromSniper;

    public override bool InstancePerEntity => true;

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (source is EntitySource_Parent { Entity: NPC npc } && npc.type == NPCID.SkeletonSniper)
            _fromSniper = true;
    }

    public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
    {
        if (!Sadomasochist.Active)
            return;

        if (!_fromSniper || target.dead)
            return;

        target.KillMe(
            PlayerDeathReason.ByProjectile(-1, projectile.whoAmI),
            target.statLifeMax2 + 100,
            info.HitDirection);
    }
}