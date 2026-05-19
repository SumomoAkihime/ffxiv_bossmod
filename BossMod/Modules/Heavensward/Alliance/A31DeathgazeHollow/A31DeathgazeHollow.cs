namespace BossMod.Heavensward.Alliance.A31DeathgazeHollow;

class DarkII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkII, new AOEShapeCone(50, 30.Degrees()));
class VoidDeath(BossModule module) : Components.StandardAOEs(module, AID.VoidDeath, 10);
class VoidBlizzardIIIAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidBlizzardIIIAOE, new AOEShapeCone(60, 10.Degrees()));
class VoidAeroII(BossModule module) : Components.SpreadFromCastTargets(module, AID.VoidAeroII, 5);

[ModuleInfo(BossModuleInfo.Maturity.WIP, StatesType = typeof(A31DeathgazeHollowStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 5507)]
public sealed class A31DeathgazeHollow(WorldState ws, Actor primary) : BossModule(ws, primary, new(300, 410), new ArenaBoundsRect(30, 15));
