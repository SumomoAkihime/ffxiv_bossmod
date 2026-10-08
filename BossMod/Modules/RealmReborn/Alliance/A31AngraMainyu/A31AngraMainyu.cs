namespace BossMod.RealmReborn.Alliance.A31AngraMainyu;

class Stare(BossModule module) : Components.Cleave(module, (uint)AID.Stare, new AOEShapeRect(63.2f, 4), activeWhileCasting: false);

class DoubleVision(BossModule module) : Components.GenericAOEs(module, (uint)AID.DoubleVision)
{
    private DateTime _activation;
    private Angle _rotation;
    private WPos _origin;

    private static readonly AOEShapeCone _half = new(40, 90.Degrees());

    private readonly AOEInstance[] _front = new AOEInstance[1];
    private readonly AOEInstance[] _back = new AOEInstance[1];
    private readonly AOEInstance[] _both = new AOEInstance[2];

    public override void Update()
    {
        _front[0] = new(_half, _origin, _rotation, _activation);
        _back[0] = new(_half, _origin, _rotation + 180.Degrees(), _activation);
        _both[0] = _front[0];
        _both[1] = _back[0];
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
            return [];
        var front = actor.FindStatus((uint)SID.BrandOfTheSullen) != null;
        var back = actor.FindStatus((uint)SID.BrandOfTheIreful) != null;
        return front && back ? _both : front ? _front : back ? _back : [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DoubleVision)
        {
            _origin = caster.Position;
            _rotation = spell.Rotation;
            _activation = Module.CastFinishAt(spell);
            Update();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SullenGaze or (uint)AID.IrefulGaze)
            _activation = default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation == default)
            return;
        if (actor.FindStatus((uint)SID.BrandOfTheSullen) != null)
            hints.Add("Stand on red (Ireful) side!");
        else if (actor.FindStatus((uint)SID.BrandOfTheIreful) != null)
            hints.Add("Stand on white (Sullen) side!");
    }
}

class MortalGaze(BossModule module) : Components.CastGaze(module, (uint)AID.MortalGaze);
class MortalGazeHelper(BossModule module) : Components.CastGaze(module, (uint)AID.MortalGazeHelper);

class Level100Flare(BossModule module) : PlayerCountCircle(module, (uint)IconID.Level100Flare, (uint)AID.Level100FlareResolve, requireMultipleOf: 2);
class Level150Death(BossModule module) : PlayerCountCircle(module, (uint)IconID.Level150Death, (uint)AID.Level150DeathResolve, requireMultipleOf: 3);

class PlayerCountCircle(BossModule module, uint iconID, uint resolve, int requireMultipleOf) : BossComponent(module)
{
    private readonly uint _iconID = iconID;
    private readonly uint _resolve = resolve;
    private readonly int _requireMultipleOf = requireMultipleOf;
    private Actor? _target;
    private DateTime _activation;

    private const float Radius = 12;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == _iconID)
        {
            _target = WorldState.Actors.Find(targetID) ?? actor;
            _activation = WorldState.FutureTime(4.5f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == _resolve)
        {
            _target = null;
            _activation = default;
        }
    }

    private int CountInside() => _target == null ? 0 : Raid.WithoutSlot().InRadius(_target.Position, Radius).Count();

    private bool IsSafe(int count) => count > 0 && count % _requireMultipleOf == 0;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target == null || !actor.Position.InCircle(_target.Position, Radius))
            return;

        var count = CountInside();
        if (IsSafe(count))
            hints.Add($"Safe count ({count}) - stay", false);
        else
            hints.Add(_requireMultipleOf == 2 ? $"{count} isn't even - GTFO!" : $"{count} not divisible by 3 - GTFO!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_target == null || !actor.Position.InCircle(_target.Position, Radius))
            return;

        // stay if it's safe, leave otherwise, but forbid entering
        if (IsSafe(CountInside()))
            hints.AddForbiddenZone(new SDInvertedCircle(_target.Position, Radius), _activation);
        else
            hints.AddForbiddenZone(new AOEShapeCircle(Radius), _target.Position, default, _activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_target == null)
            return;
        var safe = IsSafe(CountInside());
        Arena.AddCircleUnfilled(_target.Position, Radius, safe ? Colors.Safe : Colors.Danger);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => player == _target ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
}

