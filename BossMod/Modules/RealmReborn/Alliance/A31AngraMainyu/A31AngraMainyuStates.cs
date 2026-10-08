namespace BossMod.RealmReborn.Alliance.A31AngraMainyu;

class A31AngraMainyuStates : StateMachineBuilder
{
    public A31AngraMainyuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Stare>()
            .ActivateOnEnter<DoubleVision>()
            .ActivateOnEnter<MortalGaze>()
            .ActivateOnEnter<MortalGazeHelper>()
            .ActivateOnEnter<DoomPads>()
            .ActivateOnEnter<Level100Flare>()
            .ActivateOnEnter<Level150Death>()
            .ActivateOnEnter<Roulette>()
            .ActivateOnEnter<EyesOnMe>()
            .ActivateOnEnter<Paralyze>()
            .ActivateOnEnter<Adds>();
    }
}
