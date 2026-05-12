namespace BossMod.Dawntrail.Trial.T04Zelenia;

class ShockSpread(BossModule module) : Components.GenericStackSpread(module, true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Shock)
            Spreads.Add(new(actor, 4, WorldState.FutureTime(8)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ShockLock)
            Spreads.Clear();
    }
}

class ShockAOE(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Circle = new(4);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var aid = (AID)spell.Action.ID;
        if (aid == AID.ShockLock)
        {
            _aoes.Add(new(Circle, caster.Position));
        }
        else if (aid == AID.Shock6 && ++NumCasts == 2 * _aoes.Count)
        {
            _aoes.Clear();
            NumCasts = 0;
        }
    }
}

class StockBreak(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StockBreak)
            Stacks.Add(new(actor, 6, 8, 8, WorldState.FutureTime(7.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.StockBreak4)
            Stacks.Clear();
    }
}
