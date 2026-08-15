namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN3Necrophobia;

public enum OID : uint
{
    Boss = 0x4BE5, // R5.0
    BarrierHead = 0x4BE6, // R1.41
    Helper = 0x233C
}

public enum AID : uint
{
    HailOfHellflares = 47452, // Boss->self, raidwide visual
    CorpseMangler = 47459, // Boss->player, tankbuster visual
    AncientFireIII = 47455, // Boss->self, range 18 circle
    SeveredFireIIIHead = 47468, // BarrierHead->self, range 18 circle
    SeveredFireIIIBoss = 47465, // Boss->self, range 18 circle
    AncientBlizzardIII = 47456, // Boss->self, range 45 width 15 cross
    SeveredBlizzardIIIHead = 47469, // BarrierHead->self, range 45 width 15 cross
    SeveredBlizzardIIIBoss = 47466, // Boss->self, range 45 width 15 cross
    SeveredThunderVisual = 47470, // BarrierHead->self, visual for range 60 45-degree cone
    VacuumWave = 47473, // Boss->self, range 30 180-degree cone
    DeathlyRay = 47475, // BarrierHead->self, 30x6 rect
    DarkCurrentLong = 47477, // Helper->self, 60x10 rect
    DarkCurrentPulse = 47478, // Helper->self, 10x60 rect
    AncientThunder = 47458, // Helper->self, range 60 45-degree cone
    SeveredThunderHead = 47471, // Helper->self, range 60 45-degree cone
    SeveredThunderBoss = 50357 // Helper->self, range 60 45-degree cone
}

public enum TetherID : uint
{
    Fire = 400,
    Blizzard = 401,
    Thunder = 402
}

public enum SID : uint
{
    ElementOrder = 2552 // Extra 1114/1115/1116 = fire/blizzard/thunder
}

enum Element
{
    None,
    Fire,
    Blizzard,
    Thunder
}

static class ElementGeometry
{
    private const float SafeMargin = 0.5f;

    public static void AddHeadDanger(List<Shape> result, Element element, WPos origin, Angle rotation)
    {
        switch (element)
        {
            case Element.Fire:
                result.Add(new Circle(origin, 18f + SafeMargin));
                break;
            case Element.Blizzard:
                result.Add(new Cross(origin, 45f, 7.5f + SafeMargin, rotation));
                break;
            case Element.Thunder:
                result.Add(new Cone(origin, 60f, rotation - 23f.Degrees(), rotation + 23f.Degrees()));
                break;
        }
    }

    public static void AddBossDanger(List<Shape> result, Element element, WPos origin, Angle rotation)
    {
        if (element != Element.Thunder)
        {
            AddHeadDanger(result, element, origin, rotation);
            return;
        }

        for (var index = 0; index < 4; ++index)
        {
            var direction = (45f + 90f * index).Degrees();
            result.Add(new Cone(origin, 60f, direction - 23f.Degrees(), direction + 23f.Degrees()));
        }
    }
}

sealed class HailOfHellflares(BossModule module) : Components.RaidwideCast(module, (uint)AID.HailOfHellflares);
sealed class CorpseMangler(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CorpseMangler);
sealed class FireIII(BossModule module) : Components.SimpleAOEGroups(module,
    [(uint)AID.AncientFireIII, (uint)AID.SeveredFireIIIHead, (uint)AID.SeveredFireIIIBoss], 18f)
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Color = Colors.Danger;
        return base.ActiveAOEs(slot, actor);
    }
}
sealed class BlizzardIII(BossModule module) : Components.SimpleAOEGroups(module,
    [(uint)AID.AncientBlizzardIII, (uint)AID.SeveredBlizzardIIIHead, (uint)AID.SeveredBlizzardIIIBoss], new AOEShapeCross(45f, 7.5f))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Color = Colors.Danger;
        return base.ActiveAOEs(slot, actor);
    }
}
sealed class VacuumWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VacuumWave, new AOEShapeCone(30f, 90f.Degrees()));
sealed class DeathlyRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeathlyRay, new AOEShapeRect(30f, 3f));
sealed class Thunder(BossModule module) : Components.SimpleAOEGroups(module,
    [(uint)AID.AncientThunder, (uint)AID.SeveredThunderHead, (uint)AID.SeveredThunderBoss], new AOEShapeCone(60f, 22.5f.Degrees()))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Color = Colors.Danger;
        return base.ActiveAOEs(slot, actor);
    }
}

