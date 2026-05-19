namespace BossMod.Heavensward.Alliance.A32FerdiadHollow;

sealed class A32FerdiadHollowStates : StateMachineBuilder
{
    public A32FerdiadHollowStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Blackbolt>()
            .ActivateOnEnter<Blackfire2>()
            .ActivateOnEnter<JestersReap>()
            .ActivateOnEnter<JestersReward>()
            .ActivateOnEnter<JongleursX>()
            .ActivateOnEnter<PetrifyingEye>()
            .ActivateOnEnter<Flameflow1>()
            .ActivateOnEnter<AtmosAOE1>();
    }
}
