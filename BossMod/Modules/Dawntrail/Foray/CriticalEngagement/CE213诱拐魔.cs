namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE213诱拐魔;

public enum OID : uint
{
    Boss = 0x4BE1, // R5.0
    Gale = 0x4BE2, // R1.0
    Feather = 0x4BE3, // R1.0
    ArenaBoundary = 0x4BE4, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    ArenaBoundary = 47435, // ArenaBoundary->self, no cast, range 30 circle, damages players outside the arena
    HurricaneVisual = 47436, // Boss->self, 5.0s cast
    GaleTouch = 47437, // Gale->self, no cast, range 4 circle, knockback 5
    RippingWindVisual = 47438, // Gale->self, 1.0s cast
    RippingWind = 47439, // Helper->self, 1.0s cast, range 60 width 8 cross
    Gust = 47440, // Helper->self, no cast, directional knockback 24
    GaleBlade = 47441, // Boss->self, 5.0s cast, range 60 180-degree cone
    ScatterFeatherVisual = 47442, // Boss->self, 3.0s cast
    Shatter = 47443, // Feather->self, 4.5s cast, range 13 circle
    WindScatterVisual = 47444, // Boss->self, 3.5s cast
    WindScatter = 47445, // Helper->self, 4.0s cast, range 60 60-degree cone
    HeavensfallVisual = 47446, // Boss->location, no cast
    CyclonicRingVisual = 47447, // Boss->location, no cast
    Heavensfall = 47448, // Helper->self, 5.5s cast, range 15 circle
    CyclonicRing = 47449, // Helper->self, 5.5s cast, range 5-60 donut
    Hurricane = 48120, // Helper->self, no cast, raidwide
    GustVisual = 48250 // Helper->self, 4.0s cast, directional knockback telegraph
}

public enum IconID : uint
{
    RippingWind = 506
}

sealed class GaleBlade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GaleBlade, new AOEShapeCone(60f, 90f.Degrees()));
sealed class Shatter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shatter, 13f);
sealed class RippingWind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RippingWind, new AOEShapeCross(60f, 4f));
sealed class WindScatter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindScatter, new AOEShapeCone(60f, 30f.Degrees()));
sealed class Heavensfall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Heavensfall, 15f);
sealed class CyclonicRing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CyclonicRing, new AOEShapeDonut(5f, 60f));
sealed class Hurricane(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.HurricaneVisual, (uint)AID.Hurricane, 0.9d);

sealed class MovingGales(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Circle = new(4f);
    private readonly List<AOEInstance> _aoes = [];
    private readonly HashSet<ulong> _selected = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        foreach (var gale in Module.Enemies((uint)OID.Gale))
        {
            if (!gale.IsDeadOrDestroyed)
                _aoes.Add(new(Circle, gale.Position, color: _selected.Contains(gale.InstanceID) ? Colors.Danger : Colors.AOE));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.RippingWind && actor.OID == (uint)OID.Gale)
            _selected.Add(actor.InstanceID);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RippingWindVisual)
            _selected.Remove(caster.InstanceID);
    }
}

sealed class Gust(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback? _source;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
        => _source is { } source ? new Knockback[1] { source } : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GustVisual)
            _source = new(caster.Position, 24f, Module.CastFinishAt(spell, 0.65d), direction: spell.Rotation, kind: Kind.DirForward);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Gust)
        {
            _source = null;
            ++NumCasts;
        }
    }
}

sealed class CE213诱拐魔States : StateMachineBuilder
{
    public CE213诱拐魔States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GaleBlade>()
            .ActivateOnEnter<Shatter>()
            .ActivateOnEnter<MovingGales>()
            .ActivateOnEnter<RippingWind>()
            .ActivateOnEnter<WindScatter>()
            .ActivateOnEnter<Heavensfall>()
            .ActivateOnEnter<CyclonicRing>()
            .ActivateOnEnter<Gust>()
            .ActivateOnEnter<Hurricane>();
    }
}

// Temporary module; remove when upstream adds an encounter for OID 0x4BE1.
[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(CE213诱拐魔States),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.Boss,
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093,
    NameID = 14505)]
public sealed class CE213诱拐魔(WorldState ws, Actor primary) : BossModule(ws, primary, new(-150f, -860f), new ArenaBoundsCircle(30f));
