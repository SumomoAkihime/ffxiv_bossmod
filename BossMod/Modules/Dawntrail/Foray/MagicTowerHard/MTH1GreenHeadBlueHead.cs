namespace BossMod.Dawntrail.Foray.MagicTowerHard.MTH1GreenHeadBlueHead;

public enum OID : uint
{
    Boss = 0x4C18, // R15.0, visual actor shared by both heads
    GreenHead = 0x4C19, // R15.0, module primary actor
    BlueHead = 0x4C1A, // R15.0
    GreenHeadMechanic = 0x4C1B, // R1.0
    BlueHeadMechanic = 0x4C1C, // R1.0
    LightningOrb = 0x4C1F, // R2.4
    IceOrb = 0x4C20, // R2.8
    GreenArcaneMatrix = 0x4C22, // NameID 14497
    BlueArcaneMatrix = 0x4C23, // NameID 14498
    MechanicTarget = 0x4C24,
    SequenceMarker = 0x4A35,
    Helper = 0x233C
}

public enum AID : uint
{
    Buffet = 49726,
    StormsBreathVisual = 47631,
    FulgurousFugueVisual = 47632,
    FreezingFugueVisual = 47633,
    StormsBreath = 47638, // Helper->self, knockback from arena center
    PoisonBreath = 47639, // Helper->location, range 18 circle
    FulgurousFugue = 47640, // Helper->self, range 20-60 donut
    FreezingFugue = 47641, // Helper->self, range 20 circle
    StormsBreathKnockback = 48245,
    PoisonBreathVisual = 50717,

    FulgurousFugueVisual2 = 47619,
    FreezingFugueVisual2 = 47620,
    FulgurousFugue2 = 47629,
    FreezingFugue2 = 47630,
    FulgurousFugueVisual3 = 50723,
    FreezingFugueVisual3 = 50724,
    FulgurousFugue3 = 50727,
    FreezingFugue3 = 50728,

    BlazeloopAndRepeat = 47672,
    CrossblazeBlazeloop = 47673,
    CrossblazeAndRepeat = 47675,
    BlazeloopCrossblaze = 47678,
    BlazeSequenceFirst = 47683,
    BlazeSequenceSecond = 47684,
    Crossblaze = 47685, // Helper->self, range 35 cross, width 10
    Blazeloop = 47686, // Helper->self, range 5-60 donut
    BlazeFollowing = 47689,
    BlazeCircleBlue = 50706, // Helper->location, range 5 circle
    BlazeCircleGreen = 50707, // Helper->location, range 5 circle
    BlazeCircleFollowing = 50708, // Helper->location, range 5 circle

    ArcaneRevelation = 47719,
    GreenArcaneBeacon = 47721, // matrix->self, 60x5 rect
    BlueArcaneBeacon = 47722, // matrix->self, 60x5 rect
    TwoTerrorsWide = 47702, // Helper->self, 40x20 rect
    TwoTerrorsNarrow = 47703, // Helper->self, 40x10 rect

    Summon = 47710,
    BreathyDuet = 47646,
    MarkerMove = 47653,
    LightningClusterFirst = 50699, // Helper->location, range 15 circle
    IceClusterFirst = 50700, // Helper->location, range 15 circle
    LightningCluster = 50701, // Helper->location, range 15 circle
    IceCluster = 50702, // Helper->location, range 15 circle
    LevinWave = 47714, // orb->self, range 45 cone
    IceWave = 47715, // orb->self, range 45 cone

    HissingResonance = 47723,
    GreenBuffetEast = 47725,
    GreenBuffetWest = 47726,
    BlueBuffetEast = 47727,
    BlueBuffetWest = 47728,

    ThunderfrostTempest = 47739,
    Enrage = 47742,
    Archaeofury = 47749,
    ArchaeofuryGreen = 47751, // Helper->player, range 6 circle
    ArchaeofuryBlue = 47752 // Helper->player, range 6 circle
}

public enum SID : uint
{
    GreenNoiseEasterly = 5052,
    GreenNoiseWesterly = 5053,
    BlueNoiseEasterly = 5054,
    BlueNoiseWesterly = 5055
}

