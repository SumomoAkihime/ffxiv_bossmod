namespace BossMod.Heavensward.Alliance.A24Ozma;

sealed class A24OzmaStates : StateMachineBuilder
{
    public A24OzmaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<MeteorImpact>()
            .ActivateOnEnter<HolyKB>()
            .ActivateOnEnter<Holy>()
            .ActivateOnEnter<ExecrationAOE>()
            .ActivateOnEnter<AccelerationBomb>();
    }
}
