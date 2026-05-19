namespace BossMod.Heavensward.Alliance.A36DiabolosHollow;

sealed class A36DiabolosHollowStates : StateMachineBuilder
{
    public A36DiabolosHollowStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Shadethrust>()
            .ActivateOnEnter<HollowCamisado>()
            .ActivateOnEnter<HollowNightmare>()
            .ActivateOnEnter<HollowOmen1>()
            .ActivateOnEnter<HollowOmen2>()
            .ActivateOnEnter<Blindside>()
            .ActivateOnEnter<EarthShaker2>()
            .ActivateOnEnter<HollowNight>()
            .ActivateOnEnter<ParticleBeam2>()
            .ActivateOnEnter<ParticleBeam4>();
    }
}
