namespace BossMod.Heavensward.Alliance.A24Ozma;

class MeteorImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeteorImpact, 20);
class HolyKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Holy, 3);
class Holy(BossModule module) : Components.RaidwideCast(module, AID.Holy);
class ExecrationAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExecrationAOE, new AOEShapeRect(40, 5));

class AccelerationBomb(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, StatesType = typeof(A24OzmaStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4896)]
public sealed class A24Ozma(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.MainCenter, ArenaChanges.MainBounds)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.SingularityFragment), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.SingularityEcho), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.SingularityRipple), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.Ozmasphere), ArenaColor.Object);
        Arena.Actors(Enemies((uint)OID.Ozmashade), ArenaColor.Object);
    }
}
