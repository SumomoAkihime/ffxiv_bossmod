namespace BossMod.RealmReborn.Alliance.A33Cerberus;

public enum OID : uint
{
    Boss = 0xDF6, // R10.800, x1
    Helper = 0x1B2, // R0.500
    HelperAlt = 0x8EE, // R0.500
    HoundTarget = 0x19A, // R0.500
    StomachWall = 0xDFA, // R1.500
    GastricJuice = 0xDF9, // R1.000
    Wolfsbane = 0xDF8, // R0.800
    Unknown = 0xDF7, // R1.800, belly add
    Electron = 0xDFB, // R1.000
    SlabberVoidzone = 0x1E968C, // R0.500, EventObj (EventState 7 = inactive)
}

public enum AID : uint
{
    AutoAttack = 3509, // Boss->player, no cast, single-target
    PredatorClaws = 3245, // Boss->self, no cast, range 9+R cone
    TailBlow = 3246, // Boss->self, 2.0s cast, range 9+R 90-degree cone
    Innerspace = 3248, // Boss->player, no cast, single-target
    Mini = 3249, // GastricJuice->self, 3.0s cast, range 8+R circle
    Slabber = 3241, // Boss->location, 2.9s cast, range 8 circle
    Voidzone = 3376, // HelperAlt->self, no cast, range 8 circle
    Engorge = 3243, // Boss->location, no cast, range 8 circle
    Seedvolley = 344, // Wolfsbane->player, no cast, single-target
    DeathRay = 1913, // Unknown->player, no cast, single-target
    SulphurousBreath = 3250, // Boss->self, 2.0s cast, range 25+R width 6 rect
    SulphurousBreathHelper = 3251, // Helper->self, 3.0s cast, range 40+R width 6 rect
    Devour = 3242, // Boss->location, no cast, range 8 circle
    SourSough = 3510, // Wolfsbane->self, no cast, range 6+R cone
    Spew = 3244, // HelperAlt->self, no cast, range 60+R circle
    LightningBolt = 3252, // Boss->location, no cast, range 8 circle
    LightningBoltCharge = 3253, // Electron->Electron, 2.0s cast, width 4 rect charge
    HoundOutOfHell = 3247, // Boss->HoundTarget, 3.5s cast, width 14 rect charge
    Ululation = 3254, // Boss->self, 5.0s cast, range 80+R circle
    Reawakening = 3507, // Boss->self, 50.0s cast, single-target
    AutoAttackAdd = 872, // Wolfsbane/Unknown->player, no cast, single-target
    HexEye = 1914, // Unknown->self, 2.5s cast, range 3+R circle
}

public enum SID : uint
{
    Minimum = 438, // GastricJuice->player, extra=0xA
}
