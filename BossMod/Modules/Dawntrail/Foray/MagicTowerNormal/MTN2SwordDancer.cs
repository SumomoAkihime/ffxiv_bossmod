namespace BossMod.Dawntrail.Foray.MagicTowerNormal.MTN2SwordDancer;

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
    TurnaboutOuter = 49889, // Helper->self, range 19-24 54-degree cone
    MartialMystique = 49585, // Helper->self, 48x96 rect
    SpinCircle = 49592, // Sword->self, range 15 circle
    SpinDonutLarge = 49589, // Sword->self, range 20-60 donut
    SpinDonutSmall = 49590, // Sword->self, range 15-60 donut
    SwordDance = 49614, // Helper->self, 60x20 rect
    Pierce = 49595, // Sword->self, range 5 circle
    Steelsbreath = 50359, // Helper->self, knockback 24
    Surgeswords = 49616 // RushSword->self, 30x6 rect
}

sealed class SwordStorm(BossModule module) : Components.RaidwideCast(module, (uint)AID.SwordStorm);
sealed class RushShort(BossModule module) : Components.ChargeAOEs(module, (uint)AID.RushShort, 3.5f);
sealed class RushLong(BossModule module) : Components.ChargeAOEs(module, (uint)AID.RushLong, 3.5f);
sealed class TurnInner(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TurnInner, new AOEShapeDonutSector(9f, 14f, 45f.Degrees()));
sealed class TurnOuter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TurnOuter, new AOEShapeDonutSector(19f, 24f, 45f.Degrees()));
sealed class TurnInnerNarrow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TurnInnerNarrow, new AOEShapeDonutSector(9f, 14f, 32.5f.Degrees()));
sealed class TurnaboutOuter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TurnaboutOuter, new AOEShapeDonutSector(19f, 24f, 27f.Degrees()));
sealed class MartialMystique(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MartialMystique, new AOEShapeRect(48f, 48f));
sealed class SwordDance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SwordDance, new AOEShapeRect(60f, 10f));
sealed class Pierce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pierce, 5f);
sealed class Steelsbreath(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Steelsbreath, 24f);

sealed class SpinningSword(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Circle = new(15f);
    private static readonly AOEShapeDonut DonutLarge = new(20f, 60f);
    private static readonly AOEShapeDonut DonutSmall = new(15f, 60f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID != (uint)OID.SpinningSword)
            return;

        _aoes.RemoveAll(aoe => aoe.ActorID == actor.InstanceID);
        AOEShape? shape = modelState switch
        {
            7 => Circle,
            4 => DonutLarge,
            5 => DonutSmall,
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

sealed class MTN2SwordDancerStates : StateMachineBuilder
{
    public MTN2SwordDancerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SwordStorm>()
            .ActivateOnEnter<RushShort>()
            .ActivateOnEnter<RushLong>()
            .ActivateOnEnter<TurnInner>()
            .ActivateOnEnter<TurnOuter>()
            .ActivateOnEnter<TurnInnerNarrow>()
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
    StatesType = typeof(MTN2SwordDancerStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093,
    NameID = 14820,
    SortOrder = 2)]
public sealed class MTN2SwordDancer(WorldState ws, Actor primary) : BossModule(ws, primary, new(600f, 704f), new ArenaBoundsCircle(25f));