public enum IconID : uint
{
    Archaeofury = 344,
    Sequence1 = 722,
    Sequence2 = 723,
    Sequence3 = 724,
    Sequence4 = 725
}

public enum TetherID : uint
{
    Mechanic = 411,
    Buffet = 429
}

static class SafeSpot
{
    private static readonly WDir[] MarginChecks = [default, new(0.8f, 0f), new(-0.8f, 0f), new(0f, 0.8f), new(0f, -0.8f)];

    public static WPos? Find(WPos from, ReadOnlySpan<Components.GenericAOEs.AOEInstance> aoes)
    {
        WPos? best = null;
        var bestDistance = float.MaxValue;
        for (var x = -18f; x <= 18f; x += 1f)
        {
            for (var z = -18f; z <= 18f; z += 1f)
            {
                var candidate = new WPos(MTH1GreenHeadBlueHead.ArenaCenter.X + x, MTH1GreenHeadBlueHead.ArenaCenter.Z + z);
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

    public static bool Safe(WPos position, ReadOnlySpan<Components.GenericAOEs.AOEInstance> aoes)
    {
        foreach (var offset in MarginChecks)
        {
            var sample = position + offset;
            if (!InBounds(sample))
                return false;
            for (var i = 0; i < aoes.Length; ++i)
                if (aoes[i].Check(sample))
                    return false;
        }
        return true;
    }

    public static bool Safe(WPos position, in Components.GenericAOEs.AOEInstance aoe)
    {
        foreach (var offset in MarginChecks)
        {
            var sample = position + offset;
            if (!InBounds(sample) || aoe.Check(sample))
                return false;
        }
        return true;
    }

    public static bool InBounds(WPos position) =>
        MathF.Abs(position.X - MTH1GreenHeadBlueHead.ArenaCenter.X) <= 18.5f
        && MathF.Abs(position.Z - MTH1GreenHeadBlueHead.ArenaCenter.Z) <= 18.5f;
}

abstract class PredictiveAOEs(BossModule module, string warningText = "GTFO from predicted AOE!") : Components.GenericAOEs(module, warningText: warningText)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var aoes = ActiveAOEs(pcSlot, pc);
        if (aoes.Length != 0 && SafeSpot.Find(pc.Position, aoes) is WPos safe)
            Arena.ZoneCircleOutline(safe, 1.2f, Colors.Safe, 2f);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        var aoes = ActiveAOEs(slot, actor);
        if (aoes.Length == 0)
            return;

        var unsafeNow = false;
        for (var i = 0; i < aoes.Length; ++i)
            unsafeNow |= aoes[i].Check(actor.Position);
        if (unsafeNow && SafeSpot.Find(actor.Position, aoes) is WPos safe)
            movementHints.Add((actor.Position, safe, Colors.Safe));
    }
}

sealed class BuffetAssignments(BossModule module) : BossComponent(module)
{
    private static readonly WPos BlueHalf = new(-910f, 700f);
    private static readonly WPos GreenHalf = new(-890f, 700f);
    private static readonly uint BlueDim = Color.FromComponents(45, 125, 255, 48).ABGR;
    private static readonly uint BlueStrong = Color.FromComponents(45, 125, 255, 112).ABGR;
    private static readonly uint GreenDim = Color.FromComponents(45, 220, 120, 48).ABGR;
    private static readonly uint GreenStrong = Color.FromComponents(45, 220, 120, 112).ABGR;
    private readonly Dictionary<ulong, bool> _assignments = [];
    private DateTime _showUntil;

