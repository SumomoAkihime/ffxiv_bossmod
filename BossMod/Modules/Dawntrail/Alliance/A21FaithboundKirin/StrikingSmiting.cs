namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class StrikingSmiting(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCircle Small = new(10);
    private static readonly AOEShapeCircle Big = new(30);
    private Actor? _leftArm;
    private Actor? _rightArm;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.SculptedArm2)
            _leftArm = actor;
        else if ((OID)actor.OID == OID.SculptedArm1)
            _rightArm = actor;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count == 2)
            return;

        switch ((AID)spell.Action.ID)
        {
            case AID.StrikingLeftBoss:
            case AID.SmitingLeftSequence:
                AddPair(_leftArm);
                break;
            case AID.StrikingRightBoss:
            case AID.SmitingRightSequence:
                AddPair(_rightArm);
                break;
            case AID.StrikingLeftTelegraph:
                AddPair(_leftArm, 7);
                break;
            case AID.StrikingRightTelegraph:
                AddPair(_rightArm, 7);
                break;
        }

        void AddPair(Actor? arm, float delayFirst = 0)
        {
            _aoes.Add(new(Small, spell.LocXZ, Activation: Module.CastFinishAt(spell, delayFirst)));
            _aoes.Add(new(Big, arm?.Position ?? spell.LocXZ, Activation: Module.CastFinishAt(spell, 5.1f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count == 0)
            return;
        if ((AID)spell.Action.ID is AID.StrikingLeftBoss or AID.StrikingLeftFast or AID.StrikingRightBoss or AID.StrikingRightFast or AID.SmitingLeftSlow or AID.SmitingLeftFast or AID.SmitingRightSlow or AID.SmitingRightFast or AID.SmitingLeftSequence or AID.SmitingRightSequence)
            _aoes.RemoveAt(0);
    }
}
