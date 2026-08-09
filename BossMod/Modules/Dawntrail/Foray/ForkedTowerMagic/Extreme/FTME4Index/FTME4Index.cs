namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME4Index;

public enum OID : uint
{
    Boss = 0x4B67,
    Bomb = 0x4B68,
    Bird = 0x4B69,
    HolyLance = 0x4B6A,
    PropheticPhenomenon = 0x4B6B,
    IceBall = 0x4B6C,
    FireBall = 0x4B6D,
    ThunderBall = 0x4B6E,
    CataloguePhantom = 0x4B6F,
    IntegrationIceBall = 0x4E03,
    IntegrationFireBall = 0x4E04,
    IntegrationThunderBall = 0x4E05,
    FirePlatforms = 0x1EC008,
    IcePlatforms = 0x1EC009,
    ThunderPlatforms = 0x1EC00A,
    FireFloorWarning = 0x1EC00B,
    IceFloorWarning = 0x1EC00C,
    ThunderFloorWarning = 0x1EC00D,
    Helper = 0x233C
}

public enum AID : uint
{
    SealedImplements = 48384,
    SealedImplements2 = 48386,
    ElementalControl = 48394,
    ElementalDeployment = 48399,
    ElementalCreation = 48400,
    FlyingOrder = 48403,
    Jump = 48404,
    Dualcast = 48407,
    Summon = 48408,
    Prophecy = 48412,
    FlareVisual = 48415,
    FlareFollowupVisual = 48416,
    RomeosBallad = 48422,
    Aim = 48423,
    ElementalControlDamage = 48427,
    FirePlatforms = 48428,
    IcePlatforms = 48429,
    ThunderPlatforms = 48430,
    FireLine = 48431,
    IceLine = 48432,
    ThunderLine = 48433,
    ElementalIntegration = 48434,
    ElementAbsorption = 48435,
    ElementalIntegrationDamage = 48436,
    FireBall = 48437,
    IceBall = 48438,
    ThunderBall = 48439,
    FireBallImpact = 48441,
    IceBallImpact = 48442,
    ThunderBallImpact = 48443,
    MagicSwordPetrify = 48444,
    GroundFireFirst = 48445,
    GroundFireRepeat = 48446,
    Shockwave = 48447,
    FlyingFeatherWind = 48450,
    Meteor = 48451,
    Starfall = 48452,
    AllSlashVisual = 48453,
    AllSlashVisual2 = 48454,
    AllSlash = 48455,
    Flare = 48456,
    AllKnowingHellfire = 48458,
    AllKnowingSpread = 48459,
    FourfoldSealedImplements = 48906,
    FourfoldSealedImplements2 = 48909,
    FourfoldIntermediate1 = 48910,
    FourfoldIntermediate2 = 48911,
    Harp = 48912,
    Bow = 48913,
    Knife = 48914,
    Bell = 48915,
    IntegrationLongCast = 48916,
    FourfoldIntermediate3 = 50363,
    FourfoldIntermediate4 = 50364,
    AllKnowingFlames = 50472,
    ElementalIntegrationDamage2 = 50934
}

public enum SID : uint
{
    ProphecyShape = 2552, // Extra 1101 = circle, 1100 = donut
    FireResistanceDown = 2902,
    IceResistanceDown = 2903,
    ThunderResistanceDown = 2998,
    BellOrder = 5532,
    KnifeOrder = 5533,
    BowOrder = 5534,
    HarpOrder = 5535
}

public enum IconID : uint
{
    AllKnowingTankbuster = 344,
    AllKnowingSpread = 466,
    ThunderSafe = 670,
    FireSafe = 671,
    IceSafe = 672
}

enum Element
{
    None,
    Fire,
    Ice,
    Thunder
}

enum Weapon
{
    None,
    Harp,
    Bow,
    Knife,
    Bell
}

sealed class Flare(BossModule module) : Components.RaidwideCastDelay(module,
    (uint)AID.FlareVisual, (uint)AID.Flare, 0.8d, "全场伤害 1/2");

sealed class FlareFollowup(BossModule module) : Components.RaidwideInstant(module,
    (uint)AID.Flare, 0.8d, "全场伤害 2/2")
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FlareFollowupVisual)
            Activation = WorldState.FutureTime(0.8d);
        base.OnEventCast(caster, spell);
    }
}