    private bool Active => WorldState.CurrentTime <= _showUntil;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Active && _assignments.TryGetValue(actor.InstanceID, out var blue))
            hints.Add(blue ? "Blue group: go left" : "Green group: go right", false);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!Active)
            return;

        _assignments.TryGetValue(pc.InstanceID, out var blue);
        var assigned = _assignments.ContainsKey(pc.InstanceID);
        Arena.ZoneRect(BlueHalf, default(Angle), 10f, 10f, 10f, assigned && blue ? BlueStrong : BlueDim);
        Arena.ZoneRect(GreenHalf, default(Angle), 10f, 10f, 10f, assigned && !blue ? GreenStrong : GreenDim);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.Buffet)
            return;

        if (!Active)
            _assignments.Clear();
        var finish = Module.CastFinishAt(spell).AddSeconds(1d);
        if (finish > _showUntil)
            _showUntil = finish;
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.Buffet)
            return;

        var target = WorldState.Actors.Find(tether.Target);
        if (target?.OID == (uint)OID.BlueHeadMechanic)
            _assignments[source.InstanceID] = true;
        else if (target?.OID == (uint)OID.GreenHeadMechanic)
            _assignments[source.InstanceID] = false;
    }
}

sealed class ComboAOEs(BossModule module) : PredictiveAOEs(module)
{
    private static readonly AOEShapeCircle Poison = new(18f);
    private static readonly AOEShapeCircle Freezing = new(20f);
    private static readonly AOEShapeDonut Fulgurous = new(20f, 60f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            return [];

        var earliest = _aoes[0].Activation.AddSeconds(0.5d);
        var count = 1;
        while (count < _aoes.Count && _aoes[count].Activation <= earliest)
            ++count;
        return CollectionsMarshal.AsSpan(_aoes)[..count];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.PoisonBreath => Poison,
            (uint)AID.FreezingFugue or (uint)AID.FreezingFugue2 or (uint)AID.FreezingFugue3 => Freezing,
            (uint)AID.FulgurousFugue or (uint)AID.FulgurousFugue2 or (uint)AID.FulgurousFugue3 => Fulgurous,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
            _aoes.Sort((left, right) => left.Activation.CompareTo(right.Activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var index = _aoes.FindIndex(aoe => aoe.ActorID == caster.InstanceID);
        if (index >= 0)
            _aoes.RemoveAt(index);
    }
}

sealed class StormsBreath(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.StormsBreathKnockback, 14f);

sealed class FourfoldBlaze(BossModule module) : PredictiveAOEs(module)
{
    private sealed class Pattern(AOEShape first, AOEShape second)
    {
        public readonly AOEShape[] Shapes = [first, second];
        public int Next;

        public void Reset() => Next = 0;
    }

    public readonly record struct Preview(AOEShape Shape, WPos Origin, DateTime Activation, bool Green, int Sequence)
    {
        public AOEInstance AOE => new(Shape, Origin, activation: Activation);
    }

    private static readonly AOEShapeCircle Circle = new(5f);
    private static readonly AOEShapeCross Cross = new(35f, 5f);
    private static readonly AOEShapeDonut Donut = new(5f, 60f);
    private Pattern? _greenPattern;
    private Pattern? _bluePattern;
    private bool _firstGreen;
    private Preview? _resolvedGreenCircle;
    private Preview? _resolvedBlueCircle;
    private readonly List<Preview> _previews = [];
    private readonly AOEInstance[] _active = new AOEInstance[2];

    public Preview? Current => _previews.Count != 0 ? _previews[0] : null;

    public Preview? CircleForKnockback(bool green)
    {
        var resolved = green ? _resolvedGreenCircle : _resolvedBlueCircle;
        if (resolved is Preview recent && WorldState.CurrentTime <= recent.Activation.AddSeconds(1d))
            return recent;

        var index = _previews.FindIndex(preview => preview.Green == green && preview.Shape is AOEShapeCircle);
        return index >= 0 ? _previews[index] : null;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_previews.Count == 0)
            return [];
        var count = Math.Min(2, _previews.Count);
        for (var i = 0; i < count; ++i)
        {
            var aoe = _previews[i].AOE;
            aoe.Color = i == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = i == 0;
            _active[i] = aoe;
        }
        return _active.AsSpan(0, count);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Module.FindComponent<HissingResonance>()?.HasStatus(pc.InstanceID) != true)
            base.DrawArenaForeground(pcSlot, pc);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (Module.FindComponent<HissingResonance>()?.HasStatus(actor.InstanceID) != true)
            base.AddMovementHints(slot, actor, movementHints);
    }