sealed class DarkCurrent(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect Initial = new(60f, 5f, 60f);
    private static readonly AOEShapeRect Pulse = new(5f, 30f, 5f);
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<AOEInstance> _active = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _active.Clear();
        if (_aoes.Count == 0)
            return [];

        var imminent = _aoes[0].Activation.AddSeconds(0.2d);
        foreach (var source in _aoes)
        {
            var aoe = source;
            var danger = aoe.Activation <= imminent;
            aoe.Color = danger ? Colors.Danger : Colors.AOE;
            aoe.Risky = danger;
            _active.Add(aoe);
        }
        return CollectionsMarshal.AsSpan(_active);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DarkCurrentLong)
        {
            _aoes.Clear();
            var activation = Module.CastFinishAt(spell);
            _aoes.Add(new(Initial, caster.Position, spell.Rotation, activation));
            for (var wave = 1; wave <= 2; ++wave)
            {
                var distance = 10f * wave;
                var waveActivation = activation.AddSeconds(2.1d * wave);
                var leftRotation = spell.Rotation + 90f.Degrees();
                var rightRotation = spell.Rotation - 90f.Degrees();
                _aoes.Add(new(Pulse, caster.Position + distance * leftRotation.ToDirection(), leftRotation, waveActivation));
                _aoes.Add(new(Pulse, caster.Position + distance * rightRotation.ToDirection(), rightRotation, waveActivation));
            }
            Sort();
        }
        else if (spell.Action.ID == (uint)AID.DarkCurrentPulse)
        {
            var index = Find(caster.Position, Pulse);
            if (index >= 0)
            {
                var aoe = _aoes[index];
                aoe.Origin = caster.Position;
                aoe.Rotation = spell.Rotation;
                aoe.Activation = Module.CastFinishAt(spell);
                _aoes[index] = aoe;
                Sort();
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var shape = spell.Action.ID switch
        {
            (uint)AID.DarkCurrentLong => Initial,
            (uint)AID.DarkCurrentPulse => Pulse,
            _ => null
        };
        if (shape == null)
            return;

        var index = Find(caster.Position, shape);
        if (index >= 0)
            _aoes.RemoveAt(index);
        ++NumCasts;
    }

    private int Find(WPos position, AOEShape shape)
    {
        var index = _aoes.FindIndex(aoe => ReferenceEquals(aoe.Shape, shape) && (aoe.Origin - position).LengthSq() < 1f);
        return index >= 0 ? index : _aoes.FindIndex(aoe => ReferenceEquals(aoe.Shape, shape));
    }

    private void Sort() => _aoes.Sort((left, right) => left.Activation.CompareTo(right.Activation));
}

sealed class SeveredElementPreview(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Fire = new(18f);
    private static readonly AOEShapeCross Blizzard = new(45f, 7.5f);
    private static readonly AOEShapeCone Thunder = new(60f, 22.5f.Degrees());
    private readonly Dictionary<ulong, (Actor Actor, uint Tether)> _sources = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        foreach (var (source, tether) in _sources.Values)
        {
            AOEShape? shape = tether switch
            {
                (uint)TetherID.Fire => Fire,
                (uint)TetherID.Blizzard => Blizzard,
                (uint)TetherID.Thunder => Thunder,
                _ => null
            };
            if (shape != null && !source.IsDestroyed)
                _aoes.Add(new(shape, source.Position, source.Rotation, color: Colors.AOE, risky: false, actorID: source.InstanceID));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.BarrierHead && tether.ID is (uint)TetherID.Fire or (uint)TetherID.Blizzard or (uint)TetherID.Thunder)
            _sources[source.InstanceID] = (source, tether.ID);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SeveredFireIIIHead or (uint)AID.SeveredBlizzardIIIHead or (uint)AID.SeveredThunderVisual)
            _sources.Remove(caster.InstanceID);
    }
}

sealed class ElementSafezones(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Dictionary<ulong, (Actor Actor, Element Element)> _heads = [];
    private Element _current;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => [];

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_current != Element.None)
        {
            var dangers = new List<Shape>();
            foreach (var head in _heads.Values)
                if (head.Element == _current && !head.Actor.IsDestroyed)
                    ElementGeometry.AddHeadDanger(dangers, _current, head.Actor.Position, head.Actor.Rotation);
            if (dangers.Count != 0)
            {
                ElementGeometry.AddBossDanger(dangers, _current, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation);
                var safeRegion = new AOEShapeCustom([new Circle(Necrophobia.ArenaCenter, 23.5f)], dangers, origin: Necrophobia.ArenaCenter);
                safeRegion.Draw(Arena, Necrophobia.ArenaCenter, default(Angle), color: Colors.SafeFromAOE);
            }
        }
        base.DrawArenaBackground(pcSlot, pc);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID != (uint)OID.BarrierHead)
            return;

        var element = tether.ID switch
        {
            (uint)TetherID.Fire => Element.Fire,
            (uint)TetherID.Blizzard => Element.Blizzard,
            (uint)TetherID.Thunder => Element.Thunder,
            _ => Element.None
        };
        if (element != Element.None)
            _heads[source.InstanceID] = (source, element);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        _current = spell.Action.ID switch
        {
            (uint)AID.SeveredFireIIIBoss => Element.Fire,
            (uint)AID.SeveredBlizzardIIIBoss => Element.Blizzard,
            _ => _current
        };
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor.InstanceID != Module.PrimaryActor.InstanceID || status.ID != (uint)SID.ElementOrder)
            return;

        _current = status.Extra switch
        {
            1114 => Element.Fire,
            1115 => Element.Blizzard,
            1116 => Element.Thunder,
            _ => _current
        };
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SeveredFireIIIBoss or (uint)AID.SeveredBlizzardIIIBoss or (uint)AID.SeveredThunderBoss)
        {
            _current = Element.None;
            _heads.Clear();
        }
    }
}

sealed class NecrophobiaStates : StateMachineBuilder
{
    public NecrophobiaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HailOfHellflares>()
            .ActivateOnEnter<CorpseMangler>()
            .ActivateOnEnter<FireIII>()
            .ActivateOnEnter<BlizzardIII>()
            .ActivateOnEnter<VacuumWave>()
            .ActivateOnEnter<DeathlyRay>()
            .ActivateOnEnter<DarkCurrent>()
            .ActivateOnEnter<Thunder>()
            .ActivateOnEnter<SeveredElementPreview>()
            .ActivateOnEnter<ElementSafezones>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(NecrophobiaStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    PrimaryActorOID = (uint)OID.Boss,
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093,
    NameID = 14503,
    SortOrder = 3)]
public sealed class Necrophobia(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsCircle(24f))
{
    public static readonly WPos ArenaCenter = new(100f, 800f);
}
