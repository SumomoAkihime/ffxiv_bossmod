namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME3Necrophobia;

public enum OID : uint
{
    Boss = 0x4BE7, // R5.0
    BarrierHead = 0x4BE8, // R1.41
    Helper = 0x233C
}

public enum AID : uint
{
    HailOfHellflares = 47482, // Boss->self, raidwide visual
    HailOfHellflaresAOE = 47483, // Helper->self, raidwide damage
    SummonMagickedHead = 47485, // Boss->self, visual
    MagickaInjection = 47486, // Boss->self, visual
    DeployMagickedHead = 47488, // Boss->self, visual
    SeveredFireIIIBoss = 47490, // Boss->self, range 18 circle
    SeveredBlizzardIIIBoss = 47491, // Boss->self, range 45 width 15 cross
    SeveredThunderBossVisual = 47492, // Boss->self, visual
    AncientThunderSoil = 47493, // Helper->self, range 60 45-degree cone
    AncientFireIII = 47494, // BarrierHead->self, range 18 circle
    AncientBlizzardIII = 47495, // BarrierHead->self, range 45 width 15 cross
    AncientThunderVisual = 47496, // BarrierHead->self, visual
    AncientThunder = 47497, // Helper->self, range 60 45-degree cone
    DarkCurrentVisual = 47499, // Boss->self, visual
    DarkCurrentLong = 47500, // Helper->self, 60x10 rect
    DarkCurrentPulse = 47501, // Helper->self, 10x60 rect
    VacuumWave = 47502, // Boss->self, range 30 180-degree cone
    DeathlyRay = 47504, // BarrierHead->self, 30x6 rect
    CorpseMangler = 47505, // Boss->player, tankbuster visual
    ThreeMustBeItsGrave = 47506, // Boss->self, element-order visual
    SeveredCurrentVisual = 47507, // Boss->self, sequence visual
    SeveredCurrentStep = 47508, // Boss->self, step visual
    SeveredCurrentLong = 47509, // Helper->self, 60x10 rect
    SeveredFireIIIHead = 47510, // BarrierHead->self, range 18 circle
    SeveredBlizzardIIIHead = 47511, // BarrierHead->self, range 45 width 15 cross
    SeveredThunderHeadVisual = 47512, // BarrierHead->self, visual
    SeveredThunderHead = 47513, // Helper->self, range 60 45-degree cone
    FertileSoil = 47514, // Boss->self, visual
    MagickChain = 47515, // Boss->self, soil sequence visual
    SowPanicVisual = 47516, // BarrierHead->self, visual
    SowTerror1 = 47517, // Helper->self, 80x30 rect
    SowPanic1 = 47518, // Helper->self, 80x30 rect
    SowTerror2 = 47519, // Helper->self, 80x30 rect
    SowPanic2 = 47520, // Helper->self, 80x30 rect
    AncientFireIIISoil = 47521, // Boss->self, range 18 circle
    AncientBlizzardIIISoil = 47522, // Boss->self, range 45 width 15 cross
    AncientThunderSoilVisual = 47523, // Boss->self, visual
    TheVoid = 47524, // Boss->self, enrage
    SeveredThunderBoss = 50358 // Helper->self, range 60 45-degree cone
}

public enum SID : uint
{
    ElementOrder = 2552
}

public enum TetherID : uint
{
    Fire = 400,
    Blizzard = 401,
    Thunder = 402
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

    public static void AddDangers(List<Shape> result, Element element, WPos origin, Angle rotation)
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
                for (var index = 0; index < 4; ++index)
                {
                    var direction = (45f + 90f * index).Degrees();
                    result.Add(new Cone(origin, 60f, direction - 23f.Degrees(), direction + 23f.Degrees()));
                }
                break;
        }
    }

    public static Shape ForwardRect(WPos origin, Angle rotation, float length, float halfWidth)
        => new Rectangle(origin + 0.5f * length * rotation.ToDirection(), halfWidth + SafeMargin, 0.5f * length, rotation);

    public static Shape CenteredRect(WPos origin, Angle rotation, float halfLength, float halfWidth)
        => new Rectangle(origin, halfWidth + SafeMargin, halfLength, rotation);
}

sealed class HailOfHellflares(BossModule module) : Components.RaidwideCast(module, (uint)AID.HailOfHellflares);
sealed class CorpseMangler(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CorpseMangler);
sealed class TheVoid(BossModule module) : Components.RaidwideCast(module, (uint)AID.TheVoid);

sealed class FireIII(BossModule module) : Components.SimpleAOEGroups(module,
    [(uint)AID.SeveredFireIIIBoss, (uint)AID.AncientFireIII, (uint)AID.SeveredFireIIIHead, (uint)AID.AncientFireIIISoil], 18f)
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Color = Colors.Danger;
        return base.ActiveAOEs(slot, actor);
    }
}

