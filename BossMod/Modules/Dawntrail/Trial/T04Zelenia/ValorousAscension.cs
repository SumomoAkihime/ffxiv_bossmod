namespace BossMod.Dawntrail.Trial.T04Zelenia;

class ValorousAscension(BossModule module) : Components.RaidwideCast(module, AID.ValorousAscension1);

class ValorousAscensionRect(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect Rect = new(40, 4);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Math.Min(_aoes.Count, 2);
        for (var i = 0; i < count; ++i)
            yield return _aoes[i];
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11DB && actor.OID == (uint)OID.BriarThorn1)
            _aoes.Add(new(Rect, actor.Position, actor.Rotation, WorldState.FutureTime(11.1f)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && (AID)spell.Action.ID == AID.ValorousAscensionRect)
            _aoes.RemoveAt(0);
    }
}
