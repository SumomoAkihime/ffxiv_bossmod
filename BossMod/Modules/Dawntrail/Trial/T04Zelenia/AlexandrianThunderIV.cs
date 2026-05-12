namespace BossMod.Dawntrail.Trial.T04Zelenia;

class AlexandrianThunderIV(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Circle = new(8);
    private static readonly AOEShapeDonut Donut = new(8, 24);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return _aoes[0];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.AlexandrianThunderIVCircle1 or AID.AlexandrianThunderIVCircle2 => Circle,
            AID.AlexandrianThunderIVDonut1 or AID.AlexandrianThunderIVDonut2 => Donut,
            _ => null
        };

        if (shape != null)
            _aoes.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count == 0)
            return;

        switch ((AID)spell.Action.ID)
        {
            case AID.AlexandrianThunderIVCircle1:
            case AID.AlexandrianThunderIVCircle2:
            case AID.AlexandrianThunderIVDonut1:
            case AID.AlexandrianThunderIVDonut2:
                _aoes.RemoveAt(0);
                break;
        }
    }
}
