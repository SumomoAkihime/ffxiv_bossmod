namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class EastwindWheel(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect Rect = new(60, 9);
    private static readonly AOEShapeCone Cone = new(60, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        Angle offset = (AID)spell.Action.ID switch
        {
            AID.EastwindWheel2 => 45.Degrees(),
            AID.EastwindWheel1 => -45.Degrees(),
            _ => default
        };
        if (offset == default)
            return;

        var rot = spell.Rotation;
        var pos = spell.LocXZ;
        _aoes.Add(new(Rect, pos, rot, Module.CastFinishAt(spell, 0.8f)));
        _aoes.Add(new(Cone, pos, rot + offset, Module.CastFinishAt(spell, 2.8f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is not (AID.EastwindWheelRepeat or AID.EastwindWheelCone))
            return;

        if (++NumCasts > 3 && _aoes.Count > 0)
            _aoes.RemoveAt(0);

        if (_aoes.Count == 0)
            NumCasts = 0;
    }
}
