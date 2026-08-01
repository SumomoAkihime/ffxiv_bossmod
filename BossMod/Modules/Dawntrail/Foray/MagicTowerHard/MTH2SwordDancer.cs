namespace BossMod.Dawntrail.Foray.MagicTowerHard.MTH2SwordDancer;

public enum OID : uint
{
    Boss = 0x4D7E, // R6.0
    MovingSword = 0x4D7F,
    SpinningSword = 0x4D81,
    Sword = 0x4D82,
    RushSword = 0x4D84,
    Helper = 0x233C
}

public enum AID : uint
{
    ThrowingSwords = 49619,
    RushShort = 49621, // MovingSword->location, width 7 charge
    RushLong = 49622, // MovingSword->location, width 7 charge
    RushShortAlt = 50527, // MovingSword->location, width 7 charge
    RushLongAlt = 50528, // MovingSword->location, width 7 charge
    TurnInner = 49635, // Helper->self, range 9-14 90-degree cone
    TurnMiddle = 49636, // Helper->self, range 14-19 90-degree cone
    TurnOuter = 49637, // Helper->self, range 19-24 90-degree cone
    TurnMiddleNarrow = 50063, // Helper->self, range 14-19 57-degree cone
    TurnOuterNarrow = 50064, // Helper->self, range 19-24 54-degree cone
    MartialMystique = 49645, // Helper->self, 48x96 rect
    CycloswordsUnsheathed = 49646,
    Cycloswords = 49647,
    SpinDonut10 = 49648, // SpinningSword->self, range 10-60 donut
    SpinDonut15 = 49649, // SpinningSword->self, range 15-60 donut
    SpinDonut20 = 49650, // SpinningSword->self, range 20-60 donut
    SpinCircle10 = 49651, // SpinningSword->self, range 10 circle
    SpinCircle15 = 49652, // SpinningSword->self, range 15 circle
    SpinCircle20 = 49653, // SpinningSword->self, range 20 circle
    LeapingLift = 49654,
    Pierce = 49655, // Sword->self, range 5 circle
    LeapingLiftJump = 49657,
    LeapingLiftJumpLast = 49659,
    SteelsbreathVisual = 49660,
    Steelsforge = 49661, // Helper->self, range 13 circle
    SwordDanceVisual = 49667,
    SwordDance = 49672, // Helper->self, 60x20 rect
    Surgeswords = 49674, // RushSword->self, 30x6 rect
    SwordStorm = 49675, // Boss->self, raidwide
    Steelsbreath = 50360 // Helper->self, knockback 26
}

public enum SID : uint
{
    LeapingLiftIndicator = 2056
}

sealed class SwordStorm(BossModule module) : Components.RaidwideCast(module, (uint)AID.SwordStorm);
sealed class RushShort(BossModule module) : Components.ChargeAOEs(module, (uint)AID.RushShort, 3.5f);
sealed class RushLong(BossModule module) : Components.ChargeAOEs(module, (uint)AID.RushLong, 3.5f);
sealed class RushShortAlt(BossModule module) : Components.ChargeAOEs(module, (uint)AID.RushShortAlt, 3.5f);
sealed class RushLongAlt(BossModule module) : Components.ChargeAOEs(module, (uint)AID.RushLongAlt, 3.5f);
sealed class TurnInner(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TurnInner, new AOEShapeDonutSector(9f, 14f, 45f.Degrees()));
sealed class TurnMiddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TurnMiddle, new AOEShapeDonutSector(14f, 19f, 45f.Degrees()));
sealed class TurnOuter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TurnOuter, new AOEShapeDonutSector(19f, 24f, 45f.Degrees()));
sealed class TurnMiddleNarrow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TurnMiddleNarrow, new AOEShapeDonutSector(14f, 19f, 28.5f.Degrees()));
sealed class TurnOuterNarrow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TurnOuterNarrow, new AOEShapeDonutSector(19f, 24f, 27f.Degrees()));
sealed class MartialMystique(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MartialMystique, new AOEShapeRect(48f, 48f));
sealed class SwordDance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SwordDance, new AOEShapeRect(60f, 10f))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Color = Colors.Danger;
        return base.ActiveAOEs(slot, actor);
    }
}
sealed class Pierce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pierce, 5f);

static class LeapingLiftTiming
{
    public static readonly double[] KnockbackOffsets = [0d, 5.05d, 7.55d, 12.55d, 15d];
    public const double FirstKnockbackDelay = 12.5d;
    public const double ForgeAdvance = 3.48d;
}

