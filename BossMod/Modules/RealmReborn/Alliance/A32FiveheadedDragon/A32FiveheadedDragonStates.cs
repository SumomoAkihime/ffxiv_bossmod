namespace BossMod.RealmReborn.Alliance.A32FiveheadedDragon;

class A32FiveheadedDragonStates : StateMachineBuilder
{
    public A32FiveheadedDragonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WhiteBreath>()
            .ActivateOnEnter<BreathOfFire>()
            .ActivateOnEnter<IceFloor>()
            .ActivateOnEnter<BreathOfLight>()
            .ActivateOnEnter<BreathOfPoison>()
            .ActivateOnEnter<Stack>()
            .ActivateOnEnter<Discordance>()
            .ActivateOnEnter<Radiance>()
            .ActivateOnEnter<HeatWave>()
            .ActivateOnEnter<HeatWavePyretic>()
            .ActivateOnEnter<Heads>()
            .ActivateOnEnter<Prominence>()
            .ActivateOnEnter<Slimes>();
    }
}
