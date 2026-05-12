namespace BossMod.Dawntrail.Trial.T04Zelenia;

class PowerBreak(BossModule module) : Components.GroupedAOEs(module, [AID.PowerBreak1, AID.PowerBreak2], new AOEShapeRect(24, 32));
class HolyHazard(BossModule module) : Components.StandardAOEs(module, AID.HolyHazard, new AOEShapeCone(24, 60.Degrees()), 2);

class RosebloodBloom(BossModule module) : Components.KnockbackFromCastTarget(module, AID.RosebloodBloom, 10, stopAtWall: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
        {
            var c = Casters[0];
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(c.Position, 6), Module.CastFinishAt(c.CastInfo));
        }
    }
}

class ThunderSlash(BossModule module) : Components.StandardAOEs(module, AID.ThunderSlash, new AOEShapeCone(24, 30.Degrees()), 4);
class PerfumedQuietus(BossModule module) : Components.RaidwideCast(module, AID.RosebloodBloom);
class ThornedCatharsis(BossModule module) : Components.RaidwideCast(module, AID.ThornedCatharsis);
class SpecterOfTheLost(BossModule module) : Components.BaitAwayCast(module, AID.SpecterOfTheLost, new AOEShapeCone(50, 22.5f.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1030, NameID = 13861)]
public class T04Zelenia(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultArena)
{
    private static readonly WPos ArenaCenter = new(100, 100);
    public static readonly ArenaBoundsCircle DefaultArena = new(16);
    public static readonly ArenaBoundsCircle DonutArena = new(16);
}
