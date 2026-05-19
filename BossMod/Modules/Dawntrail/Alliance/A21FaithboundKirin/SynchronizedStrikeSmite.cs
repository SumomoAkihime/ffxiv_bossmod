namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class SynchronizedStrikeSmite(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeRect Narrow = new(60, 5);
    private static readonly AOEShapeRect Wide = new(60, 16);
    private Actor? _leftArm;
    private Actor? _rightArm;
    private int _visibleCount;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (int i = 0; i < _visibleCount && i < _aoes.Count; ++i)
            yield return _aoes[i];
    }

    public override void Update()
    {
        int count = _aoes.Count;
        if (count < 3)
        {
            _visibleCount = count;
            return;
        }

        var first = _aoes[0];
        var third = _aoes[2];
        _visibleCount = third.Rotation == first.Rotation ? 2 : count;
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.SculptedArm2)
            _leftArm = actor;
        else if ((OID)actor.OID == OID.SculptedArm1)
            _rightArm = actor;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AID aid = (AID)spell.Action.ID;
        AOEShapeRect? shape = aid switch
        {
            AID.SynchronizedStrikeSlow or AID.SynchronizedSequence or AID.SynchronizedStrikeTelegraph => Narrow,
            AID.SynchronizedSmiteRightSlow or AID.SynchronizedSmiteLeftSlow or AID.SynchronizedStrikeFast => Wide,
            _ => null
        };
        if (shape == null)
            return;

        bool fastPrediction = aid == AID.SynchronizedStrikeFast;
        if (!fastPrediction)
        {
            if (_aoes.Count > 1 && _aoes[0].Shape == shape && _aoes[1].Shape == shape)
                return;

            var delay = aid == AID.SynchronizedStrikeTelegraph ? 7.1f : 0;
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, delay)));
            return;
        }

        if (_aoes.Count > 2)
            return;
        AddArmPrediction(_leftArm);
        AddArmPrediction(_rightArm);

        void AddArmPrediction(Actor? arm)
        {
            if (arm == null)
                return;
            var pos = arm.Position - 30 * arm.Rotation.ToDirection();
            _aoes.Add(new(Wide, pos, arm.Rotation, Module.CastFinishAt(spell, 5.1f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count == 0)
            return;
        if ((AID)spell.Action.ID is AID.SynchronizedStrikeSlow or AID.SynchronizedStrikeFast or AID.SynchronizedSmiteRightSlow or AID.SynchronizedSmiteLeftSlow or AID.SynchronizedSmite1Fast or AID.SynchronizedSmite2Fast or AID.SynchronizedSequence)
            _aoes.RemoveAt(0);
    }
}
