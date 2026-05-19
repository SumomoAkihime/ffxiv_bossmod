namespace BossMod.Dawntrail.Alliance.A22OmegaTheOne;

class SurfaceMissile(BossModule module) : Components.GenericAOEs(module, AID.SurfaceMissile)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeRect _rect = new(12, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(4);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.SurfaceMissile)
        {
            var delay = _aoes.Count switch
            {
                < 4 => 10.1f,
                < 8 => 9.2f,
                _ => 8.2f
            };

            var rot = actor.Rotation;
            // Reborn uses Round/Quantized helpers; this fork omits them for compatibility.
            var origin = actor.Position - 6 * rot.ToDirection();
            _aoes.Add(new(_rect, origin, rot, WorldState.FutureTime(delay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && _aoes.Count > 0)
        {
            _aoes.RemoveAt(0);
            NumCasts++;
        }
    }
}
