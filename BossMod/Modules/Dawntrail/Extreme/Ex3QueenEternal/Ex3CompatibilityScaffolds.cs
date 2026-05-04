namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

// Compatibility scaffolds for incremental upstream sync.
// Keep these until each mechanic file is ported with full behavior.

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

        Cast(id + 0x20, AID.Aethertithe, 1.0f, 3.0f);
        ComponentCondition<Aethertithe>(id + 0x21, 5.0f, c => c.AOE != null)
            .ActivateOnEnter<Aethertithe>();
        ComponentCondition<Aethertithe>(id + 0x22, 5.1f, c => c.NumCasts >= 1, "Cone 1")
            .ActivateOnEnter<Retribute>();
        ComponentCondition<Retribute>(id + 0x23, 2.9f, c => c.NumCasts > 0, "Line stacks 1");
        ComponentCondition<Aethertithe>(id + 0x24, 1.2f, c => c.AOE != null);
        ComponentCondition<Aethertithe>(id + 0x25, 5.1f, c => c.NumCasts >= 2, "Cone 2");
        ComponentCondition<Retribute>(id + 0x26, 2.9f, c => c.NumCasts > 2, "Line stacks 2");
        ComponentCondition<Aethertithe>(id + 0x27, 1.2f, c => c.AOE != null);
        ComponentCondition<Aethertithe>(id + 0x28, 5.1f, c => c.NumCasts >= 3, "Cone 3")
            .DeactivateOnExit<Aethertithe>();
        ComponentCondition<Retribute>(id + 0x29, 2.9f, c => c.NumCasts > 4, "Line stacks 3")
            .DeactivateOnExit<Retribute>();

        Cast(id + 0x40, AID.VirtualShiftWind, 2.0f, 5.0f, "Raidwide (wind platform)")
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x41, AID.LawsOfWind, 5.2f, 4.0f);
        ComponentCondition<Aeroquell>(id + 0x42, 0.1f, c => c.Active)
            .ActivateOnEnter<Aeroquell>();
        ComponentCondition<Aeroquell>(id + 0x43, 5.0f, c => !c.Active, "Party stacks")
            .DeactivateOnExit<Aeroquell>();
        CastStartMulti(id + 0x44, [AID.LegitimateForceFirstR, AID.LegitimateForceFirstL], 5.1f)
            .ActivateOnEnter<AeroquellTwister>();
        ComponentCondition<MissingLink>(id + 0x45, 0.8f, c => c.TethersAssigned, "Chains")
            .ActivateOnEnter<MissingLink>()
            .ActivateOnEnter<LegitimateForce>();
        CastEnd(id + 0x46, 7.2f, "Side 1");
        ComponentCondition<LegitimateForce>(id + 0x47, 3.1f, c => c.NumCasts > 1, "Side 2")
            .DeactivateOnExit<LegitimateForce>();
        ComponentCondition<WindOfChange>(id + 0x48, 3.2f, c => c.NumCasts > 0, "Knockback")
            .ActivateOnEnter<WindOfChange>()
            .DeactivateOnExit<WindOfChange>()
            .DeactivateOnExit<MissingLink>();
        Cast(id + 0x49, AID.WorldShatterP1, 3.0f, 5.0f, "Raidwide + platform end")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<AeroquellTwister>(id + 0x4A, 2.6f, c => !c.Sources(Module).Any())
            .DeactivateOnExit<AeroquellTwister>();

        Cast(id + 0x80, AID.VirtualShiftEarth, 4.0f, 5.0f, "Raidwide (earth platform)")
            .SetHint(StateMachine.StateHint.Raidwide);
        CastStart(id + 0x81, AID.LawsOfEarth, 5.2f)
            .ActivateOnEnter<VirtualShiftEarth>();
        CastEnd(id + 0x82, 4.0f);
        CastMulti(id + 0x83, [AID.LegitimateForceFirstR, AID.LegitimateForceFirstL], 3.2f, 8.0f, "Side 1")
            .ActivateOnEnter<LegitimateForce>();
        ComponentCondition<LegitimateForce>(id + 0x84, 3.1f, c => c.NumCasts > 1, "Side 2")
            .DeactivateOnExit<LegitimateForce>();
        ComponentCondition<LawsOfEarthBurst>(id + 0x85, 5.0f, c => c.NumCasts > 0, "Towers")
            .ActivateOnEnter<LawsOfEarthBurst1>()
            .DeactivateOnExit<LawsOfEarthBurst>();
        Cast(id + 0x86, AID.GravitationalEmpire, 5.2f, 7.0f)
            .ActivateOnEnter<GravityPillar>()
            .ActivateOnEnter<GravityRay>()
            .ActivateOnEnter<LawsOfEarthBurst2>();
        ComponentCondition<GravityRay>(id + 0x87, 1.0f, c => c.NumCasts > 0, "Defamations + Cones")
            .DeactivateOnExit<GravityPillar>()
            .DeactivateOnExit<GravityRay>();
        ComponentCondition<LawsOfEarthBurst>(id + 0x88, 0.8f, c => c.NumCasts > 0, "Towers")
            .DeactivateOnExit<LawsOfEarthBurst>();
        ComponentCondition<MeteorImpact>(id + 0x89, 5.5f, c => c.Active)
            .ActivateOnEnter<MeteorImpact>();
        ComponentCondition<MeteorImpact>(id + 0x8A, 6.1f, c => c.NumCasts > 0, "Meteors 1")
            .ActivateOnEnter<WeightyBlow>();
        ComponentCondition<MeteorImpact>(id + 0x8B, 1.0f, c => c.Active);
        ComponentCondition<MeteorImpact>(id + 0x8C, 6.1f, c => c.NumCasts > 0, "Meteors 2")
            .DeactivateOnExit<MeteorImpact>();
        Cast(id + 0x8D, AID.WeightyBlow, 2.0f, 5.0f);
        ComponentCondition<WeightyBlow>(id + 0x8E, 0.1f, c => c.NumCasts > 0, "LOS 1");
        ComponentCondition<WeightyBlow>(id + 0x8F, 3.1f, c => c.NumCasts > 2, "LOS 2");
        ComponentCondition<WeightyBlow>(id + 0xA0, 3.1f, c => c.NumCasts > 4, "LOS 3");
        ComponentCondition<WeightyBlow>(id + 0xA1, 3.1f, c => c.NumCasts > 6, "LOS 4")
            .DeactivateOnExit<WeightyBlow>();
        Cast(id + 0xA2, AID.WorldShatterP1, 0.7f, 5.0f, "Raidwide + platform end")
            .DeactivateOnExit<VirtualShiftEarth>()
            .SetHint(StateMachine.StateHint.Raidwide);

        Cast(id + 0x90, AID.VirtualShiftIce, 3.0f, 5.0f, "Raidwide (ice platform)")
            .ActivateOnEnter<VirtualShiftIce>()
            .SetHint(StateMachine.StateHint.Raidwide);
        CastStart(id + 0x91, AID.LawsOfIce, 5.2f);
        CastEnd(id + 0x92, 4.0f)
            .ActivateOnEnter<LawsOfIce>();
        ComponentCondition<LawsOfIce>(id + 0x93, 1.0f, c => c.NumCasts > 0, "Move");
        ComponentCondition<Rush>(id + 0x94, 4.3f, c => c.Activation != default)
            .ActivateOnEnter<Rush>()
            .DeactivateOnExit<LawsOfIce>();
        CastStartMulti(id + 0x95, [AID.LegitimateForceFirstR, AID.LegitimateForceFirstL], 11.9f);
        ComponentCondition<Rush>(id + 0x96, 0.3f, c => c.NumCasts > 0, "Stretch tethers")
            .DeactivateOnExit<Rush>();
        CastEnd(id + 0x97, 7.7f, "Side 1")
            .ActivateOnEnter<LegitimateForce>();
        ComponentCondition<LegitimateForce>(id + 0x98, 3.1f, c => c.NumCasts > 1, "Side 2")
            .DeactivateOnExit<LegitimateForce>();
        Cast(id + 0x99, AID.LawsOfIce, 6.1f, 4.0f)
            .ActivateOnEnter<LawsOfIce>();
        ComponentCondition<LawsOfIce>(id + 0x9A, 1.0f, c => c.NumCasts > 0, "Move")
            .ActivateOnEnter<IceDart>()
            .ActivateOnEnter<RaisedTribute>();
        ComponentCondition<IceDart>(id + 0x9B, 6.1f, c => c.NumCasts > 0, "Tethers + Line stack 1")
            .DeactivateOnExit<LawsOfIce>();
        ComponentCondition<IceDart>(id + 0x9C, 7.1f, c => c.NumCasts > 2, "Tethers + Line stack 2");
        ComponentCondition<IceDart>(id + 0x9D, 7.1f, c => c.NumCasts > 4, "Tethers + Line stack 3");
        ComponentCondition<IceDart>(id + 0x9E, 7.1f, c => c.NumCasts > 6, "Tethers + Line stack 4")
            .DeactivateOnExit<IceDart>()
            .DeactivateOnExit<RaisedTribute>();
        Cast(id + 0x9F, AID.WorldShatterP1, 3.1f, 5.0f, "Raidwide + platform end")
            .DeactivateOnExit<VirtualShiftIce>()
            .SetHint(StateMachine.StateHint.Raidwide);

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
        Cast(id - 0x10, AID.AuthorityEternal, 0.0f, 10.0f, "Intermission")
            .SetHint(StateMachine.StateHint.Raidwide);
        Targetable(id - 0x0F, false, 0.2f, "Boss disappears")
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ActorTargetable(id - 0x0E, _module.BossP2, true, 24.8f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ActorCast(id, _module.BossP2, AID.RadicalShift, 4.1f, 11.0f, true, "Raidwide")
            .ActivateOnEnter<RadicalShift>()
            .ActivateOnEnter<VirtualShiftIce>()
            .ActivateOnEnter<RadicalShiftAOE>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<RadicalShiftAOE>(id + 0x01, 5.2f, c => c.NumFinishedSpreads > 0, "Spread 1");
        ActorCast(id + 0x02, _module.BossP2, AID.RadicalShift, 3.0f, 11.0f, true, "Raidwide (platform change)")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<RadicalShiftAOE>(id + 0x03, 5.2f, c => c.NumFinishedSpreads > 1, "Spread 2")
            .DeactivateOnExit<RadicalShiftAOE>();
        ActorCast(id + 0x04, _module.BossP2, AID.WorldShatterP2, 3.0f, 5.0f, true, "Raidwide + platform end")
            .DeactivateOnExit<RadicalShift>()
            .DeactivateOnExit<VirtualShiftIce>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorCast(id + 0x80, _module.BossP2, AID.DimensionalDistortion, 7.2f, 4.0f, true)
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
        ComponentCondition<DyingMemory>(id + 0x100, 1.3f, c => c.NumCasts > 0, "Memory 1")
            .ActivateOnEnter<DyingMemory>()
            .DeactivateOnExit<DyingMemory>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DyingMemoryLast>(id + 0x110, 7.8f, c => c.NumCasts > 0, "Memory end")
            .ActivateOnEnter<DyingMemoryLast>()
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

        ActorCast(id + 0x200, _module.BossP2, AID.Preservation, 7.3f, 14.0f, true, "Enrage");
    }
}
