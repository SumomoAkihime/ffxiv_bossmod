namespace BossMod.Dawntrail.Dungeon.D08StrayboroughDeadwalk.D082JackInThePot;

public enum OID : uint
{
    Boss = 0x41CA,
    StrayPhantagenitrix = 0x41D2,
    SpectralSamovar = 0x41CB,
    Helper = 0x233C
}

public enum AID : uint
{
    TricksomeTreat = 36720,
    PipingPour = 36723,
    LastDrop = 36726,
    SordidSteam = 36725
}

class TricksomeTreat(BossModule module) : Components.StandardAOEs(module, AID.TricksomeTreat, new AOEShapeCircle(19));
class PipingPour(BossModule module) : Components.StandardAOEs(module, AID.PipingPour, new AOEShapeCircle(8));
class SordidSteam(BossModule module) : Components.RaidwideCast(module, AID.SordidSteam);
class LastDrop(BossModule module) : Components.SingleTargetCast(module, AID.LastDrop);

class D082JackInThePotStates : StateMachineBuilder
{
    public D082JackInThePotStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TricksomeTreat>()
            .ActivateOnEnter<PipingPour>()
            .ActivateOnEnter<SordidSteam>()
            .ActivateOnEnter<LastDrop>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 981, NameID = 12760)]
public class D082JackInThePot(WorldState ws, Actor primary) : BossModule(ws, primary, new(17, -170), new ArenaBoundsCircle(19.5f));