sealed class SpinningSword(BossModule module) : Components.GenericAOEs(module)
{
    private sealed class Prediction(AOEShape initial, AOEShape followup, WPos origin)
    {
        public readonly AOEShape Initial = initial;
        public readonly AOEShape Followup = followup;
        public WPos Origin = origin;
        public DateTime Timeline;
    }

    private const float FutureSafeMarkerRadius = 1.2f;
    private static readonly uint FutureSafeMarkerColor = Color.FromComponents(64, 192, 255).ABGR;
    private static readonly WDir[] MarginChecks = [default, new(0.75f, 0f), new(-0.75f, 0f), new(0f, 0.75f), new(0f, -0.75f)];
    private static readonly AOEShapeDonut Donut10 = new(10f, 60f);
    private static readonly AOEShapeDonut Donut15 = new(15f, 60f);
    private static readonly AOEShapeDonut Donut20 = new(20f, 60f);
    private static readonly AOEShapeCircle Circle10 = new(10f);
    private static readonly AOEShapeCircle Circle15 = new(15f);
    private static readonly AOEShapeCircle Circle20 = new(20f);
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<AOEInstance> _currentSafeAOEs = [];
    private readonly List<AOEInstance> _futureAOEs = [];
    private readonly HashSet<ulong> _activeActors = [];
    private readonly Dictionary<ulong, int> _completedCasts = [];
    private readonly Dictionary<ulong, Prediction> _predictions = [];
    private int _groupCompletedCasts;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        BuildSafeStages();
        if (_currentSafeAOEs.Count != 0 && FindSafe(pc.Position, _currentSafeAOEs) is WPos current)
            Arena.AddLine(pc.Position, current, Colors.Safe, 2f);
        if (_futureAOEs.Count != 0 && FindSafe(pc.Position, _futureAOEs) is WPos future)
            Arena.ZoneCircleOutline(future, FutureSafeMarkerRadius, FutureSafeMarkerColor, 2f);
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID != (uint)OID.SpinningSword)
            return;

        var shape = ShapeForModel(actor.InstanceID, modelState);
        _aoes.RemoveAll(aoe => aoe.ActorID == actor.InstanceID);
        if (shape == null)
            return;

        _activeActors.Add(actor.InstanceID);
        var delay = _completedCasts.GetValueOrDefault(actor.InstanceID) == 0 ? 15.3d : 0.3d;
        _aoes.Add(new(shape, actor.Position, actor.Rotation, WorldState.FutureTime(delay), actorID: actor.InstanceID));
        if (_completedCasts.GetValueOrDefault(actor.InstanceID) == 0 && !_predictions.ContainsKey(actor.InstanceID))
            _predictions.Add(actor.InstanceID, new(shape, Opposite(shape), actor.Position));
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID != (uint)OID.SpinningSword)
            return;

        if (id == 9710 && _completedCasts.GetValueOrDefault(actor.InstanceID) == 0)
        {
            Update(actor, WorldState.FutureTime(11.3d));
            if (_predictions.TryGetValue(actor.InstanceID, out var prediction))
            {
                prediction.Origin = actor.Position;
                prediction.Timeline = WorldState.CurrentTime;
            }
        }
        else if (id == 7740)
        {
            _aoes.RemoveAll(aoe => aoe.ActorID == actor.InstanceID);
            _activeActors.Remove(actor.InstanceID);
            _completedCasts.Remove(actor.InstanceID);
            if (_activeActors.Count == 0)
            {
                _predictions.Clear();
                _groupCompletedCasts = 0;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action.ID);
        if (shape == null)
            return;

        _activeActors.Add(caster.InstanceID);
        var index = _aoes.FindIndex(aoe => aoe.ActorID == caster.InstanceID);
        var aoe = new AOEInstance(shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID);
        if (index >= 0)
            _aoes[index] = aoe;
        else
            _aoes.Add(aoe);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (ShapeForAction(spell.Action.ID) == null)
            return;

        _aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
        _completedCasts[caster.InstanceID] = _completedCasts.GetValueOrDefault(caster.InstanceID) + 1;
        ++_groupCompletedCasts;
        ++NumCasts;
    }

    private AOEShape? ShapeForModel(ulong actorID, byte modelState) => modelState switch
    {
        0 when _activeActors.Contains(actorID) => Donut10,
        4 => Donut15,
        5 => Donut20,
        6 => Circle10,
        7 => Circle15,
        31 => Circle20,
        _ => null
    };

    private static AOEShape? ShapeForAction(uint aid) => aid switch
    {
        (uint)AID.SpinDonut10 => Donut10,
        (uint)AID.SpinDonut15 => Donut15,
        (uint)AID.SpinDonut20 => Donut20,
        (uint)AID.SpinCircle10 => Circle10,
        (uint)AID.SpinCircle15 => Circle15,
        (uint)AID.SpinCircle20 => Circle20,
        _ => null
    };

    private static AOEShape Opposite(AOEShape shape) =>
        ReferenceEquals(shape, Donut10) ? Circle10 :
        ReferenceEquals(shape, Donut15) ? Circle15 :
        ReferenceEquals(shape, Donut20) ? Circle20 :
        ReferenceEquals(shape, Circle10) ? Donut10 :
        ReferenceEquals(shape, Circle15) ? Donut15 :
        ReferenceEquals(shape, Circle20) ? Donut20 :
        throw new ArgumentOutOfRangeException(nameof(shape));

    private void Update(Actor actor, DateTime activation)
    {
        var index = _aoes.FindIndex(aoe => aoe.ActorID == actor.InstanceID);
        if (index < 0)
            return;

        var aoe = _aoes[index];
        aoe.Origin = actor.Position;
        aoe.Rotation = actor.Rotation;
        aoe.Activation = activation;
        _aoes[index] = aoe;
    }

    private void BuildSafeStages()
    {
        _currentSafeAOEs.Clear();
        _futureAOEs.Clear();
        if (_predictions.Count != 3 || _predictions.Values.Any(p => p.Timeline == default))
            return;

        var ordered = _predictions.Values.OrderBy(p => p.Timeline).ToList();
        var simultaneous = (ordered[^1].Timeline - ordered[0].Timeline).TotalSeconds < 0.6d;
        if (simultaneous)
        {
            if (_groupCompletedCasts == 0)
            {
                foreach (var prediction in ordered)
                {
                    _currentSafeAOEs.Add(new(prediction.Initial, prediction.Origin));
                    _futureAOEs.Add(new(prediction.Followup, prediction.Origin));
                }
            }
            else if (_groupCompletedCasts < 6)
            {
                foreach (var prediction in ordered)
                    _currentSafeAOEs.Add(new(prediction.Followup, prediction.Origin));
            }
        }
        else if (_groupCompletedCasts == 0)
        {
            _currentSafeAOEs.Add(new(ordered[0].Initial, ordered[0].Origin));
            _futureAOEs.Add(new(ordered[0].Followup, ordered[0].Origin));
            _futureAOEs.Add(new(ordered[1].Initial, ordered[1].Origin));
        }
        else if (_groupCompletedCasts < 3)
        {
            _currentSafeAOEs.Add(new(ordered[0].Followup, ordered[0].Origin));
            _currentSafeAOEs.Add(new(ordered[1].Initial, ordered[1].Origin));
            _futureAOEs.Add(new(ordered[1].Followup, ordered[1].Origin));
            _futureAOEs.Add(new(ordered[2].Initial, ordered[2].Origin));
        }
        else if (_groupCompletedCasts < 5)
        {
            _currentSafeAOEs.Add(new(ordered[1].Followup, ordered[1].Origin));
            _currentSafeAOEs.Add(new(ordered[2].Initial, ordered[2].Origin));
        }
    }

    private WPos? FindSafe(WPos from, List<AOEInstance> aoes)
    {
        WPos? best = null;
        var bestDistance = float.MaxValue;
        for (var x = -24f; x <= 24f; x += 1f)
        {
            for (var z = -24f; z <= 24f; z += 1f)
            {
                var candidate = new WPos(MTH2SwordDancer.ArenaCenter.X + x, MTH2SwordDancer.ArenaCenter.Z + z);
                if (!Safe(candidate, aoes))
                    continue;

                var distance = (candidate - from).LengthSq();
                if (distance < bestDistance)
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }
        }
        return best;
    }

    private bool Safe(WPos position, List<AOEInstance> aoes)
    {
        foreach (var offset in MarginChecks)
        {
            var sample = position + offset;
            if (!Arena.InBounds(sample))
                return false;
            for (var i = 0; i < aoes.Count; ++i)
                if (aoes[i].Check(sample))
                    return false;
        }
        return true;
    }
}

