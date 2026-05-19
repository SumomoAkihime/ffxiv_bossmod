namespace BossMod.Heavensward.Alliance.A36DiabolosHollow;

class Shadethrust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shadethrust, new AOEShapeRect(43, 2.5f));
class HollowCamisado(BossModule module) : Components.SingleTargetCast(module, AID.HollowCamisado);
class HollowNightmare(BossModule module) : Components.CastGaze(module, AID.HollowNightmare);
class HollowOmen1(BossModule module) : Components.RaidwideCast(module, AID.HollowOmen1);
class HollowOmen2(BossModule module) : Components.RaidwideCast(module, AID.HollowOmen2);
class Blindside(BossModule module) : Components.StackWithCastTargets(module, AID.Blindside, 6, 8);
class EarthShaker2(BossModule module) : Components.SimpleProtean(module, AID.EarthShaker2, new AOEShapeCone(60, 15.Degrees()));
class HollowNight(BossModule module) : Components.StackWithCastTargets(module, AID.HollowNight, 8, 8);
class ParticleBeam2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ParticleBeam2, new AOEShapeCircle(5));
class ParticleBeam4(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ParticleBeam4, new AOEShapeCircle(5));

[ModuleInfo(BossModuleInfo.Maturity.WIP, StatesType = typeof(A36DiabolosHollowStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 5526)]
public sealed class A36DiabolosHollow(WorldState ws, Actor primary) : BossModule(ws, primary, new(-350, -445), new ArenaBoundsCircle(35));