    public WPos? FindKnockbackPreparation(Actor actor, Preview circle, WDir direction)
    {
        if (FollowingShape(circle) is not Preview following)
            return null;

        var currentAOE = circle.AOE;
        var followingAOE = following.AOE;
        var earlierAOEs = _previews
            .Where(preview => preview.Activation <= circle.Activation && preview != circle)
            .Select(preview => preview.AOE)
            .ToArray();
        WPos? best = null;
        var bestDistance = float.MaxValue;
        for (var x = -18f; x <= 18f; x += 1f)
        {
            for (var z = -18f; z <= 18f; z += 1f)
            {
                var candidate = new WPos(MTH1GreenHeadBlueHead.ArenaCenter.X + x, MTH1GreenHeadBlueHead.ArenaCenter.Z + z);
                var destination = candidate + 10f * direction;
                if (!SafeSpot.Safe(candidate, currentAOE)
                    || !SafeSpot.Safe(candidate, earlierAOEs)
                    || !SafeSpot.Safe(destination, followingAOE))
                    continue;

                var distance = (candidate - actor.Position).LengthSq();
                if (distance < bestDistance)
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }
        }
        return best;
    }

    public bool DestinationUnsafe(WPos destination, Preview circle) =>
        !SafeSpot.InBounds(destination) || FollowingShape(circle) is Preview following && following.AOE.Check(destination);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.Crossblaze or (uint)AID.Blazeloop)
        {
            var shapeType = id == (uint)AID.Crossblaze ? typeof(AOEShapeCross) : typeof(AOEShapeDonut);
            var index = Find(shapeType, spell.LocXZ);
            if (index >= 0)
            {
                var preview = _previews[index];
                _previews[index] = preview with { Activation = Module.CastFinishAt(spell) };
                _previews.Sort((left, right) => left.Activation.CompareTo(right.Activation));
            }
            return;
        }

        if (TryPattern(id, out var first, out var second))
        {
            var pattern = new Pattern(first, second);
            if (caster.OID == (uint)OID.GreenHead)
                _greenPattern = pattern;
            else if (caster.OID == (uint)OID.BlueHead)
                _bluePattern = pattern;
            return;
        }

        var green = caster.OID == (uint)OID.GreenHeadMechanic;
        if (!green && caster.OID != (uint)OID.BlueHeadMechanic)
            return;
        if (id is not ((uint)AID.BlazeSequenceFirst) and not ((uint)AID.BlazeSequenceSecond) and not ((uint)AID.BlazeFollowing))
            return;

        if (id == (uint)AID.BlazeSequenceFirst)
        {
            _greenPattern?.Reset();
            _bluePattern?.Reset();
            _previews.Clear();
            _resolvedGreenCircle = null;
            _resolvedBlueCircle = null;
            _firstGreen = green;
        }

        var patternForHead = green ? _greenPattern : _bluePattern;
        if (patternForHead == null || patternForHead.Next >= patternForHead.Shapes.Length)
            return;

        var sequence = id switch
        {
            (uint)AID.BlazeSequenceFirst => 1,
            (uint)AID.BlazeSequenceSecond => 2,
            _ => green == _firstGreen ? 3 : 4
        };
        var activation = Module.CastFinishAt(spell);
        _previews.Add(new(Circle, spell.LocXZ, activation, green, sequence));
        _previews.Add(new(patternForHead.Shapes[patternForHead.Next++], spell.LocXZ, activation.AddSeconds(2.05d), green, sequence));
        _previews.Sort((left, right) =>
        {
            var activationOrder = left.Activation.CompareTo(right.Activation);
            return activationOrder != 0 ? activationOrder : left.Sequence.CompareTo(right.Sequence);
        });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var shapeType = spell.Action.ID switch
        {
            (uint)AID.BlazeCircleBlue or (uint)AID.BlazeCircleGreen or (uint)AID.BlazeCircleFollowing => typeof(AOEShapeCircle),
            (uint)AID.Crossblaze => typeof(AOEShapeCross),
            (uint)AID.Blazeloop => typeof(AOEShapeDonut),
            _ => null
        };
        if (shapeType == null)
            return;

        var index = Find(shapeType, caster.Position);
        if (index >= 0)
        {
            if (_previews[index].Shape is AOEShapeCircle)
            {
                if (_previews[index].Green)
                    _resolvedGreenCircle = _previews[index];
                else
                    _resolvedBlueCircle = _previews[index];
            }
            _previews.RemoveAt(index);
        }
        ++NumCasts;
    }

    private int Find(Type shapeType, WPos position)
    {
        var index = _previews.FindIndex(preview => preview.Shape.GetType() == shapeType && (preview.Origin - position).LengthSq() < 1f);
        return index >= 0 ? index : _previews.FindIndex(preview => preview.Shape.GetType() == shapeType);
    }

    private Preview? FollowingShape(Preview circle)
    {
        var index = _previews.FindIndex(preview =>
            preview.Sequence == circle.Sequence
            && preview.Green == circle.Green
            && preview.Shape is not AOEShapeCircle);
        return index >= 0 ? _previews[index] : null;
    }

    private static bool TryPattern(uint aid, out AOEShape first, out AOEShape second)
    {
        (AOEShape First, AOEShape Second)? pattern = aid switch
        {
            (uint)AID.BlazeloopAndRepeat => (Donut, Donut),
            (uint)AID.CrossblazeBlazeloop => (Cross, Donut),
            (uint)AID.CrossblazeAndRepeat => (Cross, Cross),
            (uint)AID.BlazeloopCrossblaze => (Donut, Cross),
            _ => null
        };
        (first, second) = pattern ?? (null!, null!);
        return pattern != null;
    }
}

