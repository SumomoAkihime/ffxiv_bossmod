namespace BossMod.Heavensward.Alliance.A31DeathgazeHollow;

class A31DeathgazeHollowStates : StateMachineBuilder
{
    public A31DeathgazeHollowStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<VoidDeath>()
            .ActivateOnEnter<VoidAeroII>()
            .ActivateOnEnter<VoidBlizzardIIIAOE>();
    }
}
