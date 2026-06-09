using Ex5AID = BossMod.Dawntrail.Extreme.Ex5Necron.AID;
using Ex5IconID = BossMod.Dawntrail.Extreme.Ex5Necron.IconID;
using Ex5Module = BossMod.Dawntrail.Extreme.Ex5Necron.Ex5Necron;
using Ex5OID = BossMod.Dawntrail.Extreme.Ex5Necron.OID;
using Ex5SID = BossMod.Dawntrail.Extreme.Ex5Necron.SID;
using Ex5States = BossMod.Dawntrail.Extreme.Ex5Necron.Ex5NecronStates;

namespace BossMod.Dawntrail.Trial.T05Necron;

[ModuleInfo(BossModuleInfo.Maturity.AISupport, StatesType = typeof(Ex5States), ObjectIDType = typeof(Ex5OID), ActionIDType = typeof(Ex5AID), StatusIDType = typeof(Ex5SID), IconIDType = typeof(Ex5IconID), PrimaryActorOID = (uint)Ex5OID.Boss, Expansion = BossModuleInfo.Expansion.Dawntrail, Category = BossModuleInfo.Category.Trial, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1061, NameID = 14093)]
public class T05Necron(WorldState ws, Actor primary) : Ex5Module(ws, primary);