sealed class HissingResonance(BossModule module) : Components.GenericKnockback(module)
{
    private readonly Dictionary<ulong, uint> _statuses = [];
    private readonly Knockback[] _source = new Knockback[1];
    private readonly FourfoldBlaze _fourfold = module.FindComponent<FourfoldBlaze>()!;

    public bool HasStatus(ulong actorID) => _statuses.ContainsKey(actorID);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (!_statuses.TryGetValue(actor.InstanceID, out var status)
            || _fourfold.CircleForKnockback(IsGreen(status)) is not FourfoldBlaze.Preview circle)
            return [];

        var direction = Direction(status);
        _source[0] = new(MTH1GreenHeadBlueHead.ArenaCenter, 10f, circle.Activation.AddSeconds(0.4d), direction: Angle.FromDirection(direction), kind: Kind.DirForward);
        return _source;
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) =>
        _statuses.TryGetValue(actor.InstanceID, out var status)
        && _fourfold.CircleForKnockback(IsGreen(status)) is FourfoldBlaze.Preview circle
        && _fourfold.DestinationUnsafe(pos, circle);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_statuses.TryGetValue(pc.InstanceID, out var status)
            && _fourfold.CircleForKnockback(IsGreen(status)) is FourfoldBlaze.Preview circle
            && _fourfold.FindKnockbackPreparation(pc, circle, Direction(status)) is WPos preparation)
        {
            Arena.ZoneCircleOutline(preparation, 1.2f, Colors.Safe, 2f);
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (_statuses.TryGetValue(actor.InstanceID, out var status)
            && _fourfold.CircleForKnockback(IsGreen(status)) is FourfoldBlaze.Preview circle
            && _fourfold.FindKnockbackPreparation(actor, circle, Direction(status)) is WPos preparation
            && (preparation - actor.Position).LengthSq() > 2.25f)
        {
            movementHints.Add((actor.Position, preparation, Colors.Safe));
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is >= (uint)SID.GreenNoiseEasterly and <= (uint)SID.BlueNoiseWesterly)
            _statuses[actor.InstanceID] = status.ID;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (_statuses.GetValueOrDefault(actor.InstanceID) == status.ID)
            _statuses.Remove(actor.InstanceID);
    }

    private static bool IsGreen(uint status) => status is (uint)SID.GreenNoiseEasterly or (uint)SID.GreenNoiseWesterly;

    private static WDir Direction(uint status) => status switch
    {
        (uint)SID.GreenNoiseEasterly or (uint)SID.BlueNoiseEasterly => (-90f).Degrees().ToDirection(),
        _ => 90f.Degrees().ToDirection()
    };
}

