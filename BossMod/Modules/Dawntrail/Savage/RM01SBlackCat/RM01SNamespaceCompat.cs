namespace BossMod.Dawntrail.Savage.M01SBlackCat;

// Compatibility bridge for Reborn namespace naming.
// IMPORTANT: no ModuleInfo and no extra mechanic/component implementations here.
public class M01SBlackCat(WorldState ws, Actor primary) : BossMod.Dawntrail.Savage.RM01SBlackCat.M01SBlackCat(ws, primary);
class M01SBlackCatStates(BossModule module) : BossMod.Dawntrail.Savage.RM01SBlackCat.M01SBlackCatStates(module);
