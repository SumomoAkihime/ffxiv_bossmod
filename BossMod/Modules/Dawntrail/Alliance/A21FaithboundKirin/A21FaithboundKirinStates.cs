namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class A21FaithboundKirinStates : StateMachineBuilder
{
    public A21FaithboundKirinStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SummonShijin>()
            .ActivateOnEnter<StonegaIV>()
            .ActivateOnEnter<CrimsonRiddle>()
            .ActivateOnEnter<StonegaIII>()
            .ActivateOnEnter<StonegaIII2>()
            .ActivateOnEnter<Quake2>()
            .ActivateOnEnter<Quake>()
            .ActivateOnEnter<ShatteringStomp>()
            .ActivateOnEnter<EastwindWheel>()
            .ActivateOnEnter<SynchronizedStrikeSmite>()
            .ActivateOnEnter<Wringer>()
            .ActivateOnEnter<StrikingSmiting>()
            .ActivateOnEnter<MightyGrip>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<StandingFirm>()
            .ActivateOnEnter<ByakkoWalls>()
            .ActivateOnEnter<GloamingGleam>()
            .ActivateOnEnter<RazorFang>()
            .ActivateOnEnter<VermilionFlight>()
            .ActivateOnEnter<ArmOfPurgatory>()
            .ActivateOnEnter<MoontideFont>()
            .ActivateOnEnter<MidwinterMarchNorthernCurrent>()
            .ActivateOnEnter<ArenaChanges>();
    }
}
