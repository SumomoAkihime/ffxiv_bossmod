namespace BossMod.Stormblood.Alliance.A32Agrias;

class DivineLight(BossModule module) : Components.RaidwideCast(module, (uint)AID.DivineLight);
class NorthswainsStrikeEphemeralKnight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NorthswainsStrikeEphemeralKnight, new AOEShapeRect(60, 3));
class CleansingFlameSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.CleansingFlameSpread, 6);
class HallowedBoltAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HallowedBoltAOE, 15);
class HallowedBoltDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HallowedBoltDonut, new AOEShapeDonut(15f, 30f));
class MortalBlow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MortalBlow, 30f);
class DivineRuination(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(60f, 3f), (uint)TetherID.Tether2, (uint)AID.DivineRuination2, (uint)OID.Boss, 8.1d);
class ThunderSlash(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ThunderSlash, new AOEShapeCone(60f, 45f.Degrees()), tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7916)]
public class A32Agrias(WorldState ws, Actor primary) : BossModule(ws, primary, new(600, -54), new ArenaBoundsCircle(30));
