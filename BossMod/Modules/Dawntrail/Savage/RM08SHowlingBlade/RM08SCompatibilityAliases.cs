namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

// Reborn naming compatibility shims.
// These types are not wired into the state machine here; they only provide symbol-level compatibility.
class ArenaChanges(BossModule module) : P2Arena(module);
class ElementalPurge(BossModule module) : ElementalPurgeBind(module);
class MillenialDecay(BossModule module) : MillennialDecay(module);
class RiseOfTheHuntersBlade(BossModule module) : LoneWolfTethers(module);
