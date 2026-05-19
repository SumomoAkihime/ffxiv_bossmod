namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class Wringer(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCircle Circle = new(14);
    private static readonly AOEShapeDonut Donut = new(14, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return _aoes[0];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AID aid = (AID)spell.Action.ID;
        AOEShape? shape = aid switch
        {
            AID.WringerSlow or AID.WringerTelegraph or AID.DoubleWringer => Circle,
            AID.DeadWringerSlow => Donut,
            _ => null
        };
        if (shape == null)
            return;
        if (_aoes.Count > 0 && _aoes[0].Shape == shape)
            return;

        _aoes.Add(new(shape, spell.LocXZ, Activation: Module.CastFinishAt(spell, aid == AID.WringerTelegraph ? 6.5f : 0)));
        if (aid == AID.WringerTelegraph)
            _aoes.Add(new(Donut, spell.LocXZ, Activation: Module.CastFinishAt(spell, 11.5f)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count == 0)
            return;
        if ((AID)spell.Action.ID is AID.DeadWringerSlow or AID.DeadWringerFast or AID.WringerSlow or AID.WringerFast or AID.DoubleWringer)
            _aoes.RemoveAt(0);
    }
}