sealed class ElementalControl(BossModule module) : Components.RaidwideCast(module, (uint)AID.ElementalControl);
sealed class ElementalIntegration(BossModule module) : Components.RaidwideCast(module, (uint)AID.ElementalIntegration);
sealed class RomeosBallad(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RomeosBallad, 15f);
sealed class Aim(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Aim, 11f);
sealed class Meteor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Meteor, 10f);
sealed class Starfall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Starfall, new AOEShapeDonut(3f, 15f));
sealed class AllKnowingTankbuster(BossModule module) : Components.SpreadFromIcon(module,
    (uint)IconID.AllKnowingTankbuster, (uint)AID.AllKnowingHellfire, 6f, 5.1d);
sealed class AllKnowingSpread(BossModule module) : Components.SpreadFromIcon(module,
    (uint)IconID.AllKnowingSpread, (uint)AID.AllKnowingSpread, 6f, 5.1d);

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0)
            return;

        if (state == 0x00020001)
            SetBounds(Index.ExpandedBounds);
        else if (state == 0x00080004)
            SetBounds(Index.InitialBounds);
    }

    private void SetBounds(ArenaBoundsCustom bounds)
    {
        Arena.Bounds = bounds;
        Arena.Center = bounds.Center;
    }
}

sealed class FourfoldWeapons(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle HarpShape = new(15f);
    private static readonly AOEShapeCircle BowShape = new(11f);
    private static readonly AOEShapeCone ConeShape = new(30f, 30f.Degrees());
    private static readonly Angle[] FrontAngles = [0f.Degrees(), 120f.Degrees(), -120f.Degrees()];
    private static readonly Angle[] SideAngles = [180f.Degrees(), 60f.Degrees(), -60f.Degrees()];
    private readonly List<Weapon> _sequence = new(4);
    private readonly List<AOEInstance> _aoes = new(12);
    private DateTime _firstActivation;
    private int _resolved;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        for (var i = _resolved; i < _sequence.Count; ++i)
        {
            var current = i == _resolved;
            var color = current ? Colors.Danger : Colors.AOE;
            var activation = _firstActivation == default ? default : _firstActivation.AddSeconds(3.15d * i);
            AddWeapon(_sequence[i], activation, color, current);
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_resolved < _sequence.Count)
            hints.Add($"封印武器顺序：{string.Join(" → ", _sequence.Skip(_resolved).Select(WeaponName))}", false);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FourfoldSealedImplements or (uint)AID.FourfoldSealedImplements2)
        {
            _sequence.Clear();
            _resolved = 0;
            _firstActivation = Module.CastFinishAt(spell).AddSeconds(2.1d);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor != Module.PrimaryActor || _sequence.Count >= 4)
            return;

        var weapon = status.ID switch
        {
            (uint)SID.HarpOrder => Weapon.Harp,
            (uint)SID.BowOrder => Weapon.Bow,
            (uint)SID.KnifeOrder => Weapon.Knife,
            (uint)SID.BellOrder => Weapon.Bell,
            _ => Weapon.None
        };
        if (weapon != Weapon.None)
            _sequence.Add(weapon);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var weapon = spell.Action.ID switch
        {
            (uint)AID.Harp => Weapon.Harp,
            (uint)AID.Bow => Weapon.Bow,
            (uint)AID.Knife => Weapon.Knife,
            (uint)AID.Bell => Weapon.Bell,
            _ => Weapon.None
        };
        if (weapon != Weapon.None && _resolved < _sequence.Count && _sequence[_resolved] == weapon)
        {
            ++_resolved;
            ++NumCasts;
        }
    }

    private void AddWeapon(Weapon weapon, DateTime activation, uint color, bool risky)
    {
        switch (weapon)
        {
            case Weapon.Harp:
                _aoes.Add(new(HarpShape, Index.ArenaCenter, activation: activation, color: color, risky: risky));
                break;
            case Weapon.Bow:
                foreach (var angle in FrontAngles)
                    _aoes.Add(new(BowShape, Index.ArenaCenter + 20.5f * angle.ToDirection(), activation: activation, color: color, risky: risky));
                break;
            case Weapon.Knife:
                foreach (var angle in FrontAngles)
                    _aoes.Add(new(ConeShape, Index.ArenaCenter, angle, activation, color, risky));
                break;
            case Weapon.Bell:
                foreach (var angle in SideAngles)
                    _aoes.Add(new(ConeShape, Index.ArenaCenter, angle, activation, color, risky));
                break;
        }
    }

    private static string WeaponName(Weapon weapon) => weapon switch
    {
        Weapon.Harp => "琴·外",
        Weapon.Bow => "弓·内",
        Weapon.Knife => "刀·侧",
        Weapon.Bell => "铃·正",
        _ => "未知"
    };
}

