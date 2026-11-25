namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

class EarthenFuryTile(BossModule module) : Components.GenericAOEs(module, (uint)AID.EarthenFuryTile)
{
    private static readonly AOEShapeRect _shape = new(10, 10, 10);
    private readonly List<AOEInstance> _aoes = [];

    private readonly List<AOEInstance> _portedAOEView = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _portedAOEView.Clear();
        _portedAOEView.AddRange(_aoes);
        return CollectionsMarshal.AsSpan(_portedAOEView);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _aoes.Add(new(_shape, Ex3Shinryu.TileCenter(caster.Position + (caster.Position - Arena.Center).Normalized() * 10), default, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _aoes.Clear();
    }
}
