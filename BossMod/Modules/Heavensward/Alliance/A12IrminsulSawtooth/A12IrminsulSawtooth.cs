namespace BossMod.Heavensward.Alliance.A12IrminsulSawtooth;

class WhiteBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WhiteBreath, new AOEShapeCone(28f, 60f.Degrees()));
class MeanThrash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeanThrash, new AOEShapeCone(12f, 60f.Degrees()));
class MeanThrashKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.MeanThrash, 10f, stopAtWall: true);
class MucusBomb(BossModule module) : Components.SpreadFromCastTargets(module, AID.MucusBomb, 10f);
class MucusSpray(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MucusSpray2, new AOEShapeDonut(6f, 20f));
class Rootstorm(BossModule module) : Components.RaidwideCast(module, AID.Rootstorm);
class Ambush(BossModule module) : Components.StandardAOEs(module, AID.Ambush, 9f);

class ShockwaveStomp(BossModule module) : Components.CastLineOfSightAOE(module, AID.ShockwaveStomp, 70f, false)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies((uint)OID.Irminsul).Where(a => !a.IsDead);
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, StatesType = typeof(A12IrminsulSawtoothStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), PrimaryActorOID = (uint)OID.Irminsul, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 120, NameID = 4623)]
public class A12IrminsulSawtooth(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 130), new ArenaBoundsCircle(30));
