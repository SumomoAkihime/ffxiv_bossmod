namespace BossMod.Shadowbringers.Alliance.A14Engels;

class GuidedMissileBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Chase, centerAtTarget: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var baits = ActiveBaitsOn(actor);
        if (baits.Count > 0)
        {
            var bait = baits[0];
            hints.AddForbiddenZone(new AOEShapeRect(25, 25, 25), Arena.Center, activation: bait.Activation);
        }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GuidedMissileFirst)
            CurrentBaits.Clear();
    }
}
class GuidedMissile(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(6), (uint)AID.GuidedMissileFirst, (uint)AID.GuidedMissileRest, 6, 1, 4);
