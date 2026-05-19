namespace BossMod.Dawntrail.Alliance.A34Promathia;

// Reborn naming-compat bridge to local canonical A35 module.
// Intentionally no ModuleInfo and no extra mechanic components/listeners here.
public enum OID : uint
{
    Boss = 0x4DEE,
    Helper = 0x233C,
    LinkOfPromathia = 0x4DEF,
    MemoryReceptacle = 0x4DF0,
    EmptyWanderer = 0x4DF1,
    EmptyWeeper = 0x4DF2,
    EmptyThinker = 0x4DF3,
}

public enum AID : uint
{
    AutoAttack = 45308,
    Jump = 50342,
    EmptySalvation = 50317,
    FleetingEternity1 = 50318,
    FleetingEternity2 = 50319,
    Explosion = 50320,
    WheelOfImpregnabilityCast = 50321,
    BastionOfTwilightCast = 50322,
    WheelOfImpregnability = 50323,
    BastionOfTwilight = 50324,
    PestilentPenanceCast = 50330,
    PestilentPenanceBig = 50331,
    PestilentPenanceSmall = 50332,
    CometCast = 50337,
    Comet = 50338,
    Unk1 = 50345,
    FalseGenesisCast = 50343,
    FalseGenesis = 50344,
    WindsOfPromyvionVisualFirst = 50352,
    WindsOfPromyvionVisualRest = 50460,
    WindsOfPromyvionFirst = 50353,
    WindsOfPromyvionRest = 50354,
    EmptyBeleaguer = 50351,
    AuroralDrape = 50355,
    EmptySeed = 50349,
    DeadlyRebirthEnrageCast = 50346,
    DeadlyRebirthRaidwide = 50347,
    DeadlyRebirthEnrage = 50348,
    DeadlyRebirthCast = 50694,
    EarthboundHeaven = 50333,
    MalevolentBlessingCast1 = 50326,
    MalevolentBlessingCast2 = 50327,
    MalevolentBlessingCone = 50328,
    MalevolentBlessingSide = 50329,
    InfernalDeliverance = 50334,
    InfernalDeliveranceTower = 50335,
    InfernalDeliverancePuddle = 50565,
    MeteorCast = 50339,
    MeteorTank = 50340,
    MeteorSpread = 50341,
}

public enum SID : uint
{
    Unk = 2552,
    Unk1 = 2056,
    Unk2 = 2160,
    Unk3 = 2273,
    Heavy = 1796,
    SystemLock = 2578,
    Invincibility = 1570,
    DownForTheCount = 3908,
}

public enum IconID : uint
{
    WheelOfImpregnability = 687,
    BastionOfTwilight = 688,
    Tankbuster = 344,
    TurningRight = 689,
    TurningLeft = 690,
    Spread = 466,
}

public enum TetherID : uint
{
    Unk1 = 427,
    Unk2 = 12,
}

public sealed class A34Promathia(WorldState ws, Actor primary)
    : global::BossMod.Dawntrail.Alliance.A35Promathia.A35Promathia(ws, primary);

sealed class A34PromathiaStates(global::BossMod.Dawntrail.Alliance.A35Promathia.A35Promathia module)
    : global::BossMod.Dawntrail.Alliance.A35Promathia.A35PromathiaStates(module);
