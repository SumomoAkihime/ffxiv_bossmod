namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

// Reborn mechanic-name compatibility bridges for names that differ locally.
// These classes are inert unless explicitly activated by a state machine.
class ArenaChanges(BossModule module) : StormPhaseArena(module);
class AddPhase(BossModule module) : Adds(module);
class TasteOfThunderFire(BossModule module) : TasteOfThunder(module);
