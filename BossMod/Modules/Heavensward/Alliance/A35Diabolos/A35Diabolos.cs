namespace BossMod.Heavensward.Alliance.A35Diabolos;

class Nightmare(BossModule module) : Components.CastGaze(module, AID.Nightmare);
class NightTerror(BossModule module) : Components.StackWithCastTargets(module, AID.NightTerror, 10, 8);
class RuinousOmen1(BossModule module) : Components.RaidwideCast(module, AID.RuinousOmen1);
class RuinousOmen2(BossModule module) : Components.RaidwideCast(module, AID.RuinousOmen2);
class UltimateTerror(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UltimateTerror, new AOEShapeDonut(5, 19.5f));

[ModuleInfo(BossModuleInfo.Maturity.WIP, StatesType = typeof(A35DiabolosStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 5526)]
public sealed class A35Diabolos(WorldState ws, Actor primary) : BossModule(ws, primary, new(-350, -445), new ArenaBoundsCircle(35));
