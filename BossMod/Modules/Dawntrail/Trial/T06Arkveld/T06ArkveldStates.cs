namespace BossMod.Dawntrail.Trial.T06Arkveld;

class GuardianArkveldStates : StateMachineBuilder
{
    public GuardianArkveldStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<Roar1>()
            .ActivateOnEnter<ForgedFury>()
            .ActivateOnEnter<ChainbladeBlowLines>()
            .ActivateOnEnter<WyvernsRadianceCleave>()
            .ActivateOnEnter<GuardianResonanceRect>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<Concentric1>()
            .ActivateOnEnter<Concentric2>()
            .ActivateOnEnter<Concentric3>()
            .ActivateOnEnter<Concentric4>()
            .ActivateOnEnter<WyvernsOuroblade>()
            .ActivateOnEnter<SteeltailThrust>()
            .ActivateOnEnter<WildEnergy>()
            .ActivateOnEnter<ChainbladeCharge>()
            .ActivateOnEnter<ResonanceTowerSmall>()
            .ActivateOnEnter<ResonanceTowerLarge>()
            .ActivateOnEnter<CrackedCrystalSmall>()
            .ActivateOnEnter<CrackedCrystalLarge>()
            .ActivateOnEnter<WyvernsVengeance>()
            .ActivateOnEnter<WyvernsWealAOE>()
            .ActivateOnEnter<WyvernsWealPulses>()
            .ActivateOnEnter<WyvernsWealIrregularCastLane>();
    }
}
