namespace BossMod.Shadowbringers.Raid.E09CloudOfDarkness;

public enum OID : uint
{
    Boss = 0x3056, // R7.500, Cloud of Darkness
    Helper = 0x233C, // R0.500
    CloneOfDarkness = 0x3057, // R7.500
    HyperchargedCloud = 0x3058, // R1.000
    StygianTendrils = 0x3059, // R1.000, evil-seed death zones
    StygianArbor = 0x305A, // R0.500, wall trees for Balm / Bad Vibrations
}

public enum AID : uint
{
    AutoAttack = 22054, // Boss->player, no cast, single-target
    GroundRazingParticleBeam = 21997, // Boss->self, 5.0s cast, single-target
    GroundRazingParticleBeamAOE = 23320, // Helper->self, no cast, range 60 circle
    TheArtOfDarknessL = 21027, // Boss->self, 5.0s cast, range 60 180-degree cone
    TheArtOfDarknessR = 21028, // Boss->self, 5.0s cast, range 60 180-degree cone
    WideAngleParticleBeam = 23295, // Boss->self, 5.0s cast, range 45 130-degree cone
    ZeroFormParticleBeam = 21995, // Boss->self/player, 4.2s cast, range 60 width 8 rect
    EmptyPlane = 20422, // Boss->self, 5.0s cast, range 60 circle (phase)
    FloodOfEmptiness = 20786, // Boss->self, 5.0s cast, single-target
    MireOfDespair = 23303, // Helper->self, 5.4s cast, range 8 width 8 rect (center death zone)
    EarthShatteringParticleBeam = 21029, // Helper->location, 5.4s cast, range 8 width 8 rect (corners)
    ObscureWoods = 20386, // Boss->self, 5.0s cast, range 60 circle (phase)
    FloodOfObscurity = 22769, // Boss->self, 5.0s cast, single-target
    WasteAway = 21982, // Helper->self, 5.2s cast, range 50 circle (knockback)
    EvilSeed = 21991, // Helper->location, 7.5s cast, range 4 circle
    Withdraw = 21988, // Tendrils->player, no cast, single-target (tether pull)
    Aetherosynthesis = 21989, // Tendrils->player, no cast, single-target
    WideAnglePhaser = 21983, // Boss->self, 6.0s cast, single-target
    WideAnglePhaserAlt = 21984, // Boss->self, 6.0s cast, single-target (alternate)
    WideAnglePhaserAOE = 21985, // Helper->self, 7.6s cast, range 45 130-degree cone
    RejuvenatingBalm = 21986, // Boss->self, 4.0s cast, single-target
    RejuvenatingBalmAOE = 21987, // Helper->Arbor, 1.5s cast, single-target (wall explosions)
    BadVibrations = 21990, // Helper->self, 8.0s cast, range 22 180-degree cone
    DelugeOfDarkness = 20821, // Boss->self, 5.0s cast, single-target (phase)
    DelugeOfDarknessAOE = 20825, // Helper->self, no cast, range 60 circle
    ParticleConcentration = 21992, // Boss->self, 3.0s cast, single-target (towers)
    ParticleBlast = 21994, // Helper->self, no cast, range 50 circle (empty tower)
    ParticleBeam = 21993, // Helper->self, no cast, range 4 circle (tower soak)
    HyperchargedCondensation = 21294, // Boss->self, 3.0s cast, single-target
    HyperchargedDispersal = 21295, // HyperchargedCloud->Boss, no cast, single-target
    Summon = 21296, // Boss->self, 3.0s cast, single-target (spawns clones)
    AntiAirParticleBeam = 21980, // Boss->self, 5.0s cast, range 13 circle
    CloneZeroFormParticleBeam = 21979, // CloneOfDarkness->self, 5.0s cast, range 60 width 8 rect
}

public enum TetherID : uint
{
    Hypercharged = 1, // Arbor/HyperchargedCloud->Boss
    SeedNear = 17, // Tendrils->player (stand next to zone)
    SeedFar = 18, // Tendrils->player (run away)
    SeedSuck = 43, // Tendrils->player (fail)
}

