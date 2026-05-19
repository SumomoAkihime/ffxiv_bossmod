namespace BossMod.Heavensward.Quest.MSQ.DivineIntervention;

public enum OID : uint
{
    Boss = 0x1010,
    IshgardianSteelChain = 0x102C,
    SerPaulecrainColdfire = 0x1011,
    ThunderPicket = 0xEC4,
    Helper = 0xE0F
}

public enum AID : uint
{
    LightningBolt = 3993,
    IronTempest = 1003,
    Overpower = 720,
    RingOfFrost = 1316,
    Rive = 1135,
    Heartstopper = 866,
    ThunderThrust = 3992,
}

class LightningBolt(BossModule module) : Components.ChargeAOEs(module, AID.LightningBolt, 2f);
class IronTempest(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IronTempest, 5.5f);
class Overpower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Overpower, new AOEShapeCone(6.5f, 45f.Degrees()));
class RingOfFrost(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RingOfFrost, 6.5f);
class Rive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rive, new AOEShapeRect(30.5f, 1f));
class Heartstopper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Heartstopper, new AOEShapeRect(3.5f, 1.5f));
class ThunderThrust(BossModule module) : Components.RaidwideCast(module, AID.ThunderThrust);

class SerGrinnauxStates : StateMachineBuilder
{
    public SerGrinnauxStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<IronTempest>()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<RingOfFrost>()
            .ActivateOnEnter<Rive>()
            .ActivateOnEnter<Heartstopper>()
            .ActivateOnEnter<ThunderThrust>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(SerGrinnauxStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67133, NameID = 3850)]
public class SerGrinnaux(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(20f))
{

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.SerPaulecrainColdfire), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.IshgardianSteelChain), ArenaColor.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (ref var e in hints.PotentialTargets.AsSpan())
            e.Priority = e.Actor.OID == (uint)OID.IshgardianSteelChain ? 1 : 0;
    }
}
