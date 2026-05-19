namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

class SublimeEstoc(BossModule module) : Components.GenericAOEs(module, AID.SublimeEstoc)
{
    private readonly List<(Actor, DateTime)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(new AOEShapeRect(40, 2.5f), c.Item1.Position, c.Item1.Rotation, c.Item2));

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.SublimeEstoc && id == 0x2488)
            _casters.Add((actor, WorldState.FutureTime(5.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _casters.RemoveAll(c => c.Item1 == caster);
        }
    }
}
