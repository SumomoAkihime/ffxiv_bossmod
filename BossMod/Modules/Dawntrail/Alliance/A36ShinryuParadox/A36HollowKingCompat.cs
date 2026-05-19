namespace BossMod.Dawntrail.Alliance.A36HollowKing;

// Reborn-name compatibility bridge: keep all behavior in canonical A36ShinryuParadox.
public sealed class A36HollowKing(WorldState ws, Actor primary) : global::BossMod.Dawntrail.Alliance.A36ShinryuParadox.A36ShinryuParadox(ws, primary);
sealed class A36HollowKingStates(global::BossMod.Dawntrail.Alliance.A36ShinryuParadox.A36ShinryuParadox module) : global::BossMod.Dawntrail.Alliance.A36ShinryuParadox.A36ShinryuParadoxStates(module);
