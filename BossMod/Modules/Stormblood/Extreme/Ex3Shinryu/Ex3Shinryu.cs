namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

class EarthenFury(BossModule module) : Components.RaidwideCast(module, (uint)AID.EarthenFury);
class Hellfire(BossModule module) : Components.RaidwideCast(module, (uint)AID.Hellfire, "Raidwide, stand in water");
class JudgmentBolt(BossModule module) : Components.RaidwideCast(module, (uint)AID.JudgmentBolt, "Raidwide, get out of water");
class DiamondDust(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiamondDust, "Raidwide, thin ice after");
class IceStorm(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.IceStormVisual, (uint)AID.IceStorm, 0.6f);
class IceStormP3(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.IceStormVisualP3, (uint)AID.IceStorm, 0.6f);
class DarkMatter(BossModule module) : Components.RaidwideCast(module, (uint)AID.DarkMatter);
class Protostar(BossModule module) : Components.RaidwideCast(module, (uint)AID.Protostar);
class TailSpit(BossModule module) : Components.RaidwideCast(module, (uint)AID.TailSpit);
class TailSpitCenter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailSpitCenter, 5);
class SuperCyclone(BossModule module) : Components.RaidwideMultiHit(module, (uint)AID.AerialBlast, [(uint)AID.SuperCyclone], -6.1f, 2.1f, 3);
class Hypernova(BossModule module) : Components.CastHint(module, (uint)AID.HypernovaVisual, "Stack together");
class IcicleAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.Icicle]);
class AkhMorn(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.AkhMorn, 4);
class EarthBreath(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(80, 30.Degrees()), (uint)IconID.EarthBreath, (uint)AID.EarthBreathAOE, 5.6f);
class Dragonfist(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Dragonfist, 16);
class GyreCharge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GyreCharge, new AOEShapeRect(100, 30));
class Cocoons(BossModule module) : Components.AddsMulti(module, [(uint)OID.Cocoon, (uint)OID.MassiveCocoon]);
class DragonAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.Ginryu, (uint)OID.Hakkinryu], 1);
class MeteorImpactMarker(BossModule module) : Components.ProximityAOEs(module, (uint)AID.MeteorImpactMarker, 10);
class Touchdown(BossModule module) : Components.ProximityAOEs(module, (uint)AID.Touchdown, 14);
class Fireball(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Fireball, 4);
class BlazingTrail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlazingTrail, new AOEShapeRect(15, 5.5f));
class SpikedTail(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SpikedTail);
class DeathSentence(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DeathSentence);
class TeraSlash(BossModule module) : Components.SingleTargetCast(module, (uint)AID.TeraSlash);
class AtomicRay(BossModule module) : Components.StackWithIcon(module, (uint)IconID.AtomicRay, (uint)AID.AtomicRay, 6, 5.7f, 2, 2);
class AetherialRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherialRay, new AOEShapeRect(100, 1.5f));
class Wormwail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Wormwail, new AOEShapeDonut(10, 50));
class BenightingBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BenightingBreath, new AOEShapeCone(50, 45.Degrees()));
class Enrage(BossModule module) : Components.CastHint(module, (uint)AID.TidalWaveEnrage, "Enrage", true);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Nonu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 278, NameID = 5640)]
public class Ex3Shinryu(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsSquare(30, mapResolution: 0.3f))
{
    public static readonly WPos P2Center = new(0, -80);
    public static readonly ArenaBoundsSquare P2Bounds = new(20);
    public static readonly WPos ClimbCenter = new(0, -50);
    public static readonly ArenaBounds ClimbBounds = BuildClimbBounds();

    private static ShinryuPolygonBounds BuildClimbBounds()
    {
        WPos[] tail = [new(-15, -97), new(-4, -86), new(2, -76), new(6, -66), new(10, -56), new(14, -46), new(17, -36), new(17, -26)];
        var clipper = new PolygonClipper();
        var shape = new PolygonClipper.Operand(ShinryuPolygonBounds.Rectangle(P2Center - ClimbCenter, new WDir(0, 1), 20, 20));
        shape.AddContour(ShinryuPolygonBounds.Rectangle(new WPos(0, 0) - ClimbCenter, new WDir(0, 1), 30, 30));
        for (int i = 1; i < tail.Length; ++i)
        {
            var dir = (tail[i] - tail[i - 1]).Normalized();
            shape.AddContour(ShinryuPolygonBounds.Rectangle((tail[i] - ClimbCenter + (tail[i - 1] - ClimbCenter)) * 0.5f, dir, 5, (tail[i] - tail[i - 1]).Length() * 0.5f + 2));
        }
        return new(85, clipper.Simplify(shape, Clipper2Lib.FillRule.NonZero));
    }

    public static WPos TileCenter(WPos p) => new(MathF.Round(p.X / 20) * 20, MathF.Round(p.Z / 20) * 20);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, Colors.Enemy, true);
        Arena.Actors(Enemies(OID.Tail), Colors.Enemy, true);
    }
}
