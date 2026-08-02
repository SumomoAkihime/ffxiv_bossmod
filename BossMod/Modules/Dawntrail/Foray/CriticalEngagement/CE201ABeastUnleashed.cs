namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE201ABeastUnleashed;

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

sealed class YellowGemRays(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(4f);
    private readonly List<AOEInstance> _aoes = new(12);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID != (uint)OID.YellowGem || id != 9353)
            return;

        _aoes.RemoveAll(a => a.ActorID == actor.InstanceID);
        _aoes.Add(new(Shape, actor.Position, actorID: actor.InstanceID));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.YellowGemVisual)
        {
            _aoes.Clear();
            return;
        }
        if (spell.Action.ID is not (uint)AID.YellowGemRay1 and not (uint)AID.YellowGemRay2)
            return;

        var aoe = new AOEInstance(Shape, caster.Position, activation: Module.CastFinishAt(spell), actorID: caster.InstanceID);
        var index = _aoes.FindIndex(a => a.ActorID == caster.InstanceID);
        if (index >= 0)
            _aoes[index] = aoe;
        else
            _aoes.Add(aoe);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.YellowGemRay1 or (uint)AID.YellowGemRay2)
            _aoes.RemoveAll(a => a.ActorID == caster.InstanceID);
    }
}

sealed class RedRubyLight(BossModule module) : Components.RaidwideCast(module, (uint)AID.RedRubyLight);
sealed class SonicHowl(BossModule module) : Components.RaidwideCast(module, (uint)AID.SonicHowl);

sealed class RedRubyReflection(BossModule module) : Components.GenericAOEs(module)
{
    private enum WallPattern { None, Cardinals, Offset }

    private static readonly AOEShapeRect Cell = new(5f, 5f, 5f);
    private static readonly (WDir A, WDir B)[] CardinalWalls =
    [
        (new(0f, -20f), new(0f, 20f)),
        (new(-20f, 0f), new(20f, 0f))
    ];
    private static readonly (WDir A, WDir B)[] OffsetWalls =
    [
        (new(0f, -20f), new(0f, 20f)),
        (new(-10f, -10f), new(0f, -10f)),
        (new(-10f, -10f), new(-10f, 10f)),
        (new(-20f, 10f), new(-10f, 10f)),
        (new(0f, 10f), new(10f, 10f)),
        (new(10f, -10f), new(10f, 10f)),
        (new(10f, -10f), new(20f, -10f))
    ];
    private readonly List<AOEInstance> _aoes = new(16);
    private readonly Dictionary<ulong, WPos> _gems = new(12);
    private WallPattern _pattern;
    private DateTime _activation;
    private int _offsetTransform;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var wall in ActiveWalls())
            Arena.AddLine(Arena.Center + wall.A, Arena.Center + wall.B, Colors.Border);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.YellowGem && id == 9353)
            _gems[actor.InstanceID] = actor.Position;
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
        _aoes.Clear();
        _activation = WorldState.FutureTime(pattern == WallPattern.Cardinals ? 12d : 15d);
        var quarterTurns = ((int)MathF.Round(actor.Rotation.Deg / 90f) % 4 + 4) % 4;
        _offsetTransform = state == 0x00100020 ? (quarterTurns + 3) & 3 : 4 + quarterTurns;
        if (_gems.Count != 0)
            RebuildAOEs();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.YellowGemVisual)
        {
            _gems.Clear();
            return;
        }
        if (spell.Action.ID != (uint)AID.YellowGemRay1 || _pattern == WallPattern.None)
            return;

        _gems[caster.InstanceID] = caster.Position;
        _activation = Module.CastFinishAt(spell, 0.1d);
        RebuildAOEs();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_pattern != WallPattern.None && spell.Action.ID is (uint)AID.RedRubyReflectionShort or (uint)AID.RedRubyReflectionLong1 or (uint)AID.RedRubyReflectionLong2)
        {
            _pattern = WallPattern.None;
            _aoes.Clear();
            _gems.Clear();
            ++NumCasts;
        }
    }

    private (WDir A, WDir B)[] ActiveWalls()
    {
        if (_pattern == WallPattern.Cardinals)
            return CardinalWalls;
        if (_pattern != WallPattern.Offset)
            return [];

        var walls = new (WDir A, WDir B)[OffsetWalls.Length];
        for (var i = 0; i < walls.Length; ++i)
            walls[i] = (Transform(OffsetWalls[i].A, _offsetTransform), Transform(OffsetWalls[i].B, _offsetTransform));
        return walls;
    }

    private void RebuildAOEs()
    {
        var activeWalls = ActiveWalls();
        var dangerousCells = 0u;
        foreach (var gem in _gems.Values)
        {
            if (TouchesWall(gem, activeWalls))
                dangerousCells |= RegionMask(CellIndex(gem), activeWalls);
        }

        _aoes.Clear();
        for (var i = 0; i < 16; ++i)
        {
            if ((dangerousCells & (1u << i)) != 0)
                _aoes.Add(new(Cell, CellCenter(i), default, _activation));
        }
    }

    private bool TouchesWall(WPos position, (WDir A, WDir B)[] walls)
    {
        var offset = position - Arena.Center;
        return walls.Any(w => DistanceToSegmentSq(offset, w.A, w.B) <= 16.1f);
    }

    private uint RegionMask(int start, (WDir A, WDir B)[] walls)
    {
        var result = 0u;
        var pending = new Stack<int>();
        pending.Push(start);
        while (pending.Count != 0)
        {
            var cell = pending.Pop();
            var bit = 1u << cell;
            if ((result & bit) != 0)
                continue;
            result |= bit;

            var row = cell >> 2;
            var column = cell & 3;
            if (column > 0 && !CellsSeparated(cell, cell - 1, walls))
                pending.Push(cell - 1);
            if (column < 3 && !CellsSeparated(cell, cell + 1, walls))
                pending.Push(cell + 1);
            if (row > 0 && !CellsSeparated(cell, cell - 4, walls))
                pending.Push(cell - 4);
            if (row < 3 && !CellsSeparated(cell, cell + 4, walls))
                pending.Push(cell + 4);
        }
        return result;
    }

    private static bool CellsSeparated(int first, int second, (WDir A, WDir B)[] walls)
    {
        var midpoint = 0.5f * (CellOffset(first) + CellOffset(second));
        return walls.Any(w => DistanceToSegmentSq(midpoint, w.A, w.B) < 0.01f);
    }

    private int CellIndex(WPos position)
    {
        var offset = position - Arena.Center;
        return (CoordinateIndex(offset.Z) << 2) | CoordinateIndex(offset.X);
    }

    private static int CoordinateIndex(float coordinate) => coordinate switch
    {
        < -10f => 0,
        < 0f => 1,
        < 10f => 2,
        _ => 3
    };

    private WPos CellCenter(int index) => Arena.Center + CellOffset(index);

    private static WDir CellOffset(int index) => new(-15f + 10f * (index & 3), -15f + 10f * (index >> 2));

    private static WDir Transform(WDir offset, int transform)
    {
        var x = offset.X;
        var z = transform >= 4 ? -offset.Z : offset.Z;
        return (transform & 3) switch
        {
            0 => new(x, z),
            1 => new(-z, x),
            2 => new(-x, -z),
            _ => new(z, -x)
        };
    }

    private static float DistanceToSegmentSq(WDir point, WDir start, WDir end)
    {
        var segment = end - start;
        var lengthSq = segment.LengthSq();
        var t = lengthSq > 0f ? Math.Clamp((point - start).Dot(segment) / lengthSq, 0f, 1f) : 0f;
        return (point - (start + t * segment)).LengthSq();
    }
}