sealed class Surgeswords(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect Shape = new(30f, 3f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.RushSword && id == 4566)
        {
            _aoes.RemoveAll(aoe => aoe.ActorID == actor.InstanceID);
            _aoes.Add(new(Shape, actor.Position, actor.Rotation, WorldState.FutureTime(8.15d), actorID: actor.InstanceID));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.Surgeswords)
            return;

        var index = _aoes.FindIndex(aoe => aoe.ActorID == caster.InstanceID);
        var aoe = new AOEInstance(Shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID);
        if (index >= 0)
            _aoes[index] = aoe;
        else
            _aoes.Add(aoe);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Surgeswords)
        {
            _aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            ++NumCasts;
        }
    }
}

sealed class Steelsforge(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(13f);
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<WPos> _steelWarnings = [];
    private DateTime _firstKnockback;
    private int _jumpCount;
    private bool _collecting;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void Update()
    {
        _aoes.RemoveAll(aoe => WorldState.CurrentTime > aoe.Activation.AddSeconds(1d));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LeapingLift)
        {
            _aoes.Clear();
            _steelWarnings.Clear();
            _firstKnockback = default;
            _jumpCount = 0;
            _collecting = true;
            return;
        }

        if (spell.Action.ID != (uint)AID.Steelsforge)
            return;

        var index = ClosestAOE(caster.Position);
        var aoe = new AOEInstance(Shape, caster.Position, activation: Module.CastFinishAt(spell), actorID: caster.InstanceID);
        if (index >= 0)
            _aoes[index] = aoe;
        else
            _aoes.Add(aoe);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Steelsforge)
        {
            var index = MatchingAOE(caster);
            if (index >= 0)
                _aoes.RemoveAt(index);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (!_collecting || actor.OID != (uint)OID.Sword || status.ID != (uint)SID.LeapingLiftIndicator || status.Extra != 1173)
            return;

        if (_steelWarnings.All(position => (position - actor.Position).LengthSq() >= 1f))
            _steelWarnings.Add(actor.Position);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.Steelsforge)
        {
            var index = MatchingAOE(caster);
            if (index >= 0)
                _aoes.RemoveAt(index);
            ++NumCasts;
            return;
        }

        if (!_collecting || (id is not (uint)AID.LeapingLiftJump and not (uint)AID.LeapingLiftJumpLast))
            return;

        if (_firstKnockback == default)
            _firstKnockback = WorldState.FutureTime(LeapingLiftTiming.FirstKnockbackDelay);
        var warningIndex = ClosestSteelWarning(caster.Position);
        if (warningIndex >= 0 && _jumpCount < LeapingLiftTiming.KnockbackOffsets.Length)
        {
            var activation = _firstKnockback.AddSeconds(LeapingLiftTiming.KnockbackOffsets[_jumpCount] - LeapingLiftTiming.ForgeAdvance);
            _aoes.Add(new(Shape, caster.Position, activation: activation));
            _steelWarnings.RemoveAt(warningIndex);
        }
        ++_jumpCount;
        if (id == (uint)AID.LeapingLiftJumpLast)
            _collecting = false;
    }

    private int ClosestAOE(WPos position)
    {
        var bestIndex = -1;
        var bestDistance = 1f;
        for (var i = 0; i < _aoes.Count; ++i)
        {
            var distance = (_aoes[i].Origin - position).LengthSq();
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
            }
        }
        return bestIndex;
    }

    private int MatchingAOE(Actor caster)
    {
        var actorIndex = _aoes.FindIndex(aoe => aoe.ActorID == caster.InstanceID);
        return actorIndex >= 0 ? actorIndex : ClosestAOE(caster.Position);
    }

    private int ClosestSteelWarning(WPos position)
    {
        var bestIndex = -1;
        var bestDistance = 1f;
        for (var i = 0; i < _steelWarnings.Count; ++i)
        {
            var distance = (_steelWarnings[i] - position).LengthSq();
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
            }
        }
        return bestIndex;
    }
}

