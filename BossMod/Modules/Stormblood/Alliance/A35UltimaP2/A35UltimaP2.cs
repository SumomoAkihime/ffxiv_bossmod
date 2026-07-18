namespace BossMod.Stormblood.Alliance.A35UltimaP2;

class Redemption(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Redemption);
class Auralight1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Auralight1, new AOEShapeRect(50, 5));
class Auralight2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Auralight2, new AOEShapeRect(25, 5));
class Bombardment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Bombardment, 6);
class GrandCrossAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandCrossAOE, new AOEShapeCross(60, 7.5f));
class Holy(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Holy, 2);
class HolyIVBait(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HolyIVBait, 6);
class HolyIVSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HolyIVSpread, 6);
class Plummet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Plummet, new AOEShapeRect(15, 7.5f));

class RayOfLight(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _shape = new(70f, 5f);
    private readonly List<(Actor Source, DateTime Activation)> _sources = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void Update()
    {
        _aoes.Clear();
        foreach (var source in _sources)
            _aoes.Add(new(_shape, source.Source.Position, source.Source.Rotation, source.Activation));
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Dominion1 && MathF.Abs(actor.Position.Z - Module.Center.Z) >= 29f)
            _sources.Add((actor, WorldState.FutureTime(10.9d)));
    }

    public override void OnActorDestroyed(Actor actor) => _sources.RemoveAll(source => source.Source.InstanceID == actor.InstanceID);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RayOfLight1)
            _sources.RemoveAll(source => source.Source.InstanceID == caster.InstanceID);
    }
}

class Penultima(BossModule module) : BossComponent(module)
{
    private const float Radius = 3f;
    private readonly List<Actor> _towers = [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Dominion2)
            _towers.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor) => _towers.RemoveAll(tower => tower.InstanceID == actor.InstanceID);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Penultima or (uint)AID.UltimateFlare)
            _towers.RemoveAll(tower => tower.InstanceID == caster.InstanceID);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var tower in _towers)
        {
            var occupied = Module.Raid.WithoutSlot().InRadius(tower.Position, Radius).Any();
            Arena.AddCircle(tower.Position, Radius, occupied ? Colors.Safe : Colors.Danger, 2f);
        }
    }
}

class GrandCrossSafe(BossModule module) : BossComponent(module)
{
    private static readonly float[] _grid = [577.5f, 592.5f, 607.5f, 622.5f];
    private readonly List<WPos> _shards = [];
    private WPos? _safe;
    private float _shift;
    private int _numCasts;
    private bool _collecting;

    public override void OnActorCreated(Actor actor)
    {
        if (_collecting && actor.OID == (uint)OID.AuraciteShard && _shards.Count < 3)
        {
            _shards.Add(actor.Position);
            UpdateSafe();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GrandCross2:
                _shards.Clear();
                _safe = null;
                _shift = 0f;
                _numCasts = 0;
                _collecting = true;
                break;
            case AID.EastwardMarch1:
                _shift = 15f;
                UpdateSafe();
                break;
            case AID.WestwardMarch2:
                _shift = -15f;
                UpdateSafe();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GrandCrossAOE && ++_numCasts == 3)
        {
            _safe = null;
            _collecting = false;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_safe is WPos safe)
            Arena.AddRect(safe, new WDir(0f, 1f), 7.5f, 7.5f, 7.5f, Colors.Safe, 2f);
    }

    private void UpdateSafe()
    {
        if (_shards.Count != 3 || _shift == 0f)
            return;

        var predicted = new WPos[3];
        for (var i = 0; i < 3; ++i)
            predicted[i] = i < 2 ? _shards[i] + new WDir(_shift, 0f) : _shards[i];

        var safeX = _grid.First(x => !predicted.Any(p => MathF.Abs(p.X - x) < 1f));
        var safeZ = _grid.First(z => !predicted.Any(p => MathF.Abs(p.Z - z) < 1f));
        _safe = new(safeX, safeZ);
    }
}

class Embrace(BossModule module) : Components.GenericAOEs(module, (uint)AID.Embrace3)
{
    private static readonly AOEShapeCircle _shape = new(7f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Embrace2)
            _aoes.Add(new(_shape, spell.LocXZ, activation: Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == WatchedAction)
        {
            var index = _aoes.FindIndex(aoe => aoe.Origin.AlmostEqual(caster.Position, 1f));
            if (index >= 0)
                _aoes.RemoveAt(index);
        }
    }
}

class LifeDrain(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _dominions = module.Enemies((uint)OID.Dominion1);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var source in _dominions.Tethered(TetherID.Tether84))
        {
            var target = WorldState.Actors.Find(source.Tether.Target);
            if (target != null)
            {
                Arena.AddLine(source.Position, target.Position, Colors.Danger);
                Arena.AddCircle(target.Position, 1f, Colors.Danger);
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _dominions.Any(source => source.Tether.ID == (uint)TetherID.Tether84 && source.Tether.Target == player.InstanceID) ? PlayerPriority.Danger : PlayerPriority.Normal;
}

class Cataclysm(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if ((SID)status.ID == SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if ((SID)status.ID == SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7909)]
public class A35UltimaP2(WorldState ws, Actor primary) : BossModule(ws, primary, new(600, -600), new ArenaBoundsSquare(30));
