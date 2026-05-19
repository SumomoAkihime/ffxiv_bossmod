namespace BossMod.Heavensward.Alliance.A13Cuchulainn;

class CorrosiveBile1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CorrosiveBile1, new AOEShapeCone(25, 45.Degrees()));
class FlailingTentacles2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlailingTentacles2, new AOEShapeRect(32.5f, 3.5f));
class Beckon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Beckon, new AOEShapeCone(36.875f, 30.Degrees()));
class BileBelow(BossModule module) : Components.RaidwideCast(module, AID.BileBelow);
class Pestilence(BossModule module) : Components.RaidwideCast(module, AID.Pestilence);
class BlackLung(BossModule module) : Components.RaidwideCast(module, AID.BlackLung);
class GrandCorruption(BossModule module) : Components.RaidwideCast(module, AID.GrandCorruption);

[ModuleInfo(BossModuleInfo.Maturity.WIP, StatesType = typeof(A13CuchulainnStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.CFC, GroupID = 120, NameID = 4626)]
public class A13Cuchulainn(WorldState ws, Actor primary) : BossModule(ws, primary, new(288, 138.5f), new ArenaBoundsCircle(29.5f));
