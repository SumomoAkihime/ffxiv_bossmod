namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE203雪石膏之剑;

public enum OID : uint
{
    Boss = 0x4BBE, // R4.0
    Golem = 0x4BBF, // R1.65
    Helper = 0x233C
}

public enum AID : uint
{
    EmbrittlingBladeVisual = 47171, // Boss->self, 5.0s cast
    EmbrittlingBlade = 47172, // Helper->self, no cast, raidwide
    Summon = 47154, // Boss->self, 3.0s cast
    FourfoldAttackOrder = 47155, // Boss->self, 10.0s cast
    AttackOrder = 47156, // Boss->self, no cast
    AcclaimInitial = 47157, // Golem->self, 12.0s cast, range 40 90-degree cone
    Acclaim = 47158, // Golem->self, 3.0s cast, range 40 90-degree cone
    LightPrayer = 47159, // Boss->self, 3.0s cast
    FalseSpellbladeHolyVisual = 47757, // Boss->self, 32.0s cast
    FalseSpellbladeHoly = 47161, // Helper->self, no cast, raidwide
    OccultAero = 47163, // Helper->self, 5.0s cast, 50x10 rect
    OccultStoneII = 47164, // Helper->self, 5.0s cast, range 40 60-degree cone
    OccultTornado = 47165, // Helper->location, 5.0s cast, range 5 circle
    RightLeftCombination = 47166, // Boss->self, 5.0s cast, right then left
    LeftRightCombination = 47167, // Boss->self, 5.0s cast, left then right
    ClearoutAfterLeftRight = 47168, // Boss->self, no cast, second hit
    ClearoutAfterRightLeft = 47169, // Boss->self, no cast, second hit
    OccultAeroIII = 47170 // Helper->self, 5.0s cast, 50x10 rect
}

sealed class EmbrittlingBlade(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.EmbrittlingBladeVisual, (uint)AID.EmbrittlingBlade, 1.44d);
sealed class FalseSpellbladeHoly(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.FalseSpellbladeHolyVisual, (uint)AID.FalseSpellbladeHoly, 0.9d);

sealed class Acclaim(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone Cone = new(40f, 45f.Degrees());
    private readonly List<AOEInstance> _aoes = new(4);
    private readonly HashSet<ulong> _sequenceGolems = [];
    private readonly Dictionary<ulong, Angle> _rotationBeforeOrder = new(4);
    private DateTime _predictionReadyAt;
    private DateTime _predictionActivation;
    private bool _predictionPending;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void Update()
    {
        if (!_predictionPending || WorldState.CurrentTime < _predictionReadyAt)
            return;

        _predictionPending = false;
        _aoes.Clear();
        foreach (var (instanceID, startingRotation) in _rotationBeforeOrder)
        {
            if (WorldState.Actors.Find(instanceID) is not { } golem)
                continue;

            // Rotating golems have moved at least 5 degrees after 1.1s; stationary ones remain at 0.
            var rotation = startingRotation.DistanceToAngle(golem.Rotation).Deg < -1f
                ? startingRotation - 90f.Degrees()
                : startingRotation;
            SetAOE(golem, rotation, _predictionActivation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FourfoldAttackOrder)
        {
            _aoes.Clear();
            _sequenceGolems.Clear();
            _rotationBeforeOrder.Clear();
            _predictionPending = false;
        }
        else if (spell.Action.ID is (uint)AID.AcclaimInitial or (uint)AID.Acclaim)
        {
            if (spell.Action.ID == (uint)AID.AcclaimInitial)
                _sequenceGolems.Add(caster.InstanceID);
            else
                _predictionPending = false;
            SetAOE(caster, spell.Rotation, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AcclaimInitial or (uint)AID.Acclaim)
            _aoes.RemoveAll(a => a.ActorID == caster.InstanceID);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.AttackOrder)
            return;

        _aoes.Clear();
        _rotationBeforeOrder.Clear();
        foreach (var instanceID in _sequenceGolems)
        {
            if (WorldState.Actors.Find(instanceID) is { } golem)
                _rotationBeforeOrder[instanceID] = golem.Rotation;
        }
        _predictionReadyAt = WorldState.FutureTime(1.1d);
        _predictionActivation = WorldState.FutureTime(5.2d);
        _predictionPending = _rotationBeforeOrder.Count != 0;
    }

    private void SetAOE(Actor caster, Angle rotation, DateTime activation)
    {
        var aoe = new AOEInstance(Cone, caster.Position, rotation, activation, actorID: caster.InstanceID);
        var index = _aoes.FindIndex(a => a.ActorID == caster.InstanceID);
        if (index >= 0)
            _aoes[index] = aoe;
        else
            _aoes.Add(aoe);
    }
}

sealed class OccultAero(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.OccultAero, (uint)AID.OccultAeroIII], new AOEShapeRect(50f, 5f));
sealed class OccultStoneII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OccultStoneII, new AOEShapeCone(40f, 30f.Degrees()));
sealed class OccultTornado(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OccultTornado, 5f);

sealed class BladeCombination(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone Cone = new(40f, 90f.Degrees());
    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        for (var i = 0; i < aoes.Length; ++i)
        {
            aoes[i].Color = i == 0 ? Colors.Danger : Colors.AOE;
            aoes[i].Risky = i == 0;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is not (uint)AID.RightLeftCombination and not (uint)AID.LeftRightCombination)
            return;

        _aoes.Clear();
        var activation = Module.CastFinishAt(spell);
        _aoes.Add(new(Cone, caster.Position, spell.Rotation, activation));
        _aoes.Add(new(Cone, caster.Position, spell.Rotation + 180f.Degrees(), activation.AddSeconds(2.2d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RightLeftCombination or (uint)AID.LeftRightCombination
            or (uint)AID.ClearoutAfterLeftRight or (uint)AID.ClearoutAfterRightLeft)
        {
            if (_aoes.Count != 0)
                _aoes.RemoveAt(0);
            ++NumCasts;
        }
    }
}

sealed class CE203雪石膏之剑States : StateMachineBuilder
{
    public CE203雪石膏之剑States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EmbrittlingBlade>()
            .ActivateOnEnter<FalseSpellbladeHoly>()
            .ActivateOnEnter<Acclaim>()
            .ActivateOnEnter<OccultAero>()
            .ActivateOnEnter<OccultStoneII>()
            .ActivateOnEnter<OccultTornado>()
            .ActivateOnEnter<BladeCombination>();
    }
}

// Keep this contributed implementation while the upstream same-ID module is WIP.
[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(CE203雪石膏之剑States),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093,
    NameID = 14509)]
public sealed class CE203雪石膏之剑(WorldState ws, Actor primary) : BossModule(ws, primary, new(-519f, -641f), new ArenaBoundsCircle(25f));
