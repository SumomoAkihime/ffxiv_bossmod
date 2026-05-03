namespace BossMod.Dawntrail.Extreme.Ex5Necron;

// Compatibility wrappers for Reborn split-file naming.
sealed class FearOfDeathAOE2(BossModule module) : FearOfDeathAdds(module);
sealed class SpreadingFearEnrage(BossModule module) : JailEnrage(module);
sealed class SpreadingFearInterrupt(BossModule module) : JailInterrupt(module);
sealed class ChokingGraspTB(BossModule module) : JailBuster(module);
sealed class ChokingGraspHealer(BossModule module) : JailGrasp(module);
sealed class ChillingFingers(BossModule module) : JailSlow(module);
sealed class ChokingGrasp3(BossModule module) : JailGrasp(module);
sealed class LimitBreakAdds(BossModule module) : IcyHandsAdds(module);
sealed class PrisonAdds(BossModule module) : JailHands(module);
