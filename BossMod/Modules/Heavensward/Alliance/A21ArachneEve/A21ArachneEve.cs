namespace BossMod.Heavensward.Alliance.A21ArachneEve;

class DarkSpike(BossModule module) : Components.SingleTargetCast(module, AID.DarkSpike);
class SilkenSpray(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SilkenSpray, new AOEShapeCone(24.5f, 30f.Degrees()));
class ShadowBurst(BossModule module) : Components.StackWithCastTargets(module, AID.ShadowBurst, 6, 8);
class SpiderThread(BossModule module) : Components.SpreadFromCastTargets(module, AID.SpiderThread, 6);
sealed class Pitfall(BossModule module) : Components.StandardAOEs(module, AID.Pitfall1, 20f);
class FrondAffeared(BossModule module) : Components.CastGaze(module, AID.FrondAffeared);
class TheWidowsEmbrace(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TheWidowsEmbrace, 18f, kind: Kind.TowardsOrigin, stopAtWall: true);
class TheWidowsKiss(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TheWidowsKiss, 4, kind: Kind.TowardsOrigin, stopAtWall: true);

[ModuleInfo(BossModuleInfo.Maturity.WIP, StatesType = typeof(A21ArachneEveStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4871)]
public sealed class A21ArachneEve(WorldState ws, Actor primary) : BossModule(ws, primary, new(16f, -55f), new ArenaBoundsCircle(29.5f));