sealed class ArcaneRevelationAOEs(BossModule module) : PredictiveAOEs(module)
{
    private sealed class Group(DateTime activation)
    {
        public readonly DateTime Activation = activation;
        public readonly List<AOEInstance> AOEs = [];
    }

    private static readonly AOEShapeRect Matrix = new(60f, 2.5f, 60f);
    private static readonly AOEShapeRect TerrorsWide = new(40f, 10f);
    private static readonly AOEShapeRect TerrorsNarrow = new(40f, 5f);
    private static readonly AOEShapeDonut Donut = new(20f, 60f);
    private static readonly AOEShapeCircle Circle = new(20f);
    private readonly List<Group> _groups = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) =>
        _groups.Count != 0 ? CollectionsMarshal.AsSpan(_groups[0].AOEs) : [];

    public override void Update()
    {
        _groups.RemoveAll(group => WorldState.CurrentTime > group.Activation.AddSeconds(1d));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.TwoTerrorsWide or (uint)AID.TwoTerrorsNarrow)
        {
            var activation = Module.CastFinishAt(spell);
            var group = GroupAt(activation);
            group.AOEs.Add(new(id == (uint)AID.TwoTerrorsWide ? TerrorsWide : TerrorsNarrow, caster.Position, spell.Rotation, activation));
            if (id == (uint)AID.TwoTerrorsWide)
                AddMatrices(group, caster.NameID == 14490 ? (uint)OID.GreenArcaneMatrix : (uint)OID.BlueArcaneMatrix);
            return;
        }

        AOEShape? shape = id switch
        {
            (uint)AID.FulgurousFugueVisual2 or (uint)AID.FulgurousFugueVisual3 => Donut,
            (uint)AID.FreezingFugueVisual2 or (uint)AID.FreezingFugueVisual3 => Circle,
            _ => null
        };
        if (shape == null)
            return;

        var green = caster.OID == (uint)OID.GreenHead;
        if (!green && caster.OID != (uint)OID.BlueHead)
            return;

        var activation2 = Module.CastFinishAt(spell);
        var group2 = GroupAt(activation2);
        var boss = Module.Enemies((uint)OID.Boss).FirstOrDefault(actor => !actor.IsDestroyed);
        group2.AOEs.Add(new(shape, boss?.Position ?? new WPos(-899.7f, 680.3f), activation: activation2));
        AddMatrices(group2, green ? (uint)OID.GreenArcaneMatrix : (uint)OID.BlueArcaneMatrix);
    }

    private Group GroupAt(DateTime activation)
    {
        var group = _groups.FirstOrDefault(group => Math.Abs((group.Activation - activation).TotalSeconds) < 0.75d);
        if (group != null)
            return group;

        group = new(activation);
        _groups.Add(group);
        _groups.Sort((left, right) => left.Activation.CompareTo(right.Activation));
        return group;
    }

    private void AddMatrices(Group group, uint oid)
    {
        foreach (var matrix in Module.Enemies(oid))
        {
            if (!matrix.IsDestroyed)
                group.AOEs.Add(new(Matrix, matrix.Position, matrix.Rotation, group.Activation, actorID: matrix.InstanceID));
        }
    }
}

sealed class ElementalSummonAOEs(BossModule module) : PredictiveAOEs(module)
{
    private sealed class Round(int sequence, DateTime activation)
    {
        public readonly int Sequence = sequence;
        public readonly DateTime Activation = activation;
        public readonly List<AOEInstance> AOEs = [];
    }

