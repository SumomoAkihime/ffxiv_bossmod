namespace BossMod.Dawntrail.Alliance.A32Alexander;

// Naming-compatibility bridge for Reborn path/symbol references.
// Intentionally no ModuleInfo or duplicate A32Alexander module type here:
// a legacy A32Alexander class already exists elsewhere in this fork.
public class A32AlexanderResurrected(WorldState ws, Actor primary) : BossMod.Dawntrail.Alliance.A33AlexanderResurrected.A33AlexanderResurrected(ws, primary);
