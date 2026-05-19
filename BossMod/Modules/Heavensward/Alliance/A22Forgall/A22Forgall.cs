namespace BossMod.Heavensward.Alliance.A22Forgall;

class BrandOfTheFallen(BossModule module) : Components.StackWithCastTargets(module, AID.BrandOfTheFallen, 6, 8);
class MegiddoFlame2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MegiddoFlame2, new AOEShapeRect(52.6f, 4));
class DarkEruption2(BossModule module) : Components.StandardAOEs(module, AID.DarkEruption2, 6);
class MortalRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MortalRay, new AOEShapeCone(20, 22.5f.Degrees()));
class Mow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mow, new AOEShapeCone(13.8f, 60.Degrees()));
class TailDrive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailDrive, new AOEShapeCone(30, 45.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, StatesType = typeof(A22ForgallStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4878)]
public sealed class A22Forgall(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300.00003f, -416.49481f), new ArenaBoundsCircle(29.5f));
