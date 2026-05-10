namespace BossMod.Dawntrail.Extreme.Ex8Enuo;

// Compatibility aliases for newer Reborn naming without changing local behavior.
sealed class ArenaChanges(BossModule module) : ArenaSwitcher(module);
sealed class DenseAiryEmptiness(BossModule module) : Emptiness(module);
sealed class GazeOfTheVoidAOE(BossModule module) : GazeOfTheVoid(module);
sealed class MeltdownAoE(BossModule module) : MeltdownBaited(module);
sealed class MeltdownWait(BossModule module) : ChainsOfCondemnation(module);
sealed class NaughtGrowsWildCharge(BossModule module) : ReturnToNothing(module);
sealed class AggressiveShadowAdd(BossModule module) : Shadows(module);
sealed class BeaconAdd(BossModule module) : Beacon(module);
sealed class CurseoftheFlesh(BossModule module) : Shadows(module);
sealed class DeepFreeze(BossModule module) : DeepFreezeRaidwide(module);
sealed class DeepFreezeFlares(BossModule module) : DeepFreezeFlare(module);
sealed class DrainTouch(BossModule module) : Shadows(module);
sealed class EmptyShadowTower(BossModule module) : EmptyShadow(module);
sealed class GazeOfTheVoidSoaks(BossModule module) : Burst(module);
sealed class LoomingEmptinessKillZone(BossModule module) : LoomingEmptiness(module);
sealed class LoomingShadowAdd(BossModule module) : LoomingShadow(module);
sealed class NaughtGrowsBossCircle(BossModule module) : NaughtGrowsCircle(module);
sealed class NaughtGrowsBossDonut(BossModule module) : NaughtGrowsDonut(module);
sealed class NaughtHunts(BossModule module) : EndlessChase(module);
sealed class SupportShadowAdds(BossModule module) : Shadows(module);
sealed class VacuumAOE(BossModule module) : Vacuum(module);
sealed class VacuumArc1(BossModule module) : SilentTorrentSmall(module);
sealed class VacuumArc2(BossModule module) : SilentTorrentMedium(module);
sealed class VacuumArc3(BossModule module) : SilentTorrentLarge(module);
sealed class VacuumTelegraph(BossModule module) : Vacuum(module);
sealed class VoidalTurbulanceCone(BossModule module) : VoidalTurbulence(module);
