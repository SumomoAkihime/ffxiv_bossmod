namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class RoyalBanishment(BossModule module) : Components.GenericWildCharge(module, 5f, default, 60f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.RoyalBanishmentFirst)
        {
            var bossp2 = Module.Enemies(OID.BossP2);
            Source = bossp2.Count != 0 ? bossp2[0] : null;
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = p.InstanceID == targetID ? PlayerRole.Target : PlayerRole.Share;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RoyalBanishmentAOE or AID.RoyalBanishmentLast)
            ++NumCasts;
    }
}
