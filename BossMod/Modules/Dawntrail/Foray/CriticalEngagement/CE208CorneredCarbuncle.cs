namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE208CorneredCarbuncle;

public enum OID : uint
{
    Boss = 0x4C4F, // R9.07
    YellowGem = 0x4C50, // R1.0
    Helper = 0x233C,
    ReflectionCardinals = 0x1EC045, // EventObj, four cardinal walls
    ReflectionOffset = 0x1EC046 // EventObj, four offset walls
}

public enum AID : uint
{
    YellowGemVisual = 48280, // Boss->self, 3.0s cast
    YellowGemRay1 = 48281, // YellowGem->self, 3.0s cast, range 4 circle
    YellowGemRay2 = 48282, // YellowGem->self, 3.0s cast, range 4 circle
    RedRubyLight = 48284, // Boss->self, 3.0s cast
    RedRubyReflectionShort = 48285, // Helper->self, no cast, 20x20 rect
    RedRubyReflectionLong1 = 48286, // Helper->self, no cast, 40x40 rect
    RedRubyReflectionLong2 = 48287, // Helper->self, no cast, 40x40 rect
    StarvingDreadSecondVisual = 48288, // Helper->self, 2.5s cast
    StarvingDreadFirstVisual = 48289, // Helper->self, 2.5s cast
    StarvingDreadFirst = 48291, // Boss->location, 8.0s cast, directional knockback 15
    StarvingDreadSecond = 48292, // Boss->location, no cast, knockback 30
    ClawThenTail = 48294, // Boss->self, 6.0s cast, range 45 front 180-degree cone
    TailThenClaw = 48295, // Boss->self, 6.0s cast, range 40 rear 180-degree cone
    ClawThenTailSecond = 48296, // Boss->self, no cast, range 40 rear 180-degree cone
    TailThenClawSecond = 48297, // Boss->self, no cast, range 45 front 180-degree cone
    SonicHowl = 48298, // Boss->self, 5.0s cast, raidwide
    StarvingDreadSecondDamage = 49506, // Helper->self, no cast
    StarvingDreadFirstDamage = 49507, // Helper->self, no cast
    ReflectOuter = 50418, // Boss->self, 3.0s cast
    RedRubyLightDamage = 50637 // Helper->self, no cast
}

sealed class YellowGemRay1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.YellowGemRay1, 4f);
sealed class YellowGemRay2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.YellowGemRay2, 4f);
sealed class RedRubyLight(BossModule module) : Components.RaidwideCast(module, (uint)AID.RedRubyLight);
sealed class SonicHowl(BossModule module) : Components.RaidwideCast(module, (uint)AID.SonicHowl);

sealed class RedRubyReflection(BossModule module) : Components.GenericAOEs(module)
{
    private enum WallPattern { None, Cardinals, Offset }

    private static readonly WDir[] CardinalEndpoints = [new(0f, -20f), new(20f, 0f), new(0f, 20f), new(-20f, 0f)];
    private static readonly WDir[] OffsetEndpoints = [new(-10f, -20f), new(20f, 10f), new(10f, 20f), new(-20f, -10f)];
    private readonly List<AOEInstance> _aoes = new(4);
    private readonly List<(Angle Direction, Angle HalfAngle)> _regions = new(4);
    private WallPattern _pattern;
    private Angle _rotation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var ray in ActiveRays())
            Arena.AddLine(Arena.Center, Arena.Center + 20f * ray.ToDirection(), Colors.Border);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state is not 0x00100020 and not 0x01000200)
            return;

        var pattern = actor.OID switch
        {
            (uint)OID.ReflectionCardinals => WallPattern.Cardinals,
            (uint)OID.ReflectionOffset => WallPattern.Offset,
            _ => WallPattern.None
        };
        if (pattern == WallPattern.None)
            return;

        _pattern = pattern;
        _rotation = actor.Rotation;
        _aoes.Clear();
        _regions.Clear();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.YellowGemRay1 || _pattern == WallPattern.None)
            return;

        var (direction, halfAngle) = RegionForPosition(caster.Position);
        if (_regions.Any(r => r.Direction.AlmostEqual(direction, Angle.DegToRad) && r.HalfAngle.AlmostEqual(halfAngle, Angle.DegToRad)))
            return;

        _regions.Add((direction, halfAngle));
        _aoes.Add(new(new AOEShapeCone(20f, halfAngle), Arena.Center, direction, Module.CastFinishAt(spell, 0.1d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_pattern != WallPattern.None && spell.Action.ID is (uint)AID.RedRubyReflectionShort or (uint)AID.RedRubyReflectionLong1 or (uint)AID.RedRubyReflectionLong2)
        {
            _pattern = WallPattern.None;
            _aoes.Clear();
            _regions.Clear();
            ++NumCasts;
        }
    }

    private Angle[] ActiveRays()
    {
        var endpoints = _pattern == WallPattern.Cardinals ? CardinalEndpoints : _pattern == WallPattern.Offset ? OffsetEndpoints : [];
        var rays = new Angle[endpoints.Length];
        for (var i = 0; i < endpoints.Length; ++i)
            rays[i] = Angle.FromDirection(endpoints[i]) + _rotation;
        return rays;
    }

    private (Angle Direction, Angle HalfAngle) RegionForPosition(WPos position)
    {
        var rays = ActiveRays().Select(r => NormalizePositive(r.Rad)).Order().ToArray();
        var positionAngle = NormalizePositive(Angle.FromDirection(position - Arena.Center).Rad);
        for (var i = 0; i < rays.Length; ++i)
        {
            var start = rays[i];
            var end = i + 1 < rays.Length ? rays[i + 1] : rays[0] + Angle.DoublePI;
            var adjustedPosition = i == rays.Length - 1 && positionAngle < start ? positionAngle + Angle.DoublePI : positionAngle;
            if (adjustedPosition >= start && adjustedPosition < end)
            {
                var halfAngle = 0.5f * (end - start);
                return (new(NormalizePositive(start + halfAngle)), new(halfAngle));
            }
        }
        return default;
    }

    private static float NormalizePositive(float angle)
    {
        angle %= Angle.DoublePI;
        return angle < 0f ? angle + Angle.DoublePI : angle;
    }
}

