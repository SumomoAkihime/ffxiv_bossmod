namespace BossMod.Dawntrail.Alliance.A35ShinryuParadox;

// Canonical naming compatibility bridge to local A36 implementation.
// Intentionally contains no additional mechanics.
public enum OID : uint
{
    ShinryuParadox = 0x4D92,
    ShinryuParadoxPart1 = 0x4D9A,
    ShinryuParadoxPart2 = 0x4D93,
    HollowKing = 0x4D96,
    HollowKingPart = 0x4D9B,
    Helper = 0x233C,
    UnkownActor = 0x4EB3,
    Exit = 0x1E850B,
    ArcaneSphere = 0x4D97,
    ArcaneSphere1 = 0x4DCD,
    Alxaal = 0x4D98,
    Prishe = 0x4D99,
    GuloolJaJa = 0x4E53
}

public enum AID : uint
{
    SavageBlade = 50389,
    AutoAttack1 = 50721,
    AutoAttack2 = 50763,
    AutoAttack3 = 49138,
    AutoAttack4 = 49137,
    NullifyingDropkick = 50384,
    CosmicBreath = 49106,
    CosmicBreath1 = 49105,
    AuroralUppercut = 50383,
    CosmicBreath2 = 49107,
    VorpalBlade = 50390,
    CosmicTail = 49109,
    CosmicTail1 = 49108,
    CosmicTail2 = 49110,
    CloakOfTwilight = 49111,
    CloakOfTwilight1 = 49112,
    TwilightNebula = 49114,
    TwilightNebula1 = 49113,
    TwilightRadiance = 49115,
    TwilightShadow = 49116,
    Starflare = 49125,
    Starflare1 = 49124,
    SpiritsWithin = 50391,
    Starflare2 = 49126,
    Starflare3 = 49127,
    CataclysmicVortex = 49121,
    CataclysmicVortex1 = 49123,
    CataclysmicVortex2 = 49122,
    UrielBlade = 50392,
    KnuckleSandwich = 50385,
    Banish = 50386,
    DarkNova = 49135,
    DarkNova1 = 49134,
    DarkNova2 = 49136,
    AtomicTail = 49129,
    AtomicTail1 = 49128,
    AtomicTail2 = 49130,
    GyreCharge = 49132,
    GyreCharge1 = 49131,
    GyreCharge2 = 49133,
    Holy = 50393
}

public enum SID : uint
{
    CloakOfWaxingDark = 5353,
    CloakOfWaningLight = 5352,
    VulnerabilityUp = 1789,
    DownForTheCount = 1963,
    UnknownStatus1 = 2202,
    UnknownStatus2 = 2160
}

#pragma warning disable CA1707
public enum IconID : uint
{
    Icon_z6r3_b4_lock_no_lk_7s_c0k2 = 680,
    Icon_z6r3_b4_lock_lk_7s_c0k2 = 681,
    Icon_z6r3_b4_lock_mv_7s_c0k2 = 683,
    Icon_z6r3_b4_lock_no_mv_7s_c0k2 = 682,
    Icon_m0489trg_a0c = 136,
    Icon_m0489trg_b0c = 137,
    Icon_tank_lockonae_6m_5s_01t = 344
}
#pragma warning restore CA1707

public sealed class A35ShinryuParadox(WorldState ws, Actor primary)
    : global::BossMod.Dawntrail.Alliance.A36ShinryuParadox.A36ShinryuParadox(ws, primary);

sealed class A35ShinryuParadoxStates(global::BossMod.Dawntrail.Alliance.A36ShinryuParadox.A36ShinryuParadox module)
    : global::BossMod.Dawntrail.Alliance.A36ShinryuParadox.A36ShinryuParadoxStates(module);