    private static readonly AOEShapeCircle Cluster = new(15f);
    private static readonly AOEShapeCone Wave = new(45f, 30f.Degrees());
    private readonly Dictionary<ulong, bool> _markerIsGreen = [];
    private readonly List<(Actor Marker, int Sequence)> _pending = [];
    private readonly List<Round> _rounds = [];
    private DateTime _firstActivation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) =>
        _rounds.Count != 0 ? CollectionsMarshal.AsSpan(_rounds[0].AOEs) : [];

    public override void Update()
    {
        for (var i = _pending.Count - 1; i >= 0; --i)
        {
            var pending = _pending[i];
            if (_markerIsGreen.TryGetValue(pending.Marker.InstanceID, out var green))
            {
                AddPrediction(pending.Marker, pending.Sequence, green);
                _pending.RemoveAt(i);
            }
        }
        _rounds.RemoveAll(round => WorldState.CurrentTime > round.Activation.AddSeconds(1d));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Summon)
        {
            _markerIsGreen.Clear();
            _pending.Clear();
            _rounds.Clear();
            _firstActivation = default;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.Mechanic)
            return;
        if (source.OID == (uint)OID.GreenHeadMechanic)
            _markerIsGreen[tether.Target] = true;
        else if (source.OID == (uint)OID.BlueHeadMechanic)
            _markerIsGreen[tether.Target] = false;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (actor.OID != (uint)OID.SequenceMarker || iconID is < (uint)IconID.Sequence1 or > (uint)IconID.Sequence4)
            return;

        var sequence = (int)(iconID - (uint)IconID.Sequence1);
        if (sequence == 0 && _firstActivation == default)
            _firstActivation = WorldState.FutureTime(17.5d);
        if (_markerIsGreen.TryGetValue(actor.InstanceID, out var green))
            AddPrediction(actor, sequence, green);
        else
            _pending.Add((actor, sequence));
    }

    private void AddPrediction(Actor marker, int sequence, bool green)
    {
        if (_firstActivation == default)
            return;
        var activation = _firstActivation.AddSeconds(sequence * 3.65d);
        var round = _rounds.FirstOrDefault(round => round.Sequence == sequence);
        if (round == null)
        {
            round = new(sequence, activation);
            _rounds.Add(round);
            _rounds.Sort((left, right) => left.Sequence.CompareTo(right.Sequence));
        }

        round.AOEs.Add(new(Cluster, marker.Position, activation: activation, actorID: marker.InstanceID));
        var orbOID = green ? (uint)OID.LightningOrb : (uint)OID.IceOrb;
        var orb = Module.Enemies(orbOID).Where(actor => !actor.IsDestroyed).MinBy(actor => (actor.Position - marker.Position).LengthSq());
        if (orb != null)
            round.AOEs.Add(new(Wave, orb.Position, orb.Rotation, activation, actorID: orb.InstanceID));
    }
}

sealed class Archaeofury(BossModule module) : Components.UniformStackSpread(module, 0f, 6f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Archaeofury)
            AddSpread(actor, WorldState.FutureTime(5.1d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ArchaeofuryGreen or (uint)AID.ArchaeofuryBlue)
            Spreads.RemoveAll(spread => spread.Target.InstanceID == spell.MainTargetID);
    }
}

sealed class ThunderfrostTempest(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThunderfrostTempest);
sealed class Enrage(BossModule module) : Components.RaidwideCast(module, (uint)AID.Enrage);

sealed class MTH1GreenHeadBlueHeadStates : StateMachineBuilder
{
    public MTH1GreenHeadBlueHeadStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BuffetAssignments>()
            .ActivateOnEnter<ThunderfrostTempest>()
            .ActivateOnEnter<Enrage>()
            .ActivateOnEnter<ComboAOEs>()
            .ActivateOnEnter<StormsBreath>()
            .ActivateOnEnter<FourfoldBlaze>()
            .ActivateOnEnter<HissingResonance>()
            .ActivateOnEnter<ArcaneRevelationAOEs>()
            .ActivateOnEnter<ElementalSummonAOEs>()
            .ActivateOnEnter<Archaeofury>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(MTH1GreenHeadBlueHeadStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.GreenHead,
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1114,
    NameID = 14490,
    SortOrder = 1)]
public sealed class MTH1GreenHeadBlueHead(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsSquare(20f))
{
    public static readonly WPos ArenaCenter = new(-900f, 700f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.GreenHead));
        Arena.Actors(Enemies((uint)OID.BlueHead));
    }
}
