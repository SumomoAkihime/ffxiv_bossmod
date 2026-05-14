namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse.D99EminentGrief;

public enum OID : uint
{
    Boss = 0x48EA, // R28.500, x1
    BossEater = 0x48EB, // R15.000, x0 (spawn during fight)
    VodorigaMinion = 0x48EC, // R1.200, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, xN, Helper type
}

public enum AID : uint
{
    AutoAttack1 = 44813, // Helper->player, 0.8s cast, single-target
    AutoAttack2 = 44096, // Helper->player, 0.8s cast, single-target

    ChainsOfCondemnation1 = 44064, // Helper->location, 6.0s cast, range 30 circle
    ChainsOfCondemnation2 = 44070, // Helper->location, 9.0s cast, range 30 circle

    BladeOfFirstLight1 = 44073, // Helper->self, 9.0s cast, range 30 width 15 rect
    BladeOfFirstLight2 = 44067, // Helper->self, 6.0s cast, range 30 width 15 rect

    BoundsOfSinPull = 44082, // Helper->self, 4.0s cast, pull
    BoundsOfSin = 44083, // Helper->self, 3.0s cast, range 3 circle
    BoundsOfSinEnd = 44084, // Helper->self, no cast, range 8 circle

    Spinelash = 45118, // Helper->self, 1.8s cast, range 60 width 4 rect

    TerrorEye = 45115, // VodorigaMinion->location, 3.0s cast, range 6 circle
    BallOfFire = 44062, // Helper->location, 2.1s cast, range 6 circle

    AbyssalBlazeFirst = 44079, // Helper->location, 7.0s cast, range 5 circle
    AbyssalBlazeRest = 44080, // Helper->location, no cast, range 5 circle
}

class ChainsOfCondemnation(BossModule module) : Components.GroupedAOEs(module, [AID.ChainsOfCondemnation1, AID.ChainsOfCondemnation2], new AOEShapeCircle(30));
class BladeOfFirstLight(BossModule module) : Components.GroupedAOEs(module, [AID.BladeOfFirstLight1, AID.BladeOfFirstLight2], new AOEShapeRect(30, 7.5f));
class BoundsOfSin(BossModule module) : Components.StandardAOEs(module, AID.BoundsOfSin, 3);
class BoundsOfSinEnd(BossModule module) : Components.StandardAOEs(module, AID.BoundsOfSinEnd, 8);
class Spinelash(BossModule module) : Components.StandardAOEs(module, AID.Spinelash, new AOEShapeRect(60, 2));
class TerrorEyeBallOfFire(BossModule module) : Components.GroupedAOEs(module, [AID.TerrorEye, AID.BallOfFire], new AOEShapeCircle(6));
class AbyssalBlaze(BossModule module) : Components.GroupedAOEs(module, [AID.AbyssalBlazeFirst, AID.AbyssalBlazeRest], new AOEShapeCircle(5));
class Vodoriga(BossModule module) : Components.Adds(module, (uint)OID.VodorigaMinion);

class D99EminentGriefStates : StateMachineBuilder
{
    public D99EminentGriefStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Vodoriga>()
            .ActivateOnEnter<ChainsOfCondemnation>()
            .ActivateOnEnter<BladeOfFirstLight>()
            .ActivateOnEnter<BoundsOfSin>()
            .ActivateOnEnter<BoundsOfSinEnd>()
            .ActivateOnEnter<Spinelash>()
            .ActivateOnEnter<TerrorEyeBallOfFire>()
            .ActivateOnEnter<AbyssalBlaze>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1041, NameID = 14037)]
public class D99EminentGrief(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600, -300), new ArenaBoundsRect(20, 15));
