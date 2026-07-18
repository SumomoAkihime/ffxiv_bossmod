namespace BossMod.Stormblood.Alliance.A35UltimaP2;

class A35UltimaP2States : StateMachineBuilder
{
    public A35UltimaP2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HolyIVBait>()
            .ActivateOnEnter<HolyIVSpread>()
            .ActivateOnEnter<Redemption>()
            .ActivateOnEnter<Auralight1>()
            .ActivateOnEnter<Auralight2>()
            .ActivateOnEnter<Bombardment>()
            .ActivateOnEnter<RayOfLight>()
            .ActivateOnEnter<Penultima>()
            .ActivateOnEnter<GrandCrossSafe>()
            .ActivateOnEnter<Embrace>()
            .ActivateOnEnter<LifeDrain>()
            .ActivateOnEnter<GrandCrossAOE>()
            .ActivateOnEnter<Holy>()
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<Cataclysm>();
    }
}