sealed class ElementSafePlatforms(BossModule module) : BossComponent(module)
{
    private readonly Dictionary<ulong, Element> _playerSafeElements = [];
    private readonly HashSet<Element> _resolved = [];
    private readonly HashSet<Element> _pair = [];
    private DateTime _lastPairEvent;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var safe = PlayerSafeElement(pc);
        if (safe == Element.None || _resolved.Contains(safe))
            return;

        var marker = Module.Enemies(MarkerOID(safe)).FirstOrDefault(actor => !actor.IsDestroyed);
        if (marker == null)
            return;

        DrawPlatform(marker.Rotation);
        DrawPlatform(marker.Rotation + 180f.Degrees());
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var safe = PlayerSafeElement(actor);
        if (safe != Element.None && !_resolved.Contains(safe))
            hints.Add($"绿色：{ElementName(safe)}属性安全平台", false);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var safe = iconID switch
        {
            (uint)IconID.ThunderSafe => Element.Thunder,
            (uint)IconID.FireSafe => Element.Fire,
            (uint)IconID.IceSafe => Element.Ice,
            _ => Element.None
        };
        if (safe != Element.None)
            _playerSafeElements[actor.InstanceID] = safe;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var element = spell.Action.ID switch
        {
            (uint)AID.FirePlatforms => Element.Fire,
            (uint)AID.IcePlatforms => Element.Ice,
            (uint)AID.ThunderPlatforms => Element.Thunder,
            _ => Element.None
        };
        if (element == Element.None)
            return;

        if ((WorldState.CurrentTime - _lastPairEvent).TotalSeconds > 0.5d)
            _pair.Clear();
        _lastPairEvent = WorldState.CurrentTime;
        _pair.Add(element);

        if (_pair.Count != 2)
            return;

        var safe = Element.Fire;
        if (_pair.Contains(Element.Fire))
            safe = _pair.Contains(Element.Ice) ? Element.Thunder : Element.Ice;
        _resolved.Add(safe);
        _pair.Clear();

        if (_resolved.Count == 3)
        {
            _playerSafeElements.Clear();
            _resolved.Clear();
        }
    }

    private Element PlayerSafeElement(Actor actor) => _playerSafeElements.GetValueOrDefault(actor.InstanceID);

    private void DrawPlatform(Angle angle)
    {
        IndexArenaBounds.PlatformRegion(Index.ArenaCenter, angle)
            .Draw(Arena, Index.ArenaCenter, default(Angle), Colors.SafeFromAOE);
    }

    private static uint MarkerOID(Element element) => element switch
    {
        Element.Fire => (uint)OID.FirePlatforms,
        Element.Ice => (uint)OID.IcePlatforms,
        Element.Thunder => (uint)OID.ThunderPlatforms,
        _ => 0u
    };

    private static string ElementName(Element element) => element switch
    {
        Element.Fire => "火",
        Element.Ice => "冰",
        Element.Thunder => "雷",
        _ => "未知"
    };
}

sealed class ElementalDance(BossModule module) : Components.GenericAOEs(module)
{
    private sealed class Prediction(Element element, DateTime activation, bool rotating, ulong actorID)
    {
        public readonly Element Element = element;
        public readonly DateTime Activation = activation;
        public readonly bool Rotating = rotating;
        public readonly ulong ActorID = actorID;
    }

    private readonly List<Prediction> _predictions = [];
    private readonly List<AOEInstance> _aoes = new(8);
    private Element _lastResolved;
    private DateTime _lastResolvedAt;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _predictions.RemoveAll(prediction => prediction.Activation < WorldState.CurrentTime.AddSeconds(-2d));
        _aoes.Clear();
        if (_predictions.Count == 0)
            return [];