class GroundRazingParticleBeam(BossModule module) : Components.RaidwideCast(module, (uint)AID.GroundRazingParticleBeam);
class EmptyPlane(BossModule module) : Components.RaidwideCast(module, (uint)AID.EmptyPlane);
class ObscureWoods(BossModule module) : Components.RaidwideCast(module, (uint)AID.ObscureWoods);
class DelugeOfDarkness(BossModule module) : Components.RaidwideCast(module, (uint)AID.DelugeOfDarkness);
class TheArtOfDarkness(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TheArtOfDarknessL, (uint)AID.TheArtOfDarknessR], new AOEShapeCone(60, 90.Degrees()));
class WideAngleParticleBeam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WideAngleParticleBeam, new AOEShapeCone(45, 65.Degrees()));
class WideAnglePhaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WideAnglePhaserAOE, new AOEShapeCone(45, 65.Degrees()));
class ZeroFormParticleBeam(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ZeroFormParticleBeam, new AOEShapeRect(60, 4));
class AntiAirParticleBeam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AntiAirParticleBeam, 13);
class CloneZeroFormParticleBeam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CloneZeroFormParticleBeam, new AOEShapeRect(60, 4));
class EvilSeed(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EvilSeed, 4);
class BadVibrations(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BadVibrations, new AOEShapeCone(22, 90.Degrees()));
class WasteAway(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.WasteAway, 12)
{
    private sealed class UnsafeLanding(Func<WPos, bool> unsafePosition) : ShapeDistance
    {
        public override float Distance(in WPos p) => unsafePosition(p) ? float.MinValue : float.MaxValue;
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (!Arena.InBounds(pos))
            return true;
        foreach (var c in Module.Components.OfType<Components.GenericAOEs>())
            if (c.ActiveAOEs(slot, actor).ToArray().Any(a => a.Risky && a.Check(pos)))
                return true;
        return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in ActiveKnockbacks(slot, actor))
        {
            if (IsImmune(slot, s.Activation))
                continue;
            var origin = s.Origin;
            var dist = s.Distance;
            hints.AddForbiddenZone(new UnsafeLanding(p => DestinationUnsafe(slot, actor, AwayFromSource(p, origin, dist))), s.Activation);
        }
    }
}

class HyperchargedClouds(BossModule module) : Components.Adds(module, (uint)OID.HyperchargedCloud, 2)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Module.FindComponent<FloodOfEmptiness>() is { } flood && flood.ActiveAOEs(slot, actor).ToArray().Any())
            hints.AddForbiddenZone(FloodOfEmptiness.Tile, FloodOfEmptiness.CenterTile);
    }
}

class RejuvenatingBalm(BossModule module) : Components.GenericAOEs(module, (uint)AID.RejuvenatingBalmAOE)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle _shape = new(40);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.ToArray();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != WatchedAction)
            return;
        // can't use caster.Position cause they start in the center before teleporting to position
        var origin = WorldState.Actors.Find(spell.TargetID)?.Position ?? spell.LocXZ;
        _aoes.Add(new(_shape, origin, default, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != WatchedAction)
            return;
        var origin = WorldState.Actors.Find(spell.TargetID)?.Position ?? spell.LocXZ;
        _aoes.RemoveAll(a => a.Origin.AlmostEqual(origin, 1));
    }
}

class FloodOfEmptiness(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShapeRect Tile = new(4, 4, 4);
    public static readonly WPos CenterTile = new(100, 100);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.ToArray();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MireOfDespair or AID.EarthShatteringParticleBeam)
            _aoes.Add(new(Tile, TileCenter(spell.LocXZ), default, Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.MireOfDespair:
            case AID.EarthShatteringParticleBeam:
                var origin = TileCenter(spell.TargetXZ.AlmostEqual(default, 1) ? caster.Position : spell.TargetXZ);
                for (var i = 0; i < _aoes.Count; ++i)
                    if (_aoes[i].Origin.AlmostEqual(origin, 1))
                        _aoes[i] = _aoes[i] with { Activation = default };
                break;
            case AID.ObscureWoods:
            case AID.DelugeOfDarkness:
            case AID.DelugeOfDarknessAOE:
                _aoes.Clear();
                break;
        }
    }

    // snap LocXZ to nearest tile center (5x5 grid of 4r tiles)
    public static WPos TileCenter(WPos locXZ)
    {
        var gx = Math.Clamp((int)MathF.Round((locXZ.X - 84) / 8), 0, 4);
        var gz = Math.Clamp((int)MathF.Round((locXZ.Z + 4 - 84) / 8), 0, 4);
        return new(84 + 8 * gx, 84 + 8 * gz);
    }
}

