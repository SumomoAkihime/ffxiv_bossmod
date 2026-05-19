namespace BossMod.Dawntrail.Alliance.A11Prishe;

class NullifyingDropkick(BossModule module) : Components.CastSharedTankbuster(module, AID.NullifyingDropkickAOE, 6);
class Holy(BossModule module) : Components.SpreadFromCastTargets(module, AID.HolyAOE, 6);

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13351)]
public class A11Prishe(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly WPos ArenaCenter = new(800, 400);
    public static readonly ArenaBoundsSquare DefaultBounds = new(35);
}