sealed class StarvingDread(BossModule module) : Components.GenericKnockback(module)
{
    private Actor? _firstCaster;
    private Knockback? _second;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        Knockback? first = null;
        if (_firstCaster?.CastInfo is { } cast)
        {
            var direction = cast.Rotation;
            var kind = direction.ToDirection().OrthoL().Dot(actor.Position - _firstCaster.Position) >= 0f ? Kind.DirLeft : Kind.DirRight;
            first = new(_firstCaster.Position, 15f, Module.CastFinishAt(cast), direction: direction, kind: kind);
        }

        if (first != null && _second != null)
            return new Knockback[2] { first.Value, _second.Value };
        if (first != null)
            return new Knockback[1] { first.Value };
        return _second != null ? new Knockback[1] { _second.Value } : [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StarvingDreadFirst)
            _firstCaster = caster;
        else if (spell.Action.ID == (uint)AID.StarvingDreadSecondVisual)
            _second = new(caster.Position, 30f, Module.CastFinishAt(spell, 5d));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StarvingDreadFirst)
        {
            _firstCaster = null;
            ++NumCasts;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.StarvingDreadSecondDamage)
        {
            _second = null;
            ++NumCasts;
        }
    }
}

sealed class ClawTailCombo(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone Front = new(45f, 90f.Degrees());
    private static readonly AOEShapeCone Rear = new(40f, 90f.Degrees());
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
        var activation = Module.CastFinishAt(spell);
        switch (spell.Action.ID)
        {
            case (uint)AID.ClawThenTail:
                _aoes.Add(new(Front, caster.Position, spell.Rotation, activation, actorID: caster.InstanceID));
                _aoes.Add(new(Rear, caster.Position, spell.Rotation + 180f.Degrees(), activation.AddSeconds(3.1d), actorID: caster.InstanceID));
                break;
            case (uint)AID.TailThenClaw:
                _aoes.Add(new(Rear, caster.Position, spell.Rotation + 180f.Degrees(), activation, actorID: caster.InstanceID));
                _aoes.Add(new(Front, caster.Position, spell.Rotation, activation.AddSeconds(3.1d), actorID: caster.InstanceID));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ClawThenTail or (uint)AID.TailThenClaw or (uint)AID.ClawThenTailSecond or (uint)AID.TailThenClawSecond)
        {
            var index = _aoes.FindIndex(a => a.ActorID == caster.InstanceID);
            if (index >= 0)
                _aoes.RemoveAt(index);
        }
    }
}

sealed class CE208CorneredCarbuncleStates : StateMachineBuilder
{
    public CE208CorneredCarbuncleStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<YellowGemRay1>()
            .ActivateOnEnter<YellowGemRay2>()
            .ActivateOnEnter<RedRubyLight>()
            .ActivateOnEnter<RedRubyReflection>()
            .ActivateOnEnter<StarvingDread>()
            .ActivateOnEnter<ClawTailCombo>()
            .ActivateOnEnter<SonicHowl>();
    }
}

// Temporary module; remove when upstream adds an encounter for OID 0x4C4F.
[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(CE208CorneredCarbuncleStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093,
    NameID = 14791)]
public sealed class CE208CorneredCarbuncle(WorldState ws, Actor primary) : BossModule(ws, primary, new(238f, 352f), new ArenaBoundsCircle(20f));
