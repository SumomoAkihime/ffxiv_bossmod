namespace BossMod.RealmReborn.Alliance.A34CloudofDarkness;

class A34CloudofDarknessStates : StateMachineBuilder
{
    public A34CloudofDarknessStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ZeroFormParticleBeam>()
            .ActivateOnEnter<FeintParticleBeam>()
            .ActivateOnEnter<ParticleBeamTowers>()
            .ActivateOnEnter<HyperchargedClouds>()
            .ActivateOnEnter<ParticleBeamEnrage>()
            .ActivateOnEnter<Shadowlurkers>()
            .ActivateOnEnter<Adds>();
    }
}
