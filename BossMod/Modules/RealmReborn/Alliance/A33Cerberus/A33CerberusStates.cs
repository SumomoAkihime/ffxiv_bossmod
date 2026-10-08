namespace BossMod.RealmReborn.Alliance.A33Cerberus;

class A33CerberusStates : StateMachineBuilder
{
    public A33CerberusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TailBlow>()
            .ActivateOnEnter<Slabber>()
            .ActivateOnEnter<Mini>()
            .ActivateOnEnter<SulphurousBreath>()
            .ActivateOnEnter<SulphurousBreathHelper>()
            .ActivateOnEnter<HoundOutOfHell>()
            .ActivateOnEnter<LightningBoltCharge>()
            .ActivateOnEnter<HexEye>()
            .ActivateOnEnter<Ululation>()
            .ActivateOnEnter<Wolfsbane>()
            .ActivateOnEnter<GastricJuiceAdd>()
            .ActivateOnEnter<Electrons>()
            .ActivateOnEnter<StomachAdds>()
            .ActivateOnEnter<BellyArena>();
    }
}
