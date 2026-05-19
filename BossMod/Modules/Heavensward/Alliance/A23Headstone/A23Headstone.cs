namespace BossMod.Heavensward.Alliance.A23Headstone;

class TremblingEpigraph(BossModule module) : Components.RaidwideCast(module, AID.TremblingEpigraph);
class FlaringEpigraph(BossModule module) : Components.RaidwideCast(module, AID.FlaringEpigraph);
class BigBurst(BossModule module) : Components.RaidwideCast(module, AID.BigBurst);

[ModuleInfo(BossModuleInfo.Maturity.WIP, StatesType = typeof(A23HeadstoneStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4868)]
public sealed class A23Headstone(WorldState ws, Actor primary) : BossModule(ws, primary, new(-168.5f, 225f), new ArenaBoundsCircle(29.5f));
