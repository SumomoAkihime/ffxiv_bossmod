namespace BossMod.Stormblood.Ultimate.UWU;

class P4Freefire(BossModule module) : Components.GenericAOEs(module, (uint)AID.FreefireIntermission)
{
    private readonly List<Actor> _casters = [];
    private DateTime _activation;

    private static readonly AOEShape _shape = new AOEShapeCircle(17); // TODO: verify falloff

    private readonly List<AOEInstance> _portedAOEView = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _portedAOEView.Clear();
        _portedAOEView.AddRange(_casters.Select(c => new AOEInstance(_shape, c.Position, 0.Degrees(), _activation)));
        return CollectionsMarshal.AsSpan(_portedAOEView);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Helper && id == 0x0449)
        {
            _casters.Add(actor);
            _activation = WorldState.FutureTime(5.9f);
        }
    }
}
