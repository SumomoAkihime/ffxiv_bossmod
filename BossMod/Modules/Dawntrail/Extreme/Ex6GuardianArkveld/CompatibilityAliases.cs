namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

// Compatibility wrappers for Reborn split-file naming.
sealed class Roar(BossModule module) : Components.CastCounterMulti(module, [AID.Roar1, AID.Roar2, AID.Roar3]);
sealed class ClamorousChase(BossModule module) : ClamorousBait(module);
sealed class ClamorousChaseBait(BossModule module) : ClamorousBait(module);
sealed class ClamorousChaseAOE(BossModule module) : ClamorousCleave(module);
sealed class GuardianResonanceTowers(BossModule module) : GuardianResonanceTowerLarge(module);
sealed class GuardianWyvernsSiegeflight(BossModule module) : GuardianResonance(module);
sealed class WyvernsRadianceCleave(BossModule module) : WyvernsRadianceSides(module);
sealed class WyvernsRadianceExaflare(BossModule module) : WyvernsRadianceExawave(module);
sealed class WyvernsRadianceExaflare1(BossModule module) : WyvernsRadianceExawave(module);
sealed class WyvernsRadianceExaflare2(BossModule module) : WyvernsRadianceCrystal(module);
sealed class WyvernsRadianceRush(BossModule module) : Rush(module);
sealed class WyvernsRadianceConcentric(BossModule module) : WyvernsRadianceQuake(module);
sealed class WyvernsRadianceCrackedCrystal(BossModule module) : WyvernsRadianceCrystal(module);
sealed class WyvernsWealAOE(BossModule module) : WyvernsWealCast(module);
sealed class WyvernsRadianceChainbladeCharge(BossModule module) : ChainbladeCharge(module);
sealed class ChainbladeBlow(BossModule module) : ChainbladeTail(module);
