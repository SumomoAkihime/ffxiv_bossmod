namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class MoontideFont(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.MoontideFontFast, (uint)AID.MoontideFontSlow], new AOEShapeCircle(9));

sealed class MidwinterMarchNorthernCurrent(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(12), new AOEShapeDonut(12, 60)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MidwinterMarch)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count == 0)
            return;

        int order = (AID)spell.Action.ID switch
        {
            AID.MidwinterMarch => 0,
            AID.NorthernCurrent => 1,
            _ => -1
        };
        if (order >= 0)
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(5));
    }
}
