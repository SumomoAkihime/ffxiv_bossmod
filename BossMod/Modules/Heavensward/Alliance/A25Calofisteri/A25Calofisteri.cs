namespace BossMod.Heavensward.Alliance.A25Calofisteri;

class AuraBurst(BossModule module) : Components.RaidwideCast(module, AID.AuraBurst);
class DepthCharge(BossModule module) : Components.ChargeAOEs(module, AID.DepthCharge, 5);
class Extension2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Extension2, new AOEShapeCircle(6));
class FeintParticleBeam1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FeintParticleBeam1, new AOEShapeCircle(3));
class Penetration(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Penetration, 50, kind: Kind.TowardsOrigin);
class Graft(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Graft, new AOEShapeCircle(5));

abstract class Haircut(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(65.5f, 90.Degrees()));
class Haircut1(BossModule module) : Haircut(module, (uint)AID.Haircut1);
class Haircut2(BossModule module) : Haircut(module, (uint)AID.Haircut2);

abstract class SplitEnd(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(25.5f, 22.5f.Degrees()));
class SplitEnd1(BossModule module) : SplitEnd(module, (uint)AID.SplitEnd1);
class SplitEnd2(BossModule module) : SplitEnd(module, (uint)AID.SplitEnd2);

[ModuleInfo(BossModuleInfo.Maturity.WIP, StatesType = typeof(A25CalofisteriStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4897)]
public sealed class A25Calofisteri(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -34), new ArenaBoundsCircle(29.5f));
