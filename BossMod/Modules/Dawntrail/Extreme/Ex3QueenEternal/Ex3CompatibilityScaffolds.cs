namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

// Compatibility scaffolds for incremental upstream sync.
// Keep these until each mechanic file is ported with full behavior.

sealed class VirtualShiftIce(BossModule module) : BossComponent(module);

sealed class Ex3QueenEternalStates : StateMachineBuilder
{
    private readonly Ex3QueenEternal _module;

    public Ex3QueenEternalStates(Ex3QueenEternal module) : base(module)
    {
        _module = module;

        SimplePhase(0, Phase1, "P1")
            .ActivateOnEnter<ArenaChanges>()
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

        Cast(id + 0x100, AID.DivideAndConquer, 5.0f, 7.5f)
            .ActivateOnEnter<DivideAndConquerBait>()
            .ActivateOnEnter<DivideAndConquerAOE>();
        ComponentCondition<DivideAndConquerBait>(id + 0x110, 0.1f, c => c.NumCasts > 0, "Protean 1");
        ComponentCondition<DivideAndConquerBait>(id + 0x120, 7.0f, c => c.NumCasts >= 8, "Protean 8")
            .DeactivateOnExit<DivideAndConquerBait>();
        ComponentCondition<DivideAndConquerAOE>(id + 0x130, 4.0f, c => c.NumCasts > 0, "Lines")
            .DeactivateOnExit<DivideAndConquerAOE>();

        Cast(id + 0x140, AID.AbsoluteAuthorityPuddles, 3.0f, 10.0f)
            .ActivateOnEnter<AbsoluteAuthorityPuddles>();
        ComponentCondition<AbsoluteAuthorityPuddles>(id + 0x150, 0.1f, c => c.Casters.Count > 0, "Puddles bait");
        ComponentCondition<AbsoluteAuthorityExpansionBoot>(id + 0x160, 10.0f, c => c.NumCasts > 0, "Spread/stack")
            .ActivateOnEnter<AbsoluteAuthorityExpansionBoot>()
            .ActivateOnEnter<AbsoluteAuthorityHeel>()
            .DeactivateOnExit<AbsoluteAuthorityPuddles>()
            .DeactivateOnExit<AbsoluteAuthorityExpansionBoot>();
        ComponentCondition<AbsoluteAuthorityHeel>(id + 0x170, 4.0f, c => c.NumCasts > 0, "Stack")
            .DeactivateOnExit<AbsoluteAuthorityHeel>();
        ComponentCondition<AbsoluteAuthorityKnockback>(id + 0x175, 6.9f, c => c.NumCasts > 0, "Knockback")
            .ActivateOnEnter<AbsoluteAuthorityKnockback>()
            .DeactivateOnExit<AbsoluteAuthorityKnockback>();

        CastMulti(id + 0x180, [AID.LegitimateForceFirstR, AID.LegitimateForceFirstL], 3.0f, 8.0f, "Side 1")
            .ActivateOnEnter<LegitimateForce>();
        ComponentCondition<LegitimateForce>(id + 0x190, 3.1f, c => c.NumCasts > 1, "Side 2")
            .DeactivateOnExit<LegitimateForce>();

        Cast(id + 0x200, AID.RoyalDomain, 2.0f, 5.0f, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        Cast(id + 0x210, AID.Coronation, 3.1f, 3.0f)
            .ActivateOnEnter<Coronation>();
        ComponentCondition<Coronation>(id + 0x211, 2.1f, c => c.Groups.Count > 0);
        Cast(id + 0x212, AID.AtomicRay, 1.1f, 3.0f);
        ComponentCondition<AtomicRay>(id + 0x213, 1.2f, c => c.Active)
            .ActivateOnEnter<AtomicRay>();
        ComponentCondition<Coronation>(id + 0x214, 4.9f, c => c.NumCasts > 0, "Coronation")
            .DeactivateOnExit<Coronation>();
        ComponentCondition<AtomicRay>(id + 0x215, 1.1f, c => c.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<AtomicRay>();

        Cast(id + 0x300, AID.AuthorityEternal, 8.0f, 10.0f, "Phase transition");
        Targetable(id + 0x310, false, 0.2f, "Boss disappears")
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    private void Phase2(uint id)
    {
        ActorCast(id, _module.BossP2, AID.RadicalShift, 4.0f, 11.0f, true, "Raidwide")
            .ActivateOnEnter<RadicalShift>()
            .ActivateOnEnter<RadicalShiftAOE>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<RadicalShiftAOE>(id + 0x01, 5.2f, c => c.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<RadicalShiftAOE>();
        ActorCast(id + 0x80, _module.BossP2, AID.DimensionalDistortion, 7.2f, 5.0f, true)
            .ActivateOnEnter<DimensionalDistortion>();
        ComponentCondition<DimensionalDistortion>(id + 0x81, 1.0f, c => c.NumCasts > 0, "Exaflares start");
        ActorCast(id + 0x90, _module.BossP2, AID.TyrannysGrasp, 5.2f, 5.0f, true, "Front half cleave")
            .ActivateOnEnter<TyrannysGraspAOE>()
            .ActivateOnEnter<TyrannysGraspTowers>()
            .DeactivateOnExit<DimensionalDistortion>()
            .DeactivateOnExit<TyrannysGraspAOE>();
        ComponentCondition<TyrannysGraspTowers>(id + 0x91, 1.2f, c => c.NumCasts >= 1, "Tankbuster tower 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<TyrannysGraspTowers>(id + 0x92, 2.7f, c => c.NumCasts >= 2, "Tankbuster tower 2")
            .DeactivateOnExit<TyrannysGraspTowers>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<DyingMemory>(id + 0x100, 10.0f, c => c.NumCasts > 0, "Memory 1")
            .ActivateOnEnter<DyingMemory>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DyingMemoryLast>(id + 0x110, 8.0f, c => c.NumCasts > 0, "Memory end")
            .ActivateOnEnter<DyingMemoryLast>()
            .DeactivateOnExit<DyingMemory>()
            .DeactivateOnExit<DyingMemoryLast>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ActorCastStart(id + 0x120, _module.BossP2, AID.RoyalBanishment, 3.1f, true)
            .ActivateOnEnter<RoyalBanishment>();
        ActorCastEnd(id + 0x121, _module.BossP2, 5.0f, true);
        ComponentCondition<RoyalBanishment>(id + 0x130, 0.8f, c => c.NumCasts > 0, "Line stack 1");
        ComponentCondition<RoyalBanishment>(id + 0x140, 6.0f, c => c.NumCasts >= 7);
        ComponentCondition<RoyalBanishment>(id + 0x150, 3.0f, c => c.NumCasts >= 8, "Line stack 8")
            .DeactivateOnExit<RoyalBanishment>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ActorCast(id + 0x200, _module.BossP2, AID.Preservation, 9.0f, 14.0f, true, "Enrage");
    }
}