sealed class BlizzardIII(BossModule module) : Components.SimpleAOEGroups(module,
    [(uint)AID.SeveredBlizzardIIIBoss, (uint)AID.AncientBlizzardIII, (uint)AID.SeveredBlizzardIIIHead, (uint)AID.AncientBlizzardIIISoil],
    new AOEShapeCross(45f, 7.5f))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Color = Colors.Danger;
        return base.ActiveAOEs(slot, actor);
    }
}

sealed class Thunder(BossModule module) : Components.SimpleAOEGroups(module,
    [(uint)AID.SeveredThunderBoss, (uint)AID.AncientThunder, (uint)AID.SeveredThunderHead, (uint)AID.AncientThunderSoil],
    new AOEShapeCone(60f, 22.5f.Degrees()))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        Color = Colors.Danger;
        return base.ActiveAOEs(slot, actor);
    }
}

sealed class VacuumWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VacuumWave, new AOEShapeCone(30f, 90f.Degrees()));
sealed class DeathlyRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeathlyRay, new AOEShapeRect(30f, 3f));

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
        if (spell.Action.ID is (uint)AID.DarkCurrentLong or (uint)AID.SeveredCurrentLong)
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
            (uint)AID.DarkCurrentLong or (uint)AID.SeveredCurrentLong => Initial,
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

sealed class ElementSafezones(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly uint FutureSafeColor = Color.FromComponents(64, 192, 255, 64).ABGR;
    private static readonly Angle[] CurrentDirections = [180f.Degrees(), 60f.Degrees(), -60f.Degrees()];
    private readonly Dictionary<ulong, (Actor Actor, Element Element)> _heads = [];
    private readonly List<Element> _sequence = [];
    private Element _directElement;
    private int _sequenceIndex;
    private bool _sequenceActive;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => [];

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_directElement != Element.None)
        {
            DrawSafe(BuildDangers(_directElement, true, null), Colors.SafeFromAOE);
        }
        else if (_sequenceActive && _sequenceIndex < _sequence.Count)
        {
            if (_sequenceIndex + 1 < _sequence.Count)
                DrawSafe(BuildDangers(_sequence[_sequenceIndex + 1], false, _sequenceIndex + 1), FutureSafeColor);
            DrawSafe(BuildDangers(_sequence[_sequenceIndex], false, _sequenceIndex), Colors.SafeFromAOE);
        }
        base.DrawArenaBackground(pcSlot, pc);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_sequenceActive && _sequenceIndex < _sequence.Count)
            hints.Add("绿色为当前安全区，青蓝色为下一轮安全区", false);
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
        _directElement = spell.Action.ID switch
        {
            (uint)AID.SeveredFireIIIBoss => Element.Fire,
            (uint)AID.SeveredBlizzardIIIBoss => Element.Blizzard,
            (uint)AID.SeveredThunderBossVisual => Element.Thunder,
            _ => _directElement
        };

        if (spell.Action.ID == (uint)AID.ThreeMustBeItsGrave)
        {
            _directElement = Element.None;
            _sequence.Clear();
            _sequenceIndex = 0;
            _sequenceActive = true;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (!_sequenceActive || actor.InstanceID != Module.PrimaryActor.InstanceID || status.ID != (uint)SID.ElementOrder)
            return;

        var element = status.Extra switch
        {
            1114 => Element.Fire,
            1115 => Element.Blizzard,
            1116 => Element.Thunder,
            _ => Element.None
        };
        if (element != Element.None && _sequence.Count < 3 && !_sequence.Contains(element))
            _sequence.Add(element);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SeveredFireIIIBoss or (uint)AID.SeveredBlizzardIIIBoss or (uint)AID.SeveredThunderBossVisual)
            _directElement = Element.None;

        if (_sequenceActive && spell.Action.ID == (uint)AID.SeveredCurrentLong)
        {
            ++_sequenceIndex;
            if (_sequenceIndex >= 3)
            {
                _sequence.Clear();
                _sequenceActive = false;
            }
        }
    }

    private List<Shape> BuildDangers(Element element, bool includeBossElement, int? currentIndex)
    {
        var result = new List<Shape>();
        foreach (var head in _heads.Values)
            if (head.Element == element && !head.Actor.IsDestroyed)
                ElementGeometry.AddDangers(result, element, head.Actor.Position, head.Actor.Rotation);

        if (includeBossElement)
            ElementGeometry.AddDangers(result, element, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation);
        if (currentIndex is >= 0 and < 3)
            result.Add(ElementGeometry.CenteredRect(Necrophobia.ArenaCenter, CurrentDirections[currentIndex.Value], 60f, 5f));
        return result;
    }

    private void DrawSafe(List<Shape> dangers, uint color)
    {
        if (dangers.Count == 0)
            return;
        var safeRegion = new AOEShapeCustom([new Circle(Necrophobia.ArenaCenter, 24.5f)], dangers, origin: Necrophobia.ArenaCenter);
        safeRegion.Draw(Arena, Necrophobia.ArenaCenter, default(Angle), color: color);
    }
}

