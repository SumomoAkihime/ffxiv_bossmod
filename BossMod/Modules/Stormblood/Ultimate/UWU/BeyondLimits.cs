namespace BossMod.Stormblood.Ultimate.UWU;

class BeyondLimits : Components.GenericAOEs
{
    private int _soaker = -1;
    private WPos? _position;
    private ulong _puddleID;
    private readonly int[] _soakOrder = Utils.MakeArray(3, -1);
    private readonly AOEInstance[] _aoe = new AOEInstance[1];
    private static readonly AOEShapeCircle Shape = new(1);

    public BeyondLimits(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
        foreach (var (slot, group) in Service.Config.Get<UWUConfig>().BeyondLimits.Resolve(Raid))
            if (group >= 0 && group < _soakOrder.Length && slot >= 0 && slot < PartyState.MaxPartySize)
                _soakOrder[group] = slot;
    }

    private bool IsActive => !WorldState.Actors.Any(a => a.CastInfo is { } ci
        && (ci.IsSpell(AID.FeatherRain) || ci.IsSpell(AID.WeightOfTheLandAOE)));

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!IsActive || _position is not { } position)
            return [];
        // Only the assigned player sees a safe circle. Their required inside
        // position is handled explicitly, so Check and AI agree about safety.
        _aoe[0] = new(Shape, position, color: slot == _soaker ? Colors.Safe : default, risky: slot != _soaker);
        return _aoe;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsActive && slot == _soaker && _position is { } position)
            hints.Add("Soak Beyond Limits!", !actor.Position.InCircle(position, 1));
        else
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (IsActive && slot == _soaker && _position is { } position)
            hints.AddForbiddenZone(new SDInvertedCircle(position, 1));
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id != 0x1E39 || actor.HPMP.CurHP != 1 || actor.FindStatus(SID.Woken) == null)
            return;
        var group = (OID)actor.OID switch { OID.Garuda => 0, OID.Ifrit => 1, OID.Titan => 2, _ => -1 };
        if (group >= 0)
        {
            _position = actor.Position;
            _soaker = _soakOrder[group];
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BeyondLimits)
        {
            _position = actor.Position;
            _puddleID = actor.InstanceID;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.InstanceID == _puddleID)
            Clear();
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if ((SID)status.ID == SID.BeyondLimits)
            Clear();
    }

    private void Clear()
    {
        _soaker = -1;
        _position = null;
        _puddleID = 0;
    }
}
