namespace BossMod.Heavensward.Alliance.A32FerdiadHollow;

class Blackbolt(BossModule module) : Components.StackWithCastTargets(module, AID.Blackbolt, 6, 8);
class Blackfire2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Blackfire2, 7);
class JestersReap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.JestersReap, new AOEShapeCone(13.4f, 60.Degrees()));
class JestersReward(BossModule module) : Components.SimpleAOEs(module, (uint)AID.JestersReward, new AOEShapeCone(31.4f, 90.Degrees()));
class JongleursX(BossModule module) : Components.SingleTargetCast(module, AID.JongleursX);
class PetrifyingEye(BossModule module) : Components.CastGaze(module, AID.PetrifyingEye);
class Flameflow1(BossModule module) : Components.RaidwideCast(module, AID.Flameflow1);
class AtmosAOE1(BossModule module) : Components.CastHint(module, AID.AtmosAOE1, "Big AOE");

[ModuleInfo(BossModuleInfo.Maturity.WIP, StatesType = typeof(A32FerdiadHollowStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 5509)]
public sealed class A32FerdiadHollow(WorldState ws, Actor primary) : BossModule(ws, primary, new(-350, 225), new ArenaBoundsCircle(30));
