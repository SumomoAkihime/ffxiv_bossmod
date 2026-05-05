namespace BossMod.Dawntrail.Extreme.Ex8Enuo;

// Compatibility aliases for newer Reborn naming without changing local behavior.
sealed class ArenaChanges(BossModule module) : ArenaSwitcher(module);
sealed class DenseAiryEmptiness(BossModule module) : Emptiness(module);
sealed class GazeOfTheVoidAOE(BossModule module) : GazeOfTheVoid(module);
sealed class MeltdownAoE(BossModule module) : MeltdownBaited(module);
sealed class MeltdownWait(BossModule module) : ChainsOfCondemnation(module);
sealed class NaughtGrowsWildCharge(BossModule module) : ReturnToNothing(module);