sealed class StarvingDread(BossModule module) : Components.GenericKnockback(module)
{
    private Angle? _firstDirection;
    private DateTime _firstActivation;
    private Knockback? _second;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        Knockback? first = null;
        if (_firstDirection is { } direction)
        {
            var kind = direction.ToDirection().OrthoL().Dot(actor.Position - Arena.Center) >= 0f ? Kind.DirLeft : Kind.DirRight;
            first = new(Arena.Center, 15f, _firstActivation, direction: direction, kind: kind);
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
        {
            var movement = spell.LocXZ - caster.Position;
            if (movement.LengthSq() > 1f)
            {
                _firstDirection = Angle.FromDirection(movement);
                _firstActivation = Module.CastFinishAt(spell);
            }
        }
        else if (spell.Action.ID == (uint)AID.StarvingDreadSecondVisual)
            _second = new(caster.Position, 30f, Module.CastFinishAt(spell, 5d));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StarvingDreadFirst)
        {
            _firstDirection = null;
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
                _aoes.Add(new(Rear, caster.Position, spell.Rotation, activation, actorID: caster.InstanceID));
                _aoes.Add(new(Front, caster.Position, spell.Rotation + 180f.Degrees(), activation.AddSeconds(3.1d), actorID: caster.InstanceID));
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

sealed class ABeastUnleashedStates : StateMachineBuilder
{
    public ABeastUnleashedStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<YellowGemRays>()
            .ActivateOnEnter<RedRubyLight>()
            .ActivateOnEnter<RedRubyReflection>()
            .ActivateOnEnter<StarvingDread>()
            .ActivateOnEnter<ClawTailCombo>()
            .ActivateOnEnter<SonicHowl>();
    }
}

// Temporary module; remove when upstream adds an encounter for OID 0x4C4F.
[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(ABeastUnleashedStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093,
    NameID = 14791)]
public sealed class ABeastUnleashed(WorldState ws, Actor primary) : BossModule(ws, primary, new(238f, 352f), new ArenaBoundsSquare(20f));
