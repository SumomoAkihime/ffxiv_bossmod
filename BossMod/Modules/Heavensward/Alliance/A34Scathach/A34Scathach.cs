namespace BossMod.Heavensward.Alliance.A34Scathach;

class ThirtySouls(BossModule module) : Components.RaidwideCast(module, AID.ThirtySouls);
class ThirtyArrows1(BossModule module) : Components.StandardAOEs(module, AID.ThirtyArrows1, 8);
class ThirtyArrows2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThirtyArrows2, new AOEShapeRect(35.5f, 4));
class Shadespin2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shadespin2, new AOEShapeCone(30, 45.Degrees()));
class ThirtyThorns4(BossModule module) : Components.StandardAOEs(module, AID.ThirtyThorns4, 8);

[ModuleInfo(BossModuleInfo.Maturity.WIP, StatesType = typeof(A34ScathachStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 5515)]
public sealed class A34Scathach(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -50), new ArenaBoundsCircle(30));
