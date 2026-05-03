namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

// Compatibility scaffolds for incremental upstream sync.
// Keep these until each mechanic file is ported with full behavior.

sealed class AbsoluteAuthority(BossModule module) : BossComponent(module);
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
    private readonly Ex3QueenEternal _module;

    public Ex3QueenEternalStates(Ex3QueenEternal module) : base(module)
    {
        _module = module;

        SimplePhase(0, Phase1, "P1")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || Module.PrimaryActor.HPMP.CurHP == 1 && (Module.PrimaryActor.CastInfo?.IsSpell(AID.AuthorityEternal) ?? false);
        SimplePhase(1, Phase2, "P2")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && (_module.BossP2()?.IsDeadOrDestroyed ?? true);
    }

    private void Phase1(uint id)
    {
        Cast(id, AID.ProsecutionOfWar, 8.0f, 5.0f, "Tankbuster")
            .ActivateOnEnter<ProsecutionOfWar>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<ProsecutionOfWar>(id + 0x10, 3.2f, c => c.NumCasts > 1)
            .DeactivateOnExit<ProsecutionOfWar>()
            .SetHint(StateMachine.StateHint.Tankbuster);

        Cast(id + 0x100, AID.RoyalDomain, 7.0f, 5.0f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        Cast(id + 0x200, AID.AuthorityEternal, 8.0f, 10.0f, "Phase transition");
        Targetable(id + 0x210, false, 0.2f, "Boss disappears")
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    private void Phase2(uint id)
    {
        ActorCast(id, _module.BossP2, AID.RadicalShift, 4.0f, 11.0f, true, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DyingMemory>(id + 0x100, 10.0f, c => c.NumCasts > 0, "Memory 1")
            .ActivateOnEnter<DyingMemory>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DyingMemoryLast>(id + 0x110, 8.0f, c => c.NumCasts > 0, "Memory end")
            .ActivateOnEnter<DyingMemoryLast>()
            .DeactivateOnExit<DyingMemory>()
            .DeactivateOnExit<DyingMemoryLast>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ActorCast(id + 0x200, _module.BossP2, AID.Preservation, 9.0f, 14.0f, true, "Enrage");
    }
}
