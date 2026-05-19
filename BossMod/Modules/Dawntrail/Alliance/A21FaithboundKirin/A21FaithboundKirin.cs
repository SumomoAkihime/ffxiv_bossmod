namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

class StonegaIV(BossModule module) : Components.RaidwideCast(module, AID.StonegaIV);
class CrimsonRiddle(BossModule module) : Components.GroupedAOEs(module, [AID.CrimsonRiddleFront, AID.CrimsonRiddleBack], new AOEShapeCone(30, 90.Degrees()));

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058, NameID = 14053)]
public class A21FaithboundKirin(WorldState ws, Actor primary) : BossModule(ws, primary, new(-850, 780), new ArenaBoundsCircle(29.5f));
