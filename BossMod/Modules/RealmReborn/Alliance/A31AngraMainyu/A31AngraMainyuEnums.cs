namespace BossMod.RealmReborn.Alliance.A31AngraMainyu;

public enum OID : uint
{
    Boss = 0xE00, // R3.200, x1
    Helper = 0x1B2, // R0.500
    FinalHourglass = 0xE02, // R1.500
    GrimReaper = 0xE01, // R2.000
    AngraMainyusDaewa = 0xE4E, // R1.800
    RoulettePointerNE = 0x1E9727, // → 45°
    RoulettePointerSE = 0x1E9726, // → 135°
    RoulettePointerSW = 0x1E970C, // → -135°
    RoulettePointerNW = 0x1E9728, // → -45°
    DoomPlatformNE = 0x1E9712, // R2.000
    DoomPlatformNW = 0x1E9711, // R2.000
    DoomPlatformSE = 0x1E9713, // R2.000
    DoomPlatformSW = 0x1E9714, // R2.000
}

public enum AID : uint
{
    AutoAttack = 3508, // Boss->player, no cast, single-target
    DoubleVision = 3272, // Boss->self, 2.5s cast, range 60 circle
    SullenGaze = 3273, // Helper->self, no cast, forward 180-degree half
    IrefulGaze = 3274, // Helper->self, no cast, backward 180-degree half
    Stare = 3280, // Boss->self, no cast, range 60+R width 8 rect
    Level100Flare = 3275, // Boss->location, 4.5s cast
    Level100FlareResolve = 3276, // Helper->players, no cast, range 12
    MortalGaze = 3281, // Boss->self, 3.0s cast, range 60 circle
    MortalGazeHelper = 3499, // Helper->self, 4.5s cast, range 60 circle
    Death = 3279, // GrimReaper->self, no cast, lethal roulette quarter
    Thunder = 968, // AngraMainyusDaewa->player, 1.0s cast, single-target
    EyesOnMe = 3358, // AngraMainyusDaewa->self, 4.0s cast, range 30+R circle
    Level150Death = 3277, // Boss->location, 4.5s cast
    Level150DeathResolve = 3278, // Helper->players, no cast, range 12
    Paralyze = 1118, // AngraMainyusDaewa->player, 4.0s cast, single-target
}

public enum SID : uint
{
    BrandOfTheSullen = 636, // Helper->player, extra=0x1/0x2/0x3
    BrandOfTheIreful = 637, // Helper->player, extra=0x1/0x2/0x3
    Bind = 280, // none->player, extra=0x0
    Suppuration = 375, // Helper->player, extra=0x1
    Doom = 210, // Helper->player, extra=0x0
}

public enum TetherID : uint
{
    Flare = 5, // player->player
    Death = 1, // player->player
}

public enum IconID : uint
{
    Level100Flare = 44, // player->self
    Level150Death = 45, // player->self
}
