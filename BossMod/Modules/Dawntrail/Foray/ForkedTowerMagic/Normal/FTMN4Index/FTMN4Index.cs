namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN4Index;

public enum OID : uint
{
    Boss = 0x4B5F, // R7.5
    HolyLance = 0x4B62,
    PropheticPhenomenon = 0x4B63,
    IceBall = 0x4B64,
    FireBall = 0x4B65,
    ThunderBall = 0x4B66,
    CataloguePhantom = 0x4B6F,
    FirePlatforms = 0x1EC008,
    IcePlatforms = 0x1EC009,
    ThunderPlatforms = 0x1EC00A,
    Helper = 0x233C
}

public enum AID : uint
{
    Flare = 48415, // Boss->self, raidwide visual
    RomeosBallad = 48385, // Helper->self, range 15 circle
    Aim = 48387, // Helper->self, range 11 circle
    OmniElements = 48394, // Boss->self, raidwide
    FireIV = 48396, // Helper->self, damages both fire platforms
    BlizzardIV = 48397, // Helper->self, damages both ice platforms
    ThunderIV = 48398, // Helper->self, damages both thunder platforms
    ElementaryChemistry = 48905, // Helper->self, 15x15 rect
    ShockwaveVisual = 48405, // HolyLance->self, knockback 9
    Iainuki = 48389, // CataloguePhantom->self, range 30 60-degree cone
    WindSlash = 48391, // CataloguePhantom->self, range 30 60-degree cone
    AllConsumingFlames = 48420, // Helper->player, range 6 circle
    Starfall = 48413, // PropheticPhenomenon->self, range 10 circle
    Cleansing = 48414 // PropheticPhenomenon->self, range 3-15 donut
}

public enum SID : uint
{
    ProphecyShape = 2552 // Extra 1101 = circle, 1100 = donut
}

public enum IconID : uint
{
    AllConsumingFlames = 466
}

sealed class Flare(BossModule module) : Components.RaidwideCast(module, (uint)AID.Flare);
sealed class RomeosBallad(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RomeosBallad, 15f);
sealed class Aim(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Aim, 11f);
sealed class OmniElements(BossModule module) : Components.RaidwideCast(module, (uint)AID.OmniElements);
sealed class ElementaryChemistry(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElementaryChemistry, new AOEShapeRect(15f, 7.5f));
sealed class Iainuki(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Iainuki, new AOEShapeCone(30f, 30f.Degrees()));
sealed class WindSlash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindSlash, new AOEShapeCone(30f, 30f.Degrees()));
sealed class AllConsumingFlames(BossModule module) : Components.SpreadFromIcon(module,
    (uint)IconID.AllConsumingFlames, (uint)AID.AllConsumingFlames, 6f, 5.1d);

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

sealed class ElementalPlatforms(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect Platform = new(7.5f, 7.5f, 7.5f);
    private static readonly uint[] BallOIDs = [(uint)OID.IceBall, (uint)OID.FireBall, (uint)OID.ThunderBall];
    private readonly Dictionary<uint, DateTime> _activations = [];
    private readonly HashSet<uint> _resolved = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        foreach (var ballOID in BallOIDs)
        {
            var ball = Module.Enemies(ballOID).FirstOrDefault(ball => !ball.IsDestroyed);
            var marker = Module.Enemies(MarkerOID(ballOID)).FirstOrDefault(marker => !marker.IsDestroyed);
            if (ball == null)
            {
                _resolved.Remove(ballOID);
                continue;
            }
            if (marker == null || _resolved.Contains(ballOID))
                continue;

            if (!_activations.TryGetValue(ballOID, out var activation))
            {
                var start = Angle.FromDirection(ball.Position - Index.ArenaCenter);
                var clockwise1 = ClockwiseDistance(start, marker.Rotation);
                var clockwise2 = ClockwiseDistance(start, marker.Rotation + 180f.Degrees());
                var travel = Math.Min(clockwise1, clockwise2);
                activation = WorldState.FutureTime(6.45d + travel * 2.435d);
                _activations[ballOID] = activation;
            }

            AddPlatform(marker.Rotation, activation);
            AddPlatform(marker.Rotation + 180f.Degrees(), activation);
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var ballOID = spell.Action.ID switch
        {
            (uint)AID.BlizzardIV => (uint)OID.IceBall,
            (uint)AID.FireIV => (uint)OID.FireBall,
            (uint)AID.ThunderIV => (uint)OID.ThunderBall,
            _ => 0u
        };
        if (ballOID != 0u && _activations.Remove(ballOID))
            _resolved.Add(ballOID);
    }

    private void AddPlatform(Angle rotation, DateTime activation)
    {
        var center = Index.ArenaCenter + 20.5f * rotation.ToDirection();
        _aoes.Add(new(Platform, center, rotation, activation));
    }

    private static uint MarkerOID(uint ballOID) => ballOID switch
    {
        (uint)OID.IceBall => (uint)OID.IcePlatforms,
        (uint)OID.FireBall => (uint)OID.FirePlatforms,
        (uint)OID.ThunderBall => (uint)OID.ThunderPlatforms,
        _ => 0u
    };

    private static float ClockwiseDistance(Angle start, Angle target)
    {
        var distance = (start - target).Normalized().Rad;
        return distance >= 0f ? distance : distance + Angle.DoublePI;
    }
}

sealed class Prophecy(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Circle = new(10f);
    private static readonly AOEShapeDonut Donut = new(3f, 15f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        foreach (var phenomenon in Module.Enemies((uint)OID.PropheticPhenomenon))
        {
            if (phenomenon.IsDestroyed || phenomenon.FindStatus((uint)SID.ProphecyShape) is not ActorStatus status)
                continue;

            AOEShape? shape = status.Extra switch
            {
                1101 => Circle,
                1100 => Donut,
                _ => null
            };
            if (shape != null)
                _aoes.Add(new(shape, phenomenon.Position, activation: status.ExpireAt.AddSeconds(-0.4d), actorID: phenomenon.InstanceID));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }
}

sealed class Shockwave(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _sources = new(3);
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
        if (spell.Action.ID == (uint)AID.ShockwaveVisual)
            _sources.Add(new(caster.Position, 9f, Module.CastFinishAt(spell), actorID: caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShockwaveVisual)
        {
            _sources.RemoveAll(s => s.ActorID == caster.InstanceID);
            ++NumCasts;
        }
    }
}

sealed class IndexStates : StateMachineBuilder
{
    public IndexStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Flare>()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<RomeosBallad>()
            .ActivateOnEnter<Aim>()
            .ActivateOnEnter<OmniElements>()
            .ActivateOnEnter<ElementaryChemistry>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<Iainuki>()
            .ActivateOnEnter<WindSlash>()
            .ActivateOnEnter<AllConsumingFlames>()
            .ActivateOnEnter<ElementalPlatforms>()
            .ActivateOnEnter<Prophecy>();
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
}