        var ordered = _predictions.OrderBy(prediction => prediction.Activation).ToList();
        var currentActivation = ordered[0].Activation;
        AddGroup(ordered.Where(prediction => Math.Abs((prediction.Activation - currentActivation).TotalSeconds) < 0.75d),
            currentActivation, Colors.Danger, true);

        var next = ordered.FirstOrDefault(prediction => (prediction.Activation - currentActivation).TotalSeconds >= 0.75d);
        if (next != null)
            AddGroup(ordered.Where(prediction => Math.Abs((prediction.Activation - next.Activation).TotalSeconds) < 0.75d),
                next.Activation, Colors.AOE, false);
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var current = CurrentGroup();
        var elements = current.Select(prediction => prediction.Element).Distinct().ToList();
        if (elements.Count == 2)
        {
            var safe = MissingElement(elements[0], elements[1]);
            DrawSafePlatform(safe);
        }
        base.DrawArenaBackground(pcSlot, pc);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        var elements = CurrentGroup().Select(prediction => prediction.Element).Distinct().ToList();
        if (elements.Count == 2)
            hints.Add($"绿色：{ElementName(MissingElement(elements[0], elements[1]))}属性唯一安全平台", false);
    }

    public override void OnActorCreated(Actor actor)
    {
        var floor = actor.OID switch
        {
            (uint)OID.FireFloorWarning => Element.Fire,
            (uint)OID.IceFloorWarning => Element.Ice,
            (uint)OID.ThunderFloorWarning => Element.Thunder,
            _ => Element.None
        };
        if (floor != Element.None)
        {
            _predictions.Add(new(floor, WorldState.FutureTime(6.75d), false, actor.InstanceID));
            return;
        }

        var rotating = BallElement(actor.OID);
        if (rotating == Element.None || _predictions.Any(prediction =>
                prediction.Rotating && prediction.Element == rotating &&
                Math.Abs((prediction.Activation - WorldState.CurrentTime).TotalSeconds) < 20d))
            return;

        var marker = Module.Enemies(MarkerOID(rotating)).FirstOrDefault(marker => !marker.IsDestroyed);
        if (marker == null)
            return;

        var start = Angle.FromDirection(actor.Position - Index.ArenaCenter);
        var travel = Math.Min(ClockwiseDistance(start, marker.Rotation),
            ClockwiseDistance(start, marker.Rotation + 180f.Degrees()));
        _predictions.Add(new(rotating, WorldState.FutureTime(6.25d + 4.25d * travel), true, actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var element = spell.Action.ID switch
        {
            (uint)AID.FireLine => Element.Fire,
            (uint)AID.IceLine => Element.Ice,
            (uint)AID.ThunderLine => Element.Thunder,
            _ => Element.None
        };
        if (element == Element.None)
            return;

        if (_lastResolved == element && (WorldState.CurrentTime - _lastResolvedAt).TotalSeconds < 0.5d)
            return;
        _lastResolved = element;
        _lastResolvedAt = WorldState.CurrentTime;

        var prediction = _predictions
            .Where(prediction => prediction.Element == element)
            .MinBy(prediction => Math.Abs((prediction.Activation - WorldState.CurrentTime).TotalSeconds));
        if (prediction != null)
        {
            _predictions.Remove(prediction);
            ++NumCasts;
        }
    }

    private IEnumerable<Prediction> CurrentGroup()
    {
        if (_predictions.Count == 0)
            return [];
        var activation = _predictions.Min(prediction => prediction.Activation);
        return _predictions.Where(prediction => Math.Abs((prediction.Activation - activation).TotalSeconds) < 0.75d);
    }

    private void AddGroup(IEnumerable<Prediction> predictions, DateTime activation, uint color, bool risky)
    {
        foreach (var element in predictions.Select(prediction => prediction.Element).Distinct())
        {
            var marker = Module.Enemies(MarkerOID(element)).FirstOrDefault(marker => !marker.IsDestroyed);
            if (marker == null)
                continue;
            AddPlatform(marker.Rotation, activation, color, risky);
            AddPlatform(marker.Rotation + 180f.Degrees(), activation, color, risky);
        }
    }

    private void DrawSafePlatform(Element element)
    {
        var marker = Module.Enemies(MarkerOID(element)).FirstOrDefault(marker => !marker.IsDestroyed);
        if (marker == null)
            return;
        Draw(marker.Rotation);
        Draw(marker.Rotation + 180f.Degrees());

        void Draw(Angle angle)
        {
            IndexArenaBounds.PlatformRegion(Index.ArenaCenter, angle)
                .Draw(Arena, Index.ArenaCenter, default(Angle), Colors.SafeFromAOE);
        }
    }

    private void AddPlatform(Angle angle, DateTime activation, uint color, bool risky)
    {
        var shape = IndexArenaBounds.PlatformRegion(Index.ArenaCenter, angle);
        _aoes.Add(new(shape, Index.ArenaCenter, default(Angle), activation, color, risky));
    }

    private static Element BallElement(uint oid) => oid switch
    {
        (uint)OID.FireBall => Element.Fire,
        (uint)OID.IceBall => Element.Ice,
        (uint)OID.ThunderBall => Element.Thunder,
        _ => Element.None
    };

    private static uint MarkerOID(Element element) => element switch
    {
        Element.Fire => (uint)OID.FirePlatforms,
        Element.Ice => (uint)OID.IcePlatforms,
        Element.Thunder => (uint)OID.ThunderPlatforms,
        _ => 0u
    };

    private static Element MissingElement(Element first, Element second)
        => first != Element.Fire && second != Element.Fire
            ? Element.Fire
            : first != Element.Ice && second != Element.Ice
                ? Element.Ice
                : Element.Thunder;

    private static string ElementName(Element element) => element switch
    {
        Element.Fire => "火",
        Element.Ice => "冰",
        Element.Thunder => "雷",
        _ => "未知"
    };

    private static double ClockwiseDistance(Angle start, Angle target)
    {
        var distance = (start - target).Normalized().Rad;
        return distance >= 0f ? distance : distance + Angle.DoublePI;
    }
}

sealed class Prophecy(BossModule module) : Components.GenericAOEs(module)
{
    private sealed class Prediction(AOEShape shape, Angle startAngle, WPos destination, DateTime activation)
    {
        public readonly AOEShape Shape = shape;
        public readonly Angle StartAngle = startAngle;
        public WPos Destination = destination;
        public readonly DateTime Activation = activation;
        public bool DirectionConfirmed;
    }

    private static readonly AOEShapeCircle Circle = new(10f);
    private static readonly AOEShapeDonut Donut = new(3f, 15f);
    private readonly Dictionary<ulong, Prediction> _predictions = [];
    private readonly List<AOEInstance> _aoes = new(3);

    public override void Update()
    {
        foreach (var (actorID, prediction) in _predictions)
        {
            if (prediction.DirectionConfirmed || WorldState.Actors.Find(actorID) is not Actor actor)
                continue;

            var currentAngle = Angle.FromDirection(actor.Position - Index.ArenaCenter);
            var movement = (currentAngle - prediction.StartAngle).Normalized();
            if (Math.Abs(movement.Deg) < 1f)
                continue;

            prediction.DirectionConfirmed = true;
            if (movement.Rad > 0f)
                prediction.Destination = Destination(prediction.StartAngle, false);
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        foreach (var (actorID, prediction) in _predictions)
            _aoes.Add(new(prediction.Shape, prediction.Destination, activation: prediction.Activation, actorID: actorID));
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor.OID != (uint)OID.PropheticPhenomenon || status.ID != (uint)SID.ProphecyShape || _predictions.ContainsKey(actor.InstanceID))
            return;

        AOEShape? shape = status.Extra switch
        {
            1101 => Circle,
            1100 => Donut,
            _ => null
        };
        if (shape == null)
            return;

        var startAngle = Angle.FromDirection(actor.Position - Index.ArenaCenter);
        _predictions[actor.InstanceID] = new(shape, startAngle, Destination(startAngle, true), WorldState.FutureTime(10d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((spell.Action.ID == (uint)AID.Meteor || spell.Action.ID == (uint)AID.Starfall) && _predictions.Remove(caster.InstanceID))
            ++NumCasts;
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.PropheticPhenomenon)
            _predictions.Remove(actor.InstanceID);
    }

    private static WPos Destination(Angle startAngle, bool clockwise)
        => Index.ArenaCenter + 15.5f * (startAngle + (clockwise ? -60f : 60f).Degrees()).ToDirection();
}

sealed class Shockwave(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _sources = new(6);
    private readonly Knockback[] _nearest = new Knockback[1];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_sources.Count == 0)
            return [];

        var nearest = _sources[0];
        var nearestDistance = (actor.Position - nearest.Origin).LengthSq();
        for (var i = 1; i < _sources.Count; ++i)
        {
            var distance = (actor.Position - _sources[i].Origin).LengthSq();
            if (distance < nearestDistance)
            {
                nearest = _sources[i];
                nearestDistance = distance;
            }
        }
        _nearest[0] = nearest;
        return _nearest;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Shockwave)
            _sources.Add(new(caster.Position, 9f, Module.CastFinishAt(spell), actorID: caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Shockwave)
        {
            _sources.RemoveAll(source => source.ActorID == caster.InstanceID);
            ++NumCasts;
        }
    }
}

