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
    SpinDonutLarge = 49589, // Sword->self, range 5-60 donut
    SpinDonutSmall = 49590, // Sword->self, range 5-60 donut
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
sealed class SpinCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpinCircle, 15f);
sealed class SpinDonutLarge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpinDonutLarge, new AOEShapeDonut(5f, 60f));
sealed class SpinDonutSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpinDonutSmall, new AOEShapeDonut(5f, 60f));
sealed class SwordDance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SwordDance, new AOEShapeRect(60f, 10f));
sealed class Pierce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pierce, 5f);
sealed class Steelsbreath(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Steelsbreath, 24f);
sealed class Surgeswords(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Surgeswords, new AOEShapeRect(30f, 3f));

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
            .ActivateOnEnter<SpinCircle>()
            .ActivateOnEnter<SpinDonutLarge>()
            .ActivateOnEnter<SpinDonutSmall>()
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
public sealed class MTN2SwordDancer(WorldState ws, Actor primary) : BossModule(ws, primary, new(600f, 704f), new ArenaBoundsCircle(30f));
