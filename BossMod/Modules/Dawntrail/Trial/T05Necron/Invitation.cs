namespace BossMod.Dawntrail.Trial.T05Necron;

sealed class Invitation(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeRect rect = new(36f, 5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.LoomingSpecter1)
        {
            _aoes.Add(new(rect, source.Position, source.Rotation, WorldState.FutureTime(12.3f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.Invitation)
        {
            _aoes.RemoveAt(0);
        }
    }
}