sealed class GroundFire(BossModule module) : Components.GenericAOEs(module)
{
    private sealed class Path(List<WPos> points, DateTime activation)
    {
        public readonly List<WPos> Points = points;
        public int Next;
        public DateTime Activation = activation;
    }

    private static readonly AOEShapeCircle Shape = new(6f);
    private static readonly WDir[] BasePath =
    [
        new(21.76f, -16.31f),
        new(16.56f, -13.31f),
        new(11.37f, -10.31f),
        new(6.17f, -7.31f),
        new(0f, -9f),
        new(-6.17f, -7.31f),
        new(-11.37f, -10.31f),
        new(-16.56f, -13.31f),
        new(-21.76f, -16.31f)
    ];
    private static readonly Angle[] Rotations = [0f.Degrees(), 120f.Degrees(), -120f.Degrees()];
    private readonly List<Path> _paths = new(3);
    private readonly List<AOEInstance> _aoes = new(6);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        foreach (var path in _paths)
        {
            for (var offset = 0; offset < 2 && path.Next + offset < path.Points.Count; ++offset)
            {
                var current = offset == 0;
                _aoes.Add(new(Shape, path.Points[path.Next + offset],
                    activation: path.Activation.AddSeconds(2d * offset),
                    color: current ? Colors.Danger : Colors.AOE,
                    risky: current));
            }
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GroundFireFirst)
        {
            var points = BestPath(caster.Position);
            if (points.Count > 1)
                _paths.Add(new(points, WorldState.FutureTime(2d)) { Next = 1 });
        }
        else if (spell.Action.ID == (uint)AID.GroundFireRepeat)
        {
            var path = _paths
                .Where(path => path.Next < path.Points.Count)
                .MinBy(path => (path.Points[path.Next] - caster.Position).LengthSq());
            if (path != null && (path.Points[path.Next] - caster.Position).LengthSq() < 4f)
            {
                ++path.Next;
                path.Activation = WorldState.FutureTime(2d);
                if (path.Next >= path.Points.Count)
                    _paths.Remove(path);
            }
        }
    }

    private static List<WPos> BestPath(WPos origin)
    {
        List<WPos>? best = null;
        var bestDistance = float.MaxValue;
        foreach (var rotation in Rotations)
        {
            var forward = BasePath.Select(offset => Index.ArenaCenter + offset.Rotate(rotation)).ToList();
            var reverse = forward.AsEnumerable().Reverse().ToList();
            Check(forward);
            Check(reverse);
        }
        return best ?? [];

        void Check(List<WPos> candidate)
        {
            var distance = (candidate[0] - origin).LengthSq();
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }
    }
}

