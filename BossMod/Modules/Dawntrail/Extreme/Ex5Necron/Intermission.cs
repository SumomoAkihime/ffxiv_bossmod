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

// Extra compatibility aliases for Reborn Supported Bosses naming.
sealed class ChokingGrasp(BossModule module) : ChokingGraspInstant(module);
sealed class ChokingGraspBait1(BossModule module) : EndsEmbraceBait(module);
sealed class ChokingGraspBait2(BossModule module) : CenterChokingGrasp(module);
sealed class ChokingGraspMM(BossModule module) : DelayedChokingGrasp(module);
sealed class GrandCross(BossModule module) : GrandCrossRaidwide(module);
sealed class GrandCrossArenaChange(BossModule module) : GrandCrossArena(module);
sealed class GrandCrossBait(BossModule module) : Shock(module);
sealed class GrandCrossProx(BossModule module) : GrandCrossProximity(module);
sealed class GrandCrossRect(BossModule module) : GrandCrossLineCast(module);
sealed class GrandCrossRW(BossModule module) : GrandCrossRaidwide(module);
sealed class MementoMori(BossModule module) : MementoMoriLine(module);
sealed class Prisons(BossModule module) : JailHands(module);
sealed class Slow(BossModule module) : JailSlow(module);
sealed class TheEndsEmbrace(BossModule module) : EndsEmbrace(module);
