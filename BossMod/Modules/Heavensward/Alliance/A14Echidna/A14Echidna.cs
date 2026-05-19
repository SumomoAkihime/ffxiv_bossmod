namespace BossMod.Heavensward.Alliance.A14Echidna;

class SickleStrike(BossModule module) : Components.SingleTargetCast(module, AID.SickleStrike);

abstract class SickleSlash(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(18.5f, 30));
class SickleSlash1(BossModule module) : SickleSlash(module, (uint)AID.SickleSlash1);
class SickleSlash2(BossModule module) : SickleSlash(module, (uint)AID.SickleSlash2);

class AbyssalReaper(BossModule module) : Components.StandardAOEs(module, AID.AbyssalReaper, 14);
class AbyssalReaperKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AbyssalReaper, 5, stopAtWall: true);
class Petrifaction1(BossModule module) : Components.CastGaze(module, AID.Petrifaction1);
class Petrifaction2(BossModule module) : Components.CastGaze(module, AID.Petrifaction2);
class Gehenna(BossModule module) : Components.RaidwideCast(module, AID.Gehenna);
class BloodyHarvest(BossModule module) : Components.StandardAOEs(module, AID.BloodyHarvest, 12);
class Deathstrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Deathstrike, new AOEShapeRect(62, 3));
class FlameWreath(BossModule module) : Components.StandardAOEs(module, AID.FlameWreath, 18);
class SerpentineStrike(BossModule module) : Components.StandardAOEs(module, AID.SerpentineStrike, 20);

[ModuleInfo(BossModuleInfo.Maturity.WIP, StatesType = typeof(A14EchidnaStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.CFC, GroupID = 120, NameID = 4631)]
public class A14Echidna(WorldState ws, Actor primary) : BossModule(ws, primary, new(288, -126), new ArenaBoundsCircle(29.5f));