sealed class AllSlash(BossModule module) : Components.GenericAOEs(module)
{
    private sealed class Group(DateTime startedAt, DateTime activation)
    {
        public readonly DateTime StartedAt = startedAt;
        public readonly DateTime Activation = activation;
        public readonly List<AOEInstance> AOEs = [];
        public bool Resolved;
    }

    private static readonly AOEShapeRect Shape = new(15f, 3.75f);
    private readonly List<Group> _groups = new(3);
    private readonly List<AOEInstance> _aoes = new(12);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        var first = _groups.FindIndex(group => !group.Resolved);
        if (first < 0)
            return [];

        for (var i = first; i < _groups.Count; ++i)
        {
            if (_groups[i].Resolved)
                continue;
            var current = i == first;
            foreach (var aoe in _groups[i].AOEs)
                _aoes.Add(new(aoe.Shape, aoe.Origin, aoe.Rotation, aoe.Activation,
                    current ? Colors.Danger : Colors.AOE, current, aoe.ActorID));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_groups.Count >= 3)
        {
            List<Shape>? positives = null;
            List<Shape>? dangers = null;
            if (!_groups[0].Resolved)
            {
                positives = ToShapes(_groups[2]);
                dangers = [.. ToShapes(_groups[0]), .. ToShapes(_groups[1])];
            }
            else if (!_groups[1].Resolved)
            {
                positives = ToShapes(_groups[0]);
                dangers = [.. ToShapes(_groups[1]), .. ToShapes(_groups[2])];
            }

            if (positives != null && dangers != null)
            {
                var safe = new AOEShapeCustom(positives, dangers, origin: Index.ArenaCenter);
                safe.Draw(Arena, Index.ArenaCenter, default(Angle), Colors.SafeFromAOE);
            }
        }
        base.DrawArenaBackground(pcSlot, pc);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.AllSlash)
            return;

        var activation = Module.CastFinishAt(spell);
        var startedAt = WorldState.CurrentTime;
        var group = _groups.FirstOrDefault(group => Math.Abs((group.StartedAt - startedAt).TotalSeconds) < 0.1d);
        if (group == null)
        {
            group = new(startedAt, activation);
            _groups.Add(group);
            _groups.Sort((left, right) => left.Activation.CompareTo(right.Activation));
        }
        group.AOEs.Add(new(Shape, caster.Position, spell.Rotation, activation, actorID: caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.AllSlash)
            return;

        var group = _groups.FirstOrDefault(group => !group.Resolved && group.AOEs.Any(aoe => aoe.ActorID == caster.InstanceID));
        if (group != null)
        {
            group.Resolved = true;
            ++NumCasts;
        }
        if (_groups.Count != 0 && _groups.All(group => group.Resolved))
            _groups.Clear();
    }

    private static List<Shape> ToShapes(Group group)
        => [.. group.AOEs.Select(aoe => (Shape)new Rectangle(
            aoe.Origin + 7.5f * aoe.Rotation.ToDirection(), 3.75f, 7.5f, aoe.Rotation))];
}

