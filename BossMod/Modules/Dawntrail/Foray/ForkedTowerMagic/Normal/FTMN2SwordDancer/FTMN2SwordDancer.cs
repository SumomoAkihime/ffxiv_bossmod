namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN2SwordDancer;

public enum OID : uint
{
    Boss = 0x4D76, // R6.0
    MovingSword = 0x4D77,
    Sword = 0x4D7A,
    SpinningSword = 0x4D79,
    RushSword = 0x4D7C,
    Helper = 0x233C
}

public enum AID : uint
{
    SwordStorm = 49617, // Boss->self, raidwide
    RushShort = 50525, // MovingSword->location, width 7 charge
    RushLong = 50526, // MovingSword->location, width 7 charge
    TurnInner = 49575, // Helper->self, range 9-14 90-degree cone
    TurnOuter = 49577, // Helper->self, range 19-24 90-degree cone
    TurnInnerNarrow = 49578, // Helper->self, range 9-14 65-degree cone
    TurnOuterNarrow = 49580, // Helper->self, range 19-24 54-degree cone
    TurnaboutInner = 49883, // Helper->self, range 9-14 65-degree cone
    TurnaboutOuter = 49889, // Helper->self, range 19-24 54-degree cone
    MartialMystique = 49585, // Helper->self, 48x96 rect
    SpinCircle = 49592, // Sword->self, range 15 circle
    SpinDonutLarge = 49589, // Sword->self, range 20-60 donut
    SpinDonutSmall = 49590, // Sword->self, range 15-60 donut
    SwordDanceVisual = 49609, // Boss->self, sequence visual
    SwordDance = 49614, // Helper->self, 60x20 rect
    LeapingLift = 49594, // Boss->self, four-knockback visual
    Pierce = 49595, // Sword->self, range 5 circle
    Steelsbreath = 50359, // Helper->self, knockback 24
    Surgeswords = 49616 // RushSword->self, 30x6 rect
}

public enum SID : uint
{
    LeapingLift = 2056 // Sword, extra 1147 indicates knockback order
}

