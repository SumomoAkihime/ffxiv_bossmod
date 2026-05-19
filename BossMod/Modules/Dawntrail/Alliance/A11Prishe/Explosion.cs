namespace BossMod.Dawntrail.Alliance.A11Prishe;

class Explosion(BossModule module) : Components.GenericAOEs(module, AID.Explosion)
{
    private static readonly AOEShapeCircle Circle = new(8f);
    private readonly List<AOEInstance> _aoes = new(28);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            yield break;

        var firstActivation = _aoes[0].Activation;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if ((aoe.Activation - firstActivation).TotalSeconds >= 1d)
                yield return aoe with { Risky = false };
            else
                yield return aoe;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Explosion)
            _aoes.Add(new(Circle, spell.LocXZ, default, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID == AID.Explosion)
            _aoes.RemoveAt(0);
    }
}