sealed class IntegrationFirstWave(BossModule module) : BossComponent(module)
{
    private const float InterceptRadius = 9f;
    private const float MarkerRadius = 2f;

    private sealed class Wave(DateTime created)
    {
        public readonly DateTime Created = created;
        public readonly List<Actor> Balls = [];
    }

    private readonly List<Wave> _waves = new(3);
    private readonly Dictionary<ulong, List<uint>> _statusOrder = [];

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var (wave, positions) = CurrentGuidance(pc);
        if (wave != null)
            foreach (var position in positions)
                Arena.ZoneCircle(position, MarkerRadius, Colors.SafeFromAOE);
        base.DrawArenaBackground(pcSlot, pc);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var (wave, positions) = CurrentGuidance(actor);
        if (wave == null)
            return;

        if (positions.Count != 0)
            hints.AddForbiddenZone(new SDInvertedUnion([.. positions.Select(position => new SDCircle(position, MarkerRadius))]),
                wave.Created.AddSeconds(6d));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentGuidance(actor).Wave != null)
            hints.Add("绿色：当前撞球位置；获得属性耐性后顺时针前往下一处", false);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (!IsIntegrationBall(actor.OID))
            return;

        var wave = _waves.LastOrDefault();
        if (wave == null || (WorldState.CurrentTime - wave.Created).TotalSeconds > 0.5d)
        {
            wave = new(WorldState.CurrentTime);
            _waves.Add(wave);
        }
        wave.Balls.Add(actor);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (!IsResistance(status.ID) || _waves.Count == 0)
            return;

        if (!_statusOrder.TryGetValue(actor.InstanceID, out var order))
            _statusOrder[actor.InstanceID] = order = new(3);
        if (order.Count < 3 && !order.Contains(status.ID))
            order.Add(status.ID);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ElementAbsorption)
        {
            _waves.Clear();
            _statusOrder.Clear();
        }
    }

    private (Wave? Wave, List<WPos> Positions) CurrentGuidance(Actor actor)
    {
        var order = _statusOrder.GetValueOrDefault(actor.InstanceID);
        var waveIndex = order?.Count ?? 0;
        if (waveIndex >= _waves.Count)
            return (null, []);

        var wave = _waves[waveIndex];
        if (waveIndex == 0 || order == null)
            return (wave, InterceptPositions(wave));

        var previous = _waves[waveIndex - 1].Balls.FirstOrDefault(ball => StatusMatchesBall(order[^1], ball.OID));
        if (previous == null || wave.Balls.Count == 0)
            return (wave, InterceptPositions(wave));

        var target = wave.Balls.MinBy(ball => ClockwiseDistance(previous, ball));
        return target != null ? (wave, [InterceptPosition(target)]) : (wave, []);
    }

    private static List<WPos> InterceptPositions(Wave wave)
        => [.. wave.Balls
            .Select(InterceptPosition)];

    private static WPos InterceptPosition(Actor ball)
        => Index.ArenaCenter + InterceptRadius * (ball.Position - Index.ArenaCenter).Normalized();

    private static float ClockwiseDistance(Actor previous, Actor next)
    {
        var previousAngle = Angle.FromDirection(previous.Position - Index.ArenaCenter);
        var nextAngle = Angle.FromDirection(next.Position - Index.ArenaCenter);
        var distance = (previousAngle - nextAngle).Normalized().Rad;
        return distance < 0f ? distance + MathF.Tau : distance;
    }

    private static bool StatusMatchesBall(uint statusID, uint oid)
        => statusID switch
        {
            (uint)SID.FireResistanceDown => oid == (uint)OID.IntegrationFireBall,
            (uint)SID.IceResistanceDown => oid == (uint)OID.IntegrationIceBall,
            (uint)SID.ThunderResistanceDown => oid == (uint)OID.IntegrationThunderBall,
            _ => false
        };

    private static bool IsResistance(uint statusID)
        => statusID is (uint)SID.FireResistanceDown or (uint)SID.IceResistanceDown or (uint)SID.ThunderResistanceDown;

    private static bool IsIntegrationBall(uint oid)
        => oid is (uint)OID.IntegrationIceBall or (uint)OID.IntegrationFireBall or (uint)OID.IntegrationThunderBall;
}

