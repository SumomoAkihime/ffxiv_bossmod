using Ex3AID = BossMod.Dawntrail.Extreme.Ex3QueenEternal.AID;
using Ex3IconID = BossMod.Dawntrail.Extreme.Ex3QueenEternal.IconID;
using Ex3Module = BossMod.Dawntrail.Extreme.Ex3QueenEternal.Ex3QueenEternal;
using Ex3OID = BossMod.Dawntrail.Extreme.Ex3QueenEternal.OID;
using Ex3SID = BossMod.Dawntrail.Extreme.Ex3QueenEternal.SID;
using Ex3States = BossMod.Dawntrail.Extreme.Ex3QueenEternal.Ex3QueenEternalStates;

namespace BossMod.Dawntrail.Trial.T03QueenEternal;

[ModuleInfo(BossModuleInfo.Maturity.AISupport, StatesType = typeof(Ex3States), ObjectIDType = typeof(Ex3OID), ActionIDType = typeof(Ex3AID), StatusIDType = typeof(Ex3SID), IconIDType = typeof(Ex3IconID), PrimaryActorOID = (uint)Ex3OID.BossP1, Expansion = BossModuleInfo.Expansion.Dawntrail, Category = BossModuleInfo.Category.Trial, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 984, NameID = 13029)]
public class T03QueenEternal(WorldState ws, Actor primary) : Ex3Module(ws, primary);
