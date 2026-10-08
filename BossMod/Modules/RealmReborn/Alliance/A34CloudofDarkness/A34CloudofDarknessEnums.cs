namespace BossMod.RealmReborn.Alliance.A34CloudofDarkness;

public enum OID : uint
{
    Boss = 0xCFA, // R14.000, x1
    Helper = 0x1B2, // R0.500
    HelperAlt = 0x8EE, // R0.500
    DarkCloud = 0xE1C, // R1.000
    DarkStorm = 0xE1D, // R2.000
    HyperchargedCloud = 0xE03, // R1.000
    Shadowlurker = 0xCFB, // R1.000
    ParticleBeamTower = 0x1E974E, // R0.500, EventObj
    HyperchargedHexA = 0x1E9752, // A
    HyperchargedHexB = 0x1E9750, // B
    HyperchargedHexC = 0x1E9751, // C
}

public enum AID : uint
{
    AutoAttack = 3306, // Boss->player, no cast, single-target
    FeintParticleBeam = 3298, // Boss->location, 3.5s cast, range 8 circle
    FeintParticleBeamChase = 3299, // Helper->location, no cast, range 3 circle
    ZeroFormParticleBeam = 3297, // Boss->self, 2.5s cast, range 60+R width 24 rect
    ParticleBeam = 3301, // Helper->location, no cast, range 5 circle (soaked comet)
    ParticleBeamFail = 3300, // Helper->location, no cast, range 60 circle (unsoaked comet)
    Unknown = 2369, // HelperAlt->self, no cast, range 20
    FloodOfDarkness = 3305, // Boss->location, no cast, range 60 circle
    ParticleBeamEnrage = 3302, // HyperchargedCloud->self, 20.0s cast, range 60 circle
    BlackThunder = 3304, // Shadowlurker->players, no cast, range 20 circle
    BadBreath = 3303, // Shadowlurker->self, no cast, range 60+R cone
}
