namespace BossMod.Dawntrail.Dungeon.D08StrayboroughDeadwalk.D081HisRoyalHeadnessLeonoggI;

public enum OID : uint
{
    Boss = 0x4183,
    NobleNoggin = 0x4205,
    Helper = 0x233C
}

public enum AID : uint
{
    MaliciousMist = 36529,
    FallingNightmare1 = 36532,
    FallingNightmare2 = 36536,
    SpiritedCharge = 36534,
    EvilSchemeFirst = 39683,
    EvilSchemeRest = 39684,
    LoomingNightmareFirst = 39686,
    LoomingNightmareRest = 39687,
    Scream = 36531
}

public enum IconID : uint
{
    LoomingNightmare = 197
}

class MaliciousMist(BossModule module) : Components.RaidwideCast(module, AID.MaliciousMist);
class FallingNightmare1(BossModule module) : Components.StandardAOEs(module, AID.FallingNightmare1, new AOEShapeCircle(2));
class FallingNightmare2(BossModule module) : Components.StandardAOEs(module, AID.FallingNightmare2, new AOEShapeCircle(2));
class SpiritedCharge(BossModule module) : Components.StandardAOEs(module, AID.SpiritedCharge, new AOEShapeRect(6, 1));
class Scream(BossModule module) : Components.StandardAOEs(module, AID.Scream, new AOEShapeCone(20, 30.Degrees()));
class LoomingNightmareFirst(BossModule module) : Components.StandardAOEs(module, AID.LoomingNightmareFirst, new AOEShapeCircle(4));
class LoomingNightmareRest(BossModule module) : Components.StandardAOEs(module, AID.LoomingNightmareRest, new AOEShapeCircle(4));
class EvilSchemeFirst(BossModule module) : Components.StandardAOEs(module, AID.EvilSchemeFirst, new AOEShapeCircle(4));
class EvilSchemeRest(BossModule module) : Components.StandardAOEs(module, AID.EvilSchemeRest, new AOEShapeCircle(4));

class D081HisRoyalHeadnessLeonoggIStates : StateMachineBuilder
{
    public D081HisRoyalHeadnessLeonoggIStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MaliciousMist>()
            .ActivateOnEnter<LoomingNightmareFirst>()
            .ActivateOnEnter<LoomingNightmareRest>()
            .ActivateOnEnter<EvilSchemeFirst>()
            .ActivateOnEnter<EvilSchemeRest>()
            .ActivateOnEnter<FallingNightmare1>()
            .ActivateOnEnter<FallingNightmare2>()
            .ActivateOnEnter<SpiritedCharge>()
            .ActivateOnEnter<Scream>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 981, NameID = 13073)]
public class D081HisRoyalHeadnessLeonoggI(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 150), new ArenaBoundsCircle(19.5f));
