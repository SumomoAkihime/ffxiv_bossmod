namespace BossMod.Heavensward.Alliance.A33ProtoUltima;

class AetherialPool(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AetherialPool, 40, kind: Kind.TowardsOrigin, stopAtWall: true);
class AetherochemicalFlare(BossModule module) : Components.RaidwideCast(module, AID.AetherochemicalFlare);

abstract class AetherochemicalLaser(BossModule module, AID aid) : Components.SimpleAOEs(module, (uint)aid, new AOEShapeRect(71, 4));
class AetherochemicalLaser1(BossModule module) : AetherochemicalLaser(module, AID.AetherochemicalLaser1);
class AetherochemicalLaser2(BossModule module) : AetherochemicalLaser(module, AID.AetherochemicalLaser2);
class AetherochemicalLaser3(BossModule module) : AetherochemicalLaser(module, AID.AetherochemicalLaser3);

class CitadelBuster2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CitadelBuster2, new AOEShapeRect(65.5f, 5));
class FlareStar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlareStar, new AOEShapeCircle(31));
class Rotoswipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rotoswipe, new AOEShapeCone(11, 60.Degrees()));
class WreckingBall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WreckingBall, new AOEShapeCircle(8));

[ModuleInfo(BossModuleInfo.Maturity.WIP, StatesType = typeof(A33ProtoUltimaStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 3780)]
public sealed class A33ProtoUltima(WorldState ws, Actor primary) : BossModule(ws, primary, new(-350, -50), new ArenaBoundsCircle(30));
