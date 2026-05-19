namespace BossMod.Heavensward.Alliance.A34Scathach;

class A34ScathachStates : StateMachineBuilder
{
    public A34ScathachStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThirtySouls>()
            .ActivateOnEnter<ThirtyArrows1>()
            .ActivateOnEnter<ThirtyArrows2>()
            .ActivateOnEnter<Shadespin2>()
            .ActivateOnEnter<ThirtyThorns4>();
    }
}
