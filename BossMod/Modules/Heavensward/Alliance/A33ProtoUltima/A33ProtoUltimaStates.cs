namespace BossMod.Heavensward.Alliance.A33ProtoUltima;

sealed class A33ProtoUltimaStates : StateMachineBuilder
{
    public A33ProtoUltimaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetherialPool>()
            .ActivateOnEnter<WreckingBall>()
            .ActivateOnEnter<AetherochemicalFlare>()
            .ActivateOnEnter<AetherochemicalLaser1>()
            .ActivateOnEnter<AetherochemicalLaser2>()
            .ActivateOnEnter<AetherochemicalLaser3>()
            .ActivateOnEnter<CitadelBuster2>()
            .ActivateOnEnter<FlareStar>()
            .ActivateOnEnter<Rotoswipe>();
    }
}