class SeedVoidzones(BossModule module) : Components.Voidzone(module, 4, m => m.Enemies((uint)OID.StygianTendrils).Where(a => a.EventState != 7));

class SeedTethers(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor Source, Actor Target, uint ID)> _tethers = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID != (uint)OID.StygianTendrils)
            return;
        _tethers.RemoveAll(t => t.Source == source);
        if (tether.ID == 0 || tether.Target == 0)
            return;
        if ((TetherID)tether.ID is TetherID.SeedNear or TetherID.SeedFar or TetherID.SeedSuck)
            if (WorldState.Actors.Find(tether.Target) is { } target)
                _tethers.Add((source, target, tether.ID));
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.StygianTendrils)
            _tethers.RemoveAll(t => t.Source == source);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Withdraw or AID.Aetherosynthesis or AID.DelugeOfDarkness)
            _tethers.Clear();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var (src, tgt, id) in _tethers)
        {
            if (tgt != actor)
                continue;
            hints.Add((TetherID)id switch
            {
                TetherID.SeedNear => "Get next to seeds!",
                TetherID.SeedFar => "Run away from seeds!",
                _ => "Break seed tether!"
            });
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (src, tgt, id) in _tethers)
        {
            var color = (TetherID)id switch
            {
                TetherID.SeedNear => Colors.Safe,
                TetherID.SeedFar => Colors.Danger,
                _ => Colors.Danger
            };
            Arena.AddLine(src.Position, tgt.Position, color);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (src, tgt, id) in _tethers)
        {
            if (tgt != actor)
                continue;
            var offset = actor.Position - src.Position;
            var dist = offset.Length();
            var dir = dist > 0.1f ? offset / dist : new WDir(1, 0);
            switch ((TetherID)id)
            {
                case TetherID.SeedNear:
                    // sit just outside the voidzone
                    hints.AddForbiddenZone(new SDCircle(src.Position, 4));
                    var ideal = src.Position + dir * 5;
                    var toIdeal = ideal - actor.Position;
                    if (toIdeal.LengthSq() > 0.25f)
                        hints.ForcedMovement = toIdeal.ToVec3(actor.PosRot.Y);
                    break;
                case TetherID.SeedFar:
                case TetherID.SeedSuck:
                    // breaks at ~12y
                    hints.AddForbiddenZone(new SDCircle(src.Position, 15));
                    if (dist < 15)
                        hints.ForcedMovement = dir.ToVec3(actor.PosRot.Y);
                    break;
            }
        }
    }
}

class UnstableTiles : Components.GenericAOEs
{
    private enum TileState { None, Yellow, Red, Destroyed }

    private readonly TileState[] _states = new TileState[21];
    private readonly WPos?[] _centers = new WPos?[21];
    private readonly DateTime[] _activation = new DateTime[21];
    private bool _active;

    private static readonly AOEShapeRect _tile = new(4, 4, 4);

    public UnstableTiles(BossModule module) : base(module)
    {
        _centers[2] = new(92, 92);
        _centers[3] = new(100, 92);
        _centers[4] = new(108, 92);
        _centers[14] = new(116, 92);
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var result = new List<AOEInstance>();
        for (var i = 0; i < _states.Length; ++i)
        {
            if (_states[i] is not (TileState.Red or TileState.Destroyed) || _centers[i] is not { } c)
                continue;
            result.Add(new(_tile, c, default, _activation[i]));
        }
        return result.ToArray();
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index > 20)
            return;

