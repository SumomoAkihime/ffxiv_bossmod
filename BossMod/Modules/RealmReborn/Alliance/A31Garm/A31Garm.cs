namespace BossMod.RealmReborn.Alliance.A31Garm;

public enum OID : uint
{
    Boss = 0xD9C, // R3.700, x3
    Helper = 0x1B2, // R0.500
    TwoHeadedDragon = 0xD9D, // R4.000
    SacrificedNinja = 0xD9F, // R0.500
    SacrificedKunoichi = 0xD9E, // R0.500
    BurnPuddle = 0x1E9701, // R0.500, EventObj type, (EventState 7 = inactive)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/SacrificedNinja/SacrificedKunoichi->player, no cast, single-target
    TheDragonsVoice = 3344, // Boss->self, 4.5s cast, range 30 circle
    TheRamsVoice = 3343, // Boss->self, 3.0s cast, range 6+R circle
    MarrowDrainLeft = 3340, // Boss->self, 3.0s cast, range 6+R 120-degree cone
    MarrowDrainMiddle = 3341, // Boss->self, 3.0s cast, range 6+R 120-degree cone
    MarrowDrainRight = 3342, // Boss->self, 3.0s cast, range 6+R 120-degree cone
    BurnPuddle = 3410, // Helper->location, 10.0s cast, range 14 circle

    MeanThrash = 3345, // TwoHeadedDragon->self, 2.0s cast, range 6+R 120-degree cone
    AutoAttackDragon = 682, // TwoHeadedDragon->players, no cast, range 6+R cone
    Diarchy = 3346, // TwoHeadedDragon->self, 0.5s cast, range 9+R cone
    BallOfFire = 3347, // TwoHeadedDragon->location, 3.0s cast, range 6 circle
    BallOfIce = 3348, // TwoHeadedDragon->location, 3.0s cast, range 6 circle
}

class MarrowDrain(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.MarrowDrainLeft, (uint)AID.MarrowDrainMiddle, (uint)AID.MarrowDrainRight], new AOEShapeCone(9.7f, 60.Degrees()));
class TheDragonsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheDragonsVoice, new AOEShapeDonut(8, 30));
class TheDragonsVoiceInterrupt(BossModule module) : Components.CastInterruptHint(module, (uint)AID.TheDragonsVoice);
class TheRamsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheRamsVoice, new AOEShapeCircle(9));
class TheRamsVoiceInterrupt(BossModule module) : Components.CastInterruptHint(module, (uint)AID.TheRamsVoice);
class BurnPuddle(BossModule module) : Components.VoidzoneAtCastTarget(module, 14, (uint)AID.BurnPuddle, m => m.Enemies((uint)OID.BurnPuddle).Where(a => a.EventState != 7), 0);
class MeanThrash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeanThrash, new AOEShapeCone(10, 60.Degrees()));
class Diarchy(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Diarchy, new AOEShapeCone(13, 45.Degrees()));
class BallOfFire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BallOfFire, 6);
class BallOfIce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BallOfIce, 6);
class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.TwoHeadedDragon, (uint)OID.SacrificedNinja, (uint)OID.SacrificedKunoichi]);

class A31GarmStates : StateMachineBuilder
{
    public A31GarmStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MarrowDrain>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<TheDragonsVoiceInterrupt>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<TheRamsVoiceInterrupt>()
            .ActivateOnEnter<BurnPuddle>()
            .ActivateOnEnter<MeanThrash>()
            .ActivateOnEnter<Diarchy>()
            .ActivateOnEnter<BallOfFire>()
            .ActivateOnEnter<BallOfIce>()
            .ActivateOnEnter<Adds>()
            .Raw.Update = () =>
            {
                var bossesDead = Module.Enemies((uint)OID.Boss).All(b => b.IsDeadOrDestroyed);
                var dragon = Module.Enemies((uint)OID.TwoHeadedDragon);
                var ninjas = Module.Enemies((uint)OID.SacrificedNinja);
                var kunoichi = Module.Enemies((uint)OID.SacrificedKunoichi);
                var addsSpawned = dragon.Count + ninjas.Count + kunoichi.Count > 0;
                return bossesDead && addsSpawned && dragon.All(a => a.IsDeadOrDestroyed) && ninjas.All(a => a.IsDeadOrDestroyed) && kunoichi.All(a => a.IsDeadOrDestroyed);
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3243)]
public class A31Garm(WorldState ws, Actor primary) : BossModule(ws, primary, new(-77, 383), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss), Colors.Enemy);
    }
}
