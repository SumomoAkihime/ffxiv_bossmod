namespace BossMod.Stormblood.Alliance.A34UltimaP1;

class HolyIVBait(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HolyIVBait, 6f);
class HolyIVSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HolyIVSpread, 6f);
class AuralightAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AuralightAOE, 20f);
class AuralightRect(BossModule module) : Components.GenericAOEs(module)
{
    private sealed class Wall(ulong casterID, AOEInstance aoe)
    {
        public readonly ulong CasterID = casterID;
        public readonly AOEInstance AOE = aoe;
        public DateTime ExpireAt = DateTime.MaxValue;
    }

    private static readonly AOEShapeRect _shape = new(70f, 5f);
    private readonly List<Wall> _walls = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void Update()
    {
        _walls.RemoveAll(wall => wall.ExpireAt <= WorldState.CurrentTime);
        _aoes.Clear();
        foreach (var wall in _walls)
            _aoes.Add(wall.AOE);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AuralightRect)
        {
            _walls.RemoveAll(wall => wall.CasterID == caster.InstanceID);
            _walls.Add(new(caster.InstanceID, new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell))));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AuralightRect)
        {
            var wall = _walls.Find(wall => wall.CasterID == caster.InstanceID);
            if (wall != null)
                wall.ExpireAt = WorldState.FutureTime(6d);
        }
    }
}
class GrandCrossAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandCrossAOE, new AOEShapeCross(60f, 7.5f));
class Plummet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Plummet, new AOEShapeRect(15f, 7.5f));
class TimeEruption(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.TimeEruptionAOEFirst, (uint)AID.TimeEruptionAOESecond], new AOEShapeRect(20f, 10f), expectedNumCasters: 9);

class Eruption2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Eruption2, 8f);
class ControlTower2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ControlTower2, 6f);

class Towerfall(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _shape = new(40f, 5f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TowerfallDirection)
            _aoes.Add(new(_shape, actor.Position, actor.Rotation, WorldState.FutureTime(9.4d)));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.TowerfallDirection)
            _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(actor.Position, 1f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Towerfall)
            _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1f));
    }
}

abstract class ExtremeEdge(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(60f, 18f));
class ExtremeEdge1(BossModule module) : ExtremeEdge(module, (uint)AID.ExtremeEdge1);
class ExtremeEdge2(BossModule module) : ExtremeEdge(module, (uint)AID.ExtremeEdge2);

class CrushWeapon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrushWeapon, 6f);
class Searchlight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Searchlight, 6f);
class HallowedBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HallowedBolt, 6f);

class PrevailingCurrent(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(6f);
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<Actor> _aspersories = module.Enemies((uint)OID.Aspersory);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void Update()
    {
        _aoes.Clear();
        foreach (var aspersory in _aspersories)
        {
            if (!aspersory.IsDead)
                _aoes.Add(new(_shape, aspersory.Position));
        }
    }
}

class UltimateIllusionArena(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _mustadio = module.Enemies((uint)OID.Mustadio);
    private readonly List<Actor> _agrias = module.Enemies((uint)OID.Agrias1);
    private readonly List<Actor> _thunderGod = module.Enemies((uint)OID.TheThunderGod);
    private bool _active;

    public override void Update()
    {
        if (_mustadio.Count == 0 || _agrias.Count == 0 || _thunderGod.Count == 0)
            return;

        var center = new WPos(
            (_mustadio[0].Position.X + _agrias[0].Position.X + _thunderGod[0].Position.X) / 3f,
            (_mustadio[0].Position.Z + _agrias[0].Position.Z + _thunderGod[0].Position.Z) / 3f);
        if ((center - A34UltimaP1.ArenaCenter).LengthSq() < 225f)
            return;

        Arena.Center = center.Rounded();
        if (!_active)
        {
            Arena.Bounds = A34UltimaP1.UltimateIllusionBounds;
            _active = true;
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7909)]
public class A34UltimaP1(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly WPos ArenaCenter = new(600f, -600f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(30f);
    public static readonly ArenaBoundsCircle UltimateIllusionBounds = new(8f);
}