class Roulette(BossModule module) : Components.GenericAOEs(module, (uint)AID.Death)
{
    private static readonly AOEShapeCone _quarter = new(40, 45.Degrees());

    private bool _active;
    private Angle? _lit;
    private DateTime _litAt;
    private Angle? _aoe;
    private DateTime _until;

    private bool Showing => _aoe != null && (_until == default || WorldState.CurrentTime < _until);
    private bool GlassesAlive => Module.Enemies((uint)OID.FinalHourglass).Any(h => !h.IsDeadOrDestroyed);

    private readonly AOEInstance[] _aoes = new AOEInstance[1];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Showing ? _aoes : [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID != (uint)OID.FinalHourglass || _active)
            return;
        _active = true;
        _lit = _aoe = null;
        _litAt = _until = default;
    }

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (state != 0x8 || !PointerDir(actor.OID, out var dir))
            return;
        _lit = dir;
        _litAt = WorldState.CurrentTime;
        if (_active && !GlassesAlive)
            _aoe = dir;
    }

    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID == (uint)OID.FinalHourglass && actor.IsDead)
            LockIfStopped();
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.FinalHourglass)
            LockIfStopped();
    }

    public override void Update()
    {
        // cycle is about 1s, so a 1.3s stall should mean the finger stopped spinning
        if (_aoe == null && _lit != null && _active && WorldState.CurrentTime >= _litAt.AddSeconds(1.3f))
            _aoe = _lit;
        if (Showing)
            _aoes[0] = new(_quarter, Arena.Center, _aoe!.Value);
    }

    private void LockIfStopped()
    {
        if (_aoe == null && _lit != null && _active && !GlassesAlive)
            _aoe = _lit;
    }

    private static bool PointerDir(uint oid, out Angle dir)
    {
        dir = oid switch
        {
            (uint)OID.RoulettePointerNE => 45.Degrees(),
            (uint)OID.RoulettePointerSE => 135.Degrees(),
            (uint)OID.RoulettePointerSW => -135.Degrees(),
            (uint)OID.RoulettePointerNW => -45.Degrees(),
            _ => default
        };
        return oid is (uint)OID.RoulettePointerNE or (uint)OID.RoulettePointerSE or (uint)OID.RoulettePointerSW or (uint)OID.RoulettePointerNW;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Death)
        {
            _aoe = spell.Rotation;
            _until = WorldState.FutureTime(3);
            _active = false;
            Update();
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!_active && !Showing)
            return;
        var left = Module.Enemies((uint)OID.FinalHourglass).Count(h => !h.IsDeadOrDestroyed);
        if (left > 0)
            hints.Add($"Roulette: kill hourglasses ({left} left)!");
        else if (Showing)
            hints.Add("Roulette: GTFO from cone!");
    }
}

class DoomPads(BossModule module) : BossComponent(module)
{
    private BitMask _dooms;
    private Actor? _activePlatform;
    private static readonly AOEShapeCircle _platformShape = new(2);
    private static readonly uint[] _platformOIDs = [(uint)OID.DoomPlatformNE, (uint)OID.DoomPlatformNW, (uint)OID.DoomPlatformSE, (uint)OID.DoomPlatformSW];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_dooms[slot])
            hints.Add("Cleanse your doom on glowy platform!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_dooms[slot] && _activePlatform is { } target)
            hints.AddForbiddenZone(new SDInvertedCircle(target.Position, _platformShape.Radius), actor.FindStatus((uint)SID.Doom)!.Value.ExpireAt);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_dooms[pcSlot])
            _platformShape.Draw(Arena, _activePlatform, Colors.SafeFromAOE);
    }

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (state == 0x4 && _platformOIDs.Contains(actor.OID))
            _activePlatform = actor;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
            _dooms.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
            _dooms.Clear(Raid.FindSlot(actor.InstanceID));
    }
}

class EyesOnMe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EyesOnMe, new AOEShapeCircle(31.8f));
class Paralyze(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Paralyze, showNameInHint: true);
class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.FinalHourglass, (uint)OID.GrimReaper, (uint)OID.AngraMainyusDaewa], 1);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3231)]
public class A31AngraMainyu(WorldState ws, Actor primary) : BossModule(ws, primary, new(-147, 297), new ArenaBoundsCircle(30));