sealed class SwordStorm(BossModule module) : Components.RaidwideCast(module, (uint)AID.SwordStorm);
sealed class RushShort(BossModule module) : Components.ChargeAOEs(module, (uint)AID.RushShort, 3.5f);
sealed class RushLong(BossModule module) : Components.ChargeAOEs(module, (uint)AID.RushLong, 3.5f);
sealed class TurnInner(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TurnInner, new AOEShapeDonutSector(9f, 14f, 45f.Degrees()));
sealed class TurnOuter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TurnOuter, new AOEShapeDonutSector(19f, 24f, 45f.Degrees()));
sealed class TurnInnerNarrow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TurnInnerNarrow, new AOEShapeDonutSector(9f, 14f, 32.5f.Degrees()));
sealed class TurnOuterNarrow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TurnOuterNarrow, new AOEShapeDonutSector(19f, 24f, 27f.Degrees()));
sealed class TurnaboutInner(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TurnaboutInner, new AOEShapeDonutSector(9f, 14f, 32.5f.Degrees()));
sealed class TurnaboutOuter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TurnaboutOuter, new AOEShapeDonutSector(19f, 24f, 27f.Degrees()));
sealed class MartialMystique(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MartialMystique, new AOEShapeRect(48f, 48f));
sealed class SwordDance(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect Shape = new(60f, 10f, 60f);
    private readonly List<AOEInstance> _aoes = [];
    private Angle? _firstDirection;
    private int _resolved;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_resolved is > 0 and < 4 && _firstDirection is Angle direction)
        {
            var center = Arena.Center;
            var first = new Rectangle(center, 10f, 60f, direction);
            Shape[] otherLines =
            [
                new Rectangle(center, 10f, 60f, direction + 45f.Degrees()),
                new Rectangle(center, 10f, 60f, direction + 90f.Degrees()),
                new Rectangle(center, 10f, 60f, direction + 135f.Degrees())
            ];
            var safeRegion = new AOEShapeCustom([first], otherLines, origin: center);
            safeRegion.Draw(Arena, center, default(Angle), color: Colors.SafeFromAOE);
        }
        base.DrawArenaBackground(pcSlot, pc);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SwordDanceVisual)
        {
            _aoes.Clear();
            _firstDirection = null;
            _resolved = 0;
        }
        else if (spell.Action.ID == (uint)AID.SwordDance)
        {
            _firstDirection ??= spell.Rotation;
            _aoes.Add(new(Shape, Arena.Center, spell.Rotation, Module.CastFinishAt(spell), Colors.Danger, actorID: caster.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SwordDance)
        {
            _aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            ++_resolved;
            ++NumCasts;
        }
    }
}
sealed class Pierce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pierce, 5f);
sealed class Steelsbreath(BossModule module) : Components.GenericKnockback(module, (uint)AID.Steelsbreath)
{
    private const float NextSourceMarkerRadius = 2f;
    private static readonly uint NextSourceMarkerColor = Color.FromComponents(64, 192, 255).ABGR;
    private readonly List<Knockback> _sources = [];

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
            return;
        }
        if (spell.Action.ID != (uint)AID.Steelsbreath)
            return;

        var source = new Knockback(caster.Position, 24f, Module.CastFinishAt(spell), actorID: caster.InstanceID);
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

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor.OID == (uint)OID.Sword && status.ID == (uint)SID.LeapingLift && status.Extra == 1147
            && _sources.All(source => source.ActorID != actor.InstanceID))
        {
            _sources.Add(new(actor.Position, 24f, status.ExpireAt.AddSeconds(-1d), actorID: actor.InstanceID));
            _sources.Sort((left, right) => left.Activation.CompareTo(right.Activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Steelsbreath)
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

sealed class SpinningSword(BossModule module) : Components.GenericAOEs(module)
{
    private const float SafeMargin = 0.75f;
    private static readonly uint FutureSafeColor = Color.FromComponents(64, 192, 255, 64).ABGR;
    private static readonly AOEShapeCircle Circle = new(15f);
    private static readonly AOEShapeDonut DonutLarge = new(20f, 60f);
    private static readonly AOEShapeDonut DonutSmall = new(15f, 60f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_aoes.Count != 0)
        {
            var ordered = _aoes.OrderBy(aoe => aoe.Activation).ToList();
            var currentActivation = ordered[0].Activation;
            var current = ordered.Where(aoe => Math.Abs((aoe.Activation - currentActivation).TotalSeconds) < 0.5d).ToList();
            var nextIndex = ordered.FindIndex(aoe => (aoe.Activation - currentActivation).TotalSeconds >= 0.5d);
            if (nextIndex >= 0)
            {
                var next = ordered[nextIndex];
                var nextGroup = ordered.Where(aoe => Math.Abs((aoe.Activation - next.Activation).TotalSeconds) < 0.5d).ToList();
                DrawSafeRegion(nextGroup, FutureSafeColor);
            }
            DrawSafeRegion(current, Colors.SafeFromAOE);
        }
        base.DrawArenaBackground(pcSlot, pc);
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID != (uint)OID.SpinningSword)
            return;

        _aoes.RemoveAll(aoe => aoe.ActorID == actor.InstanceID);
        AOEShape? shape = modelState switch
        {
            7 => Circle,
            4 => DonutSmall,
            5 => DonutLarge,
            _ => null
        };
        if (shape != null)
            _aoes.Add(new(shape, actor.Position, actor.Rotation, WorldState.FutureTime(13d), actorID: actor.InstanceID));
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.SpinningSword && id == 9710)
            Update(actor, WorldState.FutureTime(9.4d));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SpinCircle or (uint)AID.SpinDonutLarge or (uint)AID.SpinDonutSmall)
            Update(caster, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SpinCircle or (uint)AID.SpinDonutLarge or (uint)AID.SpinDonutSmall)
        {
            _aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            ++NumCasts;
        }
    }

    private void Update(Actor actor, DateTime activation)
    {
        var index = _aoes.FindIndex(aoe => aoe.ActorID == actor.InstanceID);
        if (index >= 0)
        {
            var aoe = _aoes[index];
            aoe.Origin = actor.Position;
            aoe.Rotation = actor.Rotation;
            aoe.Activation = activation;
            _aoes[index] = aoe;
        }
    }

    private void DrawSafeRegion(List<AOEInstance> aoes, uint color)
    {
        var dangers = new Shape[aoes.Count];
        for (var i = 0; i < aoes.Count; ++i)
        {
            dangers[i] = aoes[i].Shape switch
            {
                AOEShapeCircle circle => new Circle(aoes[i].Origin, circle.Radius + SafeMargin),
                AOEShapeDonut donut => new Donut(aoes[i].Origin, MathF.Max(0f, donut.InnerRadius - SafeMargin), donut.OuterRadius + SafeMargin),
                _ => throw new ArgumentOutOfRangeException(nameof(aoes))
            };
        }
        var safeRegion = new AOEShapeCustom([new Circle(Arena.Center, 24f - SafeMargin)], dangers, origin: Arena.Center);
        safeRegion.Draw(Arena, Arena.Center, default(Angle), color: color);
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
        if (spell.Action.ID == (uint)AID.Surgeswords)
        {
            var index = _aoes.FindIndex(aoe => aoe.ActorID == caster.InstanceID);
            if (index >= 0)
            {
                var aoe = _aoes[index];
                aoe.Origin = caster.Position;
                aoe.Rotation = spell.Rotation;
                aoe.Activation = Module.CastFinishAt(spell);
                _aoes[index] = aoe;
            }
            else
            {
                _aoes.Add(new(Shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
            }
        }
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

sealed class SwordDancerStates : StateMachineBuilder
{
    public SwordDancerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SwordStorm>()
            .ActivateOnEnter<RushShort>()
            .ActivateOnEnter<RushLong>()
            .ActivateOnEnter<TurnInner>()
            .ActivateOnEnter<TurnOuter>()
            .ActivateOnEnter<TurnInnerNarrow>()
            .ActivateOnEnter<TurnOuterNarrow>()
            .ActivateOnEnter<TurnaboutInner>()
            .ActivateOnEnter<TurnaboutOuter>()
            .ActivateOnEnter<MartialMystique>()
            .ActivateOnEnter<SpinningSword>()
            .ActivateOnEnter<SwordDance>()
            .ActivateOnEnter<Pierce>()
            .ActivateOnEnter<Steelsbreath>()
            .ActivateOnEnter<Surgeswords>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(SwordDancerStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    PrimaryActorOID = (uint)OID.Boss,
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093,
    NameID = 14820,
    SortOrder = 2)]
public sealed class SwordDancer(WorldState ws, Actor primary) : BossModule(ws, primary, new(600f, 704f), new ArenaBoundsCircle(24f));