sealed class IndexStates : StateMachineBuilder
{
    public IndexStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<ElementSafePlatforms>()
            .ActivateOnEnter<ElementalDance>()
            .ActivateOnEnter<Flare>()
            .ActivateOnEnter<FlareFollowup>()
            .ActivateOnEnter<ElementalControl>()
            .ActivateOnEnter<ElementalIntegration>()
            .ActivateOnEnter<FourfoldWeapons>()
            .ActivateOnEnter<RomeosBallad>()
            .ActivateOnEnter<Aim>()
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<Starfall>()
            .ActivateOnEnter<Prophecy>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<AllKnowingTankbuster>()
            .ActivateOnEnter<AllKnowingSpread>()
            .ActivateOnEnter<GroundFire>()
            .ActivateOnEnter<AllSlash>()
            .ActivateOnEnter<IntegrationFirstWave>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(IndexStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093,
    NameID = 14717,
    SortOrder = 4)]
public sealed class Index(WorldState ws, Actor primary) : BossModule(ws, primary, InitialBounds.Center, InitialBounds)
{
    public static readonly WPos ArenaCenter = new(0f, -628f);
    public static readonly ArenaBoundsCustom InitialBounds = IndexArenaBounds.Initial(ArenaCenter);
    public static readonly ArenaBoundsCustom ExpandedBounds = IndexArenaBounds.Expanded(ArenaCenter);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Bomb));
        Arena.Actors(Enemies((uint)OID.Bird));
    }
}