sealed class LeapingLiftKnockback(BossModule module) : Components.GenericKnockback(module, (uint)AID.Steelsbreath)
{
    private const float NextSourceMarkerRadius = 2f;
    private static readonly uint NextSourceMarkerColor = Color.FromComponents(64, 192, 255).ABGR;
    private readonly List<Knockback> _sources = [];
    private DateTime _firstActivation;
    private bool _collecting;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) =>
        _sources.Count != 0 ? CollectionsMarshal.AsSpan(_sources)[..1] : [];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_sources.Count > 1)
            Arena.ZoneCircleOutline(_sources[1].Origin, NextSourceMarkerRadius, NextSourceMarkerColor, 2f);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LeapingLift)
        {
            _sources.Clear();
            _firstActivation = default;
            _collecting = true;
            return;
        }

        if (spell.Action.ID != (uint)AID.Steelsbreath)
            return;

        var source = new Knockback(caster.Position, 26f, Module.CastFinishAt(spell), actorID: caster.InstanceID);
        var index = ClosestSource(caster.Position);
        if (index >= 0)
        {
            _sources[index] = source;
            if (index != 0)
            {
                _sources.RemoveAt(index);
                _sources.Insert(0, source);
            }
        }
        else
        {
            _sources.Insert(0, source);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (_collecting && (id is (uint)AID.LeapingLiftJump or (uint)AID.LeapingLiftJumpLast) && _sources.Count < LeapingLiftTiming.KnockbackOffsets.Length)
        {
            if (_firstActivation == default)
                _firstActivation = WorldState.FutureTime(LeapingLiftTiming.FirstKnockbackDelay);
            var activation = _firstActivation.AddSeconds(LeapingLiftTiming.KnockbackOffsets[_sources.Count]);
            _sources.Add(new(caster.Position, 26f, activation, actorID: caster.InstanceID));
            if (id == (uint)AID.LeapingLiftJumpLast)
                _collecting = false;
        }

        if (id == (uint)AID.Steelsbreath)
        {
            var index = ClosestSource(caster.Position);
            if (index >= 0)
                _sources.RemoveAt(index);
        }
        base.OnEventCast(caster, spell);
    }

    private int ClosestSource(WPos position)
    {
        var bestIndex = -1;
        var bestDistance = 1f;
        for (var i = 0; i < _sources.Count; ++i)
        {
            var distance = (_sources[i].Origin - position).LengthSq();
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
            }
        }
        return bestIndex;
    }
}

