namespace BossMod.Dawntrail.Trial.T02ZoraalJa;

class DoubleEdgedSwords(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone Cone = new(30, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            yield break;

        // first cleave is the immediate danger; keep second visible but non-risky
        var first = _aoes[0];
        first.Risky = true;
        yield return first;
        for (var i = 1; i < _aoes.Count; ++i)
            yield return _aoes[i];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DoubleEdgedSwords)
            _aoes.Add(new(Cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), _aoes.Count == 0 ? ArenaColor.Danger : default, false));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.DoubleEdgedSwords)
            _aoes.RemoveAt(0);
    }
}