sealed class FertileSoil(BossModule module) : Components.GenericAOEs(module)
{
    private sealed class Prediction(Actor head, DateTime activation)
    {
        public readonly Actor Head = head;
        public DateTime Activation = activation;
    }

    private static readonly AOEShapeRect HalfArena = new(80f, 15f);
    private readonly List<Prediction> _order = [];
    private readonly List<AOEInstance> _active = [];
    private readonly List<Shape> _elementDangers = [];
    private DateTime _firstActivation;
    private uint _elementDamageAID;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _active.Clear();
        for (var index = 0; index < Math.Min(2, _order.Count); ++index)
        {
            var prediction = _order[index];
            var rotation = (Necrophobia.ArenaCenter - prediction.Head.Position).ToAngle();
            _active.Add(new(HalfArena, prediction.Head.Position, rotation, prediction.Activation,
                index == 0 ? Colors.Danger : Colors.AOE, index == 0, prediction.Head.InstanceID));
        }
        return CollectionsMarshal.AsSpan(_active);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_order.Count != 0)
        {
            var current = _order[0];
            var rotation = (Necrophobia.ArenaCenter - current.Head.Position).ToAngle();
            var dangers = new List<Shape>(_elementDangers)
            {
                ElementGeometry.ForwardRect(current.Head.Position, rotation, 80f, 15f)
            };
            var safeRegion = new AOEShapeCustom([new Circle(Necrophobia.ArenaCenter, 24.5f)], dangers, origin: Necrophobia.ArenaCenter);
            safeRegion.Draw(Arena, Necrophobia.ArenaCenter, default(Angle), color: Colors.SafeFromAOE);
        }
        base.DrawArenaBackground(pcSlot, pc);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_order.Count != 0)
            hints.Add("绿色为下一次半场攻击的安全区", false);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FertileSoil)
        {
            _order.Clear();
            _elementDangers.Clear();
            _elementDamageAID = 0;
            _firstActivation = default;
        }
        else if (spell.Action.ID == (uint)AID.MagickChain)
        {
            _firstActivation = Module.CastFinishAt(spell).AddSeconds(1.35d);
            UpdateActivations();
        }

        var element = spell.Action.ID switch
        {
            (uint)AID.AncientFireIIISoil => Element.Fire,
            (uint)AID.AncientBlizzardIIISoil => Element.Blizzard,
            (uint)AID.AncientThunderSoilVisual => Element.Thunder,
            _ => Element.None
        };
        if (element != Element.None)
        {
            _elementDangers.Clear();
            ElementGeometry.AddDangers(_elementDangers, element, Module.PrimaryActor.Position, spell.Rotation);
            _elementDamageAID = element == Element.Thunder ? (uint)AID.AncientThunderSoil : spell.Action.ID;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor.OID != (uint)OID.BarrierHead || status.ID != (uint)SID.ElementOrder || status.Extra is not (1117 or 1118)
            || _order.Any(prediction => prediction.Head.InstanceID == actor.InstanceID))
            return;

        if (_firstActivation == default)
            _firstActivation = WorldState.CurrentTime.AddSeconds(13.58d);
        _order.Add(new(actor, _firstActivation.AddSeconds(6d * _order.Count)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SowTerror1 or (uint)AID.SowPanic1 or (uint)AID.SowTerror2 or (uint)AID.SowPanic2)
        {
            var index = _order.FindIndex(prediction => (prediction.Head.Position - caster.Position).LengthSq() < 1f);
            if (index >= 0)
            {
                _order.RemoveAt(index);
                ++NumCasts;
            }
        }

        if (spell.Action.ID == _elementDamageAID)
        {
            _elementDangers.Clear();
            _elementDamageAID = 0;
        }
    }

    private void UpdateActivations()
    {
        for (var index = 0; index < _order.Count; ++index)
            _order[index].Activation = _firstActivation.AddSeconds(6d * index);
    }
}

sealed class NecrophobiaStates : StateMachineBuilder
{
    public NecrophobiaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HailOfHellflares>()
            .ActivateOnEnter<CorpseMangler>()
            .ActivateOnEnter<TheVoid>()
            .ActivateOnEnter<FireIII>()
            .ActivateOnEnter<BlizzardIII>()
            .ActivateOnEnter<Thunder>()
            .ActivateOnEnter<VacuumWave>()
            .ActivateOnEnter<DeathlyRay>()
            .ActivateOnEnter<DarkCurrent>()
            .ActivateOnEnter<ElementSafezones>()
            .ActivateOnEnter<FertileSoil>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(NecrophobiaStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1114,
    NameID = 14503,
    SortOrder = 3)]
public sealed class Necrophobia(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsCircle(25f))
{
    public static readonly WPos ArenaCenter = new(100f, 800f);
}