sealed class SafeSpotHints(BossModule module) : BossComponent(module)
{
    private static readonly WDir[] MarginChecks = [default, new(0.75f, 0f), new(-0.75f, 0f), new(0f, 0.75f), new(0f, -0.75f)];
    private readonly List<Components.GenericAOEs.AOEInstance> _active = [];

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        Gather(slot, actor);
        if (_active.Any(aoe => aoe.Check(actor.Position)) && FindSafe(actor.Position) is WPos safe)
            movementHints.Add((actor.Position, safe, Colors.Safe));
    }

    private void Gather(int slot, Actor actor)
    {
        _active.Clear();
        foreach (var component in Module.Components)
        {
            if (component is not Components.GenericAOEs aoes)
                continue;
            foreach (var aoe in aoes.ActiveAOEs(slot, actor))
                _active.Add(aoe);
        }

        if (_active.Count == 0)
            return;
        var earliest = _active.Min(aoe => aoe.Activation);
        var latest = earliest.AddSeconds(0.6d);
        _active.RemoveAll(aoe => aoe.Activation > latest);
    }

    private WPos? FindSafe(WPos from)
    {
        WPos? best = null;
        var bestDistance = float.MaxValue;
        for (var x = -24f; x <= 24f; x += 1f)
        {
            for (var z = -24f; z <= 24f; z += 1f)
            {
                var candidate = new WPos(MTH2SwordDancer.ArenaCenter.X + x, MTH2SwordDancer.ArenaCenter.Z + z);
                if (!Safe(candidate))
                    continue;

                var distance = (candidate - from).LengthSq();
                if (distance < bestDistance)
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }
        }
        return best;
    }

    private bool Safe(WPos position)
    {
        foreach (var offset in MarginChecks)
        {
            var sample = position + offset;
            if (!Arena.InBounds(sample))
                return false;
            for (var i = 0; i < _active.Count; ++i)
                if (_active[i].Check(sample))
                    return false;
        }
        return true;
    }
}

sealed class MTH2SwordDancerStates : StateMachineBuilder
{
    public MTH2SwordDancerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SwordStorm>()
            .ActivateOnEnter<RushShort>()
            .ActivateOnEnter<RushLong>()
            .ActivateOnEnter<RushShortAlt>()
            .ActivateOnEnter<RushLongAlt>()
            .ActivateOnEnter<TurnInner>()
            .ActivateOnEnter<TurnMiddle>()
            .ActivateOnEnter<TurnOuter>()
            .ActivateOnEnter<TurnMiddleNarrow>()
            .ActivateOnEnter<TurnOuterNarrow>()
            .ActivateOnEnter<MartialMystique>()
            .ActivateOnEnter<SpinningSword>()
            .ActivateOnEnter<SwordDance>()
            .ActivateOnEnter<Pierce>()
            .ActivateOnEnter<Steelsforge>()
            .ActivateOnEnter<LeapingLiftKnockback>()
            .ActivateOnEnter<Surgeswords>()
            .ActivateOnEnter<SafeSpotHints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(MTH2SwordDancerStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    PrimaryActorOID = (uint)OID.Boss,
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1114,
    NameID = 14820,
    SortOrder = 2)]
public sealed class MTH2SwordDancer(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsCircle(25f))
{
    public static readonly WPos ArenaCenter = new(600f, 704f);
}
