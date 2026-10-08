namespace BossMod.RealmReborn.Alliance.A32FiveheadedDragon;

class WhiteBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WhiteBreath, new AOEShapeCone(30, 60.Degrees()));
class BreathOfFire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BreathOfFire, 6);
class BreathOfLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BreathOfLight, 6);
class BreathOfPoison(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BreathOfPoison, 6);
class IceFloor(BossModule module) : Components.VoidzoneAtCastTarget(module, 12, (uint)AID.BreathOfIce, m => m.Enemies((uint)OID.IceFloor).Where(a => a.EventState != 7), 7.4f);
class Stack(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, (uint)AID.BreathOfThunder, 6, 6, 3);
class Discordance(BossModule module) : Components.RaidwideCast(module, (uint)AID.Discordance, "Kill the heads!");
class Radiance(BossModule module) : Components.RaidwideCast(module, (uint)AID.Radiance, "Kill Prominence!");
class HeatWave(BossModule module) : Components.RaidwideCast(module, (uint)AID.HeatWave, "Pyretic incoming, stop moving");
class HeatWavePyretic(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Pyretic)
        {
            var state = new PlayerState(Requirement.Stay, status.ExpireAt);
            SetState(Raid.FindSlot(actor.InstanceID), in state);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Pyretic)
            ClearState(Raid.FindSlot(actor.InstanceID));
    }
}
class Heads(BossModule module) : Components.AddsMulti(module, [(uint)OID.HeadOfPoison, (uint)OID.HeadOfFire, (uint)OID.HeadOfThunder, (uint)OID.HeadOfIce], 1);
class Prominence(BossModule module) : Components.Adds(module, (uint)OID.Prominence, 2);
class Slimes(BossModule module) : Components.AddsMulti(module, [(uint)OID.PoisonSlime, (uint)OID.ToxicSlime], 1);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3227)]
public class A32FiveheadedDragon(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, 179), new ArenaBoundsCircle(30));
