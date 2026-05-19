namespace BossMod.Dawntrail.Alliance.A22OmegaTheOne;

class OmegaBlaster(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCone _cone = new(50, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.OmegaBlasterFirst or AID.OmegaBlasterSecond)
        {
            _aoes.Add(new(_cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (_aoes.Count > 1)
            {
                if (_aoes[0].Activation > _aoes[1].Activation)
                    (_aoes[0], _aoes[1]) = (_aoes[1], _aoes[0]);
                var second = _aoes[1];
                second.Origin += 5 * second.Rotation.ToDirection();
                _aoes[1] = second;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.OmegaBlasterFirst or AID.OmegaBlasterSecond)
        {
            _aoes.RemoveAt(0);
            NumCasts++;
            if (_aoes.Count == 1)
            {
                var aoe = _aoes[0];
                aoe.Origin -= 5 * aoe.Rotation.ToDirection();
                _aoes[0] = aoe;
            }
        }
    }
}
