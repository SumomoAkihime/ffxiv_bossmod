namespace BossMod.Dawntrail.Trial.T07Doomtrain;

sealed class T07DoomtrainStates : StateMachineBuilder
{
    public T07DoomtrainStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CarGeometry>()
            .ActivateOnEnter<LightningBurstTankBuster>()
            .ActivateOnEnter<LightningExpress>()
            .ActivateOnEnter<WindpipeDrawIn>()
            .ActivateOnEnter<Blastpipe>()
            .ActivateOnEnter<UnlimitedExpress>()
            .ActivateOnEnter<PlasmaBeam>()
            .ActivateOnEnter<ElectrayLower>()
            .ActivateOnEnter<ElectrayUpper>()
            .ActivateOnEnter<ThunderousBreathLowerDeck>()
            .ActivateOnEnter<HeadlightUpperDeck>()
            .ActivateOnEnter<HailOfThunder>()
            .ActivateOnEnter<Derail>()
            .ActivateOnEnter<DerailmentSiegeSpread>()
            .ActivateOnEnter<DerailmentSiegeStack>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<RunawayTrain>()
            .ActivateOnEnter<RunawayTrainRaidwide>()
            .ActivateOnEnter<AetherSurge>()
            .ActivateOnEnter<AetherialRay>()
            .ActivateOnEnter<AetherIntermissionAdds>()
            .ActivateOnEnter<DoomtrainIntermissionAdds>()
            .ActivateOnEnter<GhostTrainAdds>()
            .ActivateOnEnter<ArcaneRevelation>();
    }
}