        switch (state)
        {
            case 0x08000001: // become activatable
                _active = true;
                _states[index] = TileState.None;
                break;
            case 0x00200010: // first outline, as soon as someone stands on them
                FindCenter(index);
                _states[index] = TileState.Yellow;
                _activation[index] = WorldState.FutureTime(5.7f);
                break;
            case 0x00800040: // red outline, about to collapse
                FindCenter(index);
                _states[index] = TileState.Red;
                _activation[index] = WorldState.FutureTime(6.1f);
                break;
            case 0x02000001: // when the outline disappears
                _states[index] = TileState.None;
                _activation[index] = default;
                break;
            case 0x00080004: // collapsed
                FindCenter(index);
                _states[index] = TileState.Destroyed;
                _activation[index] = default;
                break;
            case 0x00020001: // restored after collapse
                _states[index] = TileState.None;
                _activation[index] = default;
                break;
            case 0x20000004: // cleanup
            case 0x40001000:
            case 0x00100004:
                Reset();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ObscureWoods or AID.DelugeOfDarkness or AID.DelugeOfDarknessAOE)
            Reset();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_active)
            return;
        var cell = CellCenter(actor.Position);
        for (var i = 0; i < _states.Length; ++i)
        {
            if (_centers[i] is not { } c || !c.AlmostEqual(cell, 1))
                continue;

            // other states don't really matter
            if (_states[i] is TileState.Red)
            {
                hints.Add("GTFO from tile! About to collapse!");
                return;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_active)
            return;

        for (var i = 0; i < _states.Length; ++i)
        {
            if (_centers[i] is not { } c)
                continue;
            if (_states[i] is TileState.Red or TileState.Destroyed) // only move on red
                hints.AddForbiddenZone(_tile, c);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_active)
            return;
        for (var i = 0; i < _states.Length; ++i)
        {
            if (_states[i] != TileState.Yellow || _centers[i] is not { } c)
                continue;
            Arena.AddRect(c, new WDir(0, 1), 4, 4, 4, Colors.Danger);
        }
    }

    private void Reset()
    {
        _active = false;
        Array.Clear(_states);
        Array.Clear(_activation);
    }

    private void FindCenter(int index)
    {
        if (_centers[index] != null)
            return;

        var claimed = new List<WPos>();
        for (var i = 0; i < _states.Length; ++i)
        {
            if (i != index && _states[i] != TileState.None && _centers[i] is { } c)
                claimed.Add(c);
        }

        Actor? best = null;
        foreach (var actor in Raid.WithoutSlot(true))
        {
            var cell = CellCenter(actor.Position);
            if (claimed.Any(c => c.AlmostEqual(cell, 1)))
                continue;
            if (best == null || actor == Raid.Player())
                best = actor;
        }

        if (best != null)
            _centers[index] = CellCenter(best.Position);
        else if (Raid.Player() is { } pc)
            _centers[index] = CellCenter(pc.Position);
    }

    private static WPos CellCenter(WPos p)
    {
        var gx = Math.Clamp((int)((p.X - 80) / 8), 0, 4);
        var gz = Math.Clamp((int)((p.Z - 80) / 8), 0, 4);
        return new(84 + 8 * gx, 84 + 8 * gz);
    }
}

class ParticleConcentration(BossModule module) : Components.GenericTowers(module, (uint)AID.ParticleBeam)
{
    private static readonly WDir[] _offsets = [new(-8, -8), new(8, -8), new(-8, 8), new(8, 8)];

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ParticleConcentration)
        {
            var act = WorldState.FutureTime(10.3f);
            foreach (var o in _offsets)
                Towers.Add(new(Module.Center + o, 4, activation: act));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ParticleBeam or AID.ParticleBlast)
        {
            NumCasts++;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 2));
        }
    }
}

class E09CloudOfDarknessStates : StateMachineBuilder
{
    public E09CloudOfDarknessStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GroundRazingParticleBeam>()
            .ActivateOnEnter<TheArtOfDarkness>()
            .ActivateOnEnter<WideAngleParticleBeam>()
            .ActivateOnEnter<ZeroFormParticleBeam>()
            .ActivateOnEnter<EmptyPlane>()
            .ActivateOnEnter<FloodOfEmptiness>()
            .ActivateOnEnter<UnstableTiles>()
            .ActivateOnEnter<ObscureWoods>()
            .ActivateOnEnter<WasteAway>()
            .ActivateOnEnter<EvilSeed>()
            .ActivateOnEnter<SeedVoidzones>()
            .ActivateOnEnter<SeedTethers>()
            .ActivateOnEnter<WideAnglePhaser>()
            .ActivateOnEnter<RejuvenatingBalm>()
            .ActivateOnEnter<BadVibrations>()
            .ActivateOnEnter<DelugeOfDarkness>()
            .ActivateOnEnter<ParticleConcentration>()
            .ActivateOnEnter<HyperchargedClouds>()
            .ActivateOnEnter<AntiAirParticleBeam>()
            .ActivateOnEnter<CloneZeroFormParticleBeam>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 749, NameID = 9764)]
public class E09CloudOfDarkness(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, Colors.Enemy);
        Arena.Actors(Enemies((uint)OID.HyperchargedCloud), Colors.Enemy);
        Arena.Actors(Enemies((uint)OID.CloneOfDarkness), Colors.Object);
        Arena.Actors(Enemies((uint)OID.StygianArbor), Colors.Object);
    }
}
