namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

// Compatibility scaffolds for incremental upstream sync.
// Keep these until each mechanic file is ported with full behavior.

sealed class AbsoluteAuthority(BossModule module) : BossComponent(module);
sealed class Aethertithe(BossModule module) : BossComponent(module);
sealed class ArenaChanges(BossModule module) : BossComponent(module);
sealed class Coronation(BossModule module) : BossComponent(module);
sealed class DimensionalDistortion(BossModule module) : BossComponent(module);
sealed class DivideAndConquer(BossModule module) : BossComponent(module);
sealed class LegitimateForce(BossModule module) : BossComponent(module);
sealed class RadicalShift(BossModule module) : BossComponent(module);
sealed class RoyalBanishment(BossModule module) : BossComponent(module);
sealed class TyrannysGrasp(BossModule module) : BossComponent(module);
sealed class VirtualShiftEarth(BossModule module) : BossComponent(module);
sealed class VirtualShiftIce(BossModule module) : BossComponent(module);
sealed class VirtualShiftWind(BossModule module) : BossComponent(module);

sealed class Ex3QueenEternalStates : StateMachineBuilder
{
    public Ex3QueenEternalStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}
