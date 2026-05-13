namespace BossMod.Components;

// Compatibility adapters for modules authored against the Reborn component naming.
// They map to this fork's existing component implementations.
file enum CompatAID : uint { }

public class SimpleAOEs(BossModule module, uint aid, AOEShape shape, int maxCasts = int.MaxValue) : StandardAOEs(module, (Enum)Enum.ToObject(typeof(CompatAID), aid), shape, maxCasts)
{
    public SimpleAOEs(BossModule module, uint aid, float radius, int maxCasts = int.MaxValue) : this(module, aid, new AOEShapeCircle(radius), maxCasts) { }
}

public class SimpleAOEGroups(BossModule module, uint[] aids, AOEShape shape, int maxCasts = int.MaxValue) : GroupedAOEs(module, [.. aids.Select(a => (Enum)Enum.ToObject(typeof(CompatAID), a))], shape, maxCasts)
{
    public SimpleAOEGroups(BossModule module, uint[] aids, float radius, int maxCasts = int.MaxValue) : this(module, aids, new AOEShapeCircle(radius), maxCasts) { }
}

public class SimpleKnockbacks(BossModule module, uint aid, float distance, AOEShape? shape = null, Knockback.Kind kind = Knockback.Kind.AwayFromOrigin, bool stopAtWall = false)
    : KnockbackFromCastTarget(module, (Enum)Enum.ToObject(typeof(CompatAID), aid), distance, shape: shape, kind: kind, stopAtWall: stopAtWall)
{
}

public class Voidzone(BossModule module, float radius, Func<BossModule, IEnumerable<Actor>> sources, float moveHintLength = default)
    : PersistentVoidzone(module, radius, sources, moveHintLength)
{
}

public class VoidzoneAtCastTarget(BossModule module, float radius, uint aid, Func<BossModule, IEnumerable<Actor>> sources, double castEventToSpawn = default)
    : PersistentVoidzoneAtCastTarget(module, radius, (Enum)Enum.ToObject(typeof(CompatAID), aid), sources, (float)castEventToSpawn)
{
}

public class RaidwideCasts(BossModule module, uint[] aids, string hint = "Raidwide") : RaidwideCast(module, (Enum)Enum.ToObject(typeof(CompatAID), default(uint)), hint)
{
    private readonly ActionID[] _aids = [.. aids.Select(a => ActionID.MakeSpell((Enum)Enum.ToObject(typeof(CompatAID), a)))];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_aids.Contains(spell.Action))
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aids.Contains(spell.Action))
            Casters.Remove(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aids.Contains(spell.Action))
            ++NumCasts;
    }
}

public abstract class GenericKnockback(BossModule module, uint aid = default, int maxCasts = int.MaxValue, bool stopAtWall = false, bool stopAfterWall = false)
    : Knockback(module, aid == default ? null : (Enum)Enum.ToObject(typeof(CompatAID), aid), maxCasts: maxCasts, stopAtWall: stopAtWall || stopAfterWall)
{
    public new enum Kind
    {
        None,
        AwayFromOrigin,
        TowardsOrigin,
        DirBackward,
        DirForward,
        DirLeft,
        DirRight
    }

    public readonly struct SafeWall(WPos vertex1, WPos vertex2)
    {
        public readonly WPos Vertex1 = vertex1;
        public readonly WPos Vertex2 = vertex2;
    }

    public readonly struct Knockback(
        WPos origin,
        float distance,
        DateTime activation = default,
        AOEShape? shape = null,
        Angle direction = default,
        Kind kind = Kind.AwayFromOrigin,
        float minDistance = default,
        IReadOnlyList<SafeWall>? safeWalls = null,
        ulong actorID = default,
        bool ignoreImmunes = false)
    {
        public readonly WPos Origin = origin;
        public readonly float Distance = distance;
        public readonly DateTime Activation = activation;
        public readonly AOEShape? Shape = shape;
        public readonly Angle Direction = direction;
        public readonly Kind Kind = kind;
        public readonly float MinDistance = minDistance;
        public readonly IReadOnlyList<SafeWall> SafeWalls = safeWalls ?? [];
        public readonly ulong ActorID = actorID;
        public readonly bool IgnoreImmunes = ignoreImmunes;
    }

    public abstract ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor);

    public override IEnumerable<global::BossMod.Components.Knockback.Source> Sources(int slot, Actor actor)
    {
        var active = ActiveKnockbacks(slot, actor);
        var sources = new List<global::BossMod.Components.Knockback.Source>(active.Length);
        foreach (var kb in active)
            sources.Add(new(kb.Origin, kb.Distance, kb.Activation, kb.Shape, kb.Direction, ConvertKind(kb.Kind), kb.MinDistance));
        return sources;
    }

    private static global::BossMod.Components.Knockback.Kind ConvertKind(Kind kind) => kind switch
    {
        Kind.AwayFromOrigin => global::BossMod.Components.Knockback.Kind.AwayFromOrigin,
        Kind.TowardsOrigin => global::BossMod.Components.Knockback.Kind.TowardsOrigin,
        Kind.DirBackward => global::BossMod.Components.Knockback.Kind.TowardsOrigin,
        Kind.DirForward => global::BossMod.Components.Knockback.Kind.DirForward,
        Kind.DirLeft => global::BossMod.Components.Knockback.Kind.DirLeft,
        Kind.DirRight => global::BossMod.Components.Knockback.Kind.DirRight,
        _ => global::BossMod.Components.Knockback.Kind.None
    };
}

public abstract class GenericBaitStack(BossModule module, uint aid = default, bool onlyShowOutlines = false)
    : GenericBaitAway(module, aid == default ? null : (Enum)Enum.ToObject(typeof(CompatAID), aid), alwaysDrawOtherBaits: !onlyShowOutlines, damageType: AIHints.PredictedDamageType.Shared)
{
    public const string HintStack = "Stack!";
    public const string HintAvoidOther = "GTFO from other stacks!";
    public const string HintAvoid = "GTFO from stack!";

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!EnableHints)
            return;

        if (ActiveBaitsOn(actor).Any())
        {
            hints.Add(HintStack, ActiveBaitsOn(actor).Any(b => !PlayersClippedBy(b).Any()));
            if (ActiveBaitsNotOn(actor).Any(b => IsClippedBy(actor, b)))
                hints.Add(HintAvoidOther);
        }
        else if (ActiveBaitsNotOn(actor).Any(b => b.Forbidden[slot] && IsClippedBy(actor, b)))
        {
            hints.Add(HintAvoid);
        }
        else if (ActiveBaitsNotOn(actor).Any(b => !b.Forbidden[slot] && IsClippedBy(actor, b)))
        {
            hints.Add(HintStack, false);
        }
    }
}

public class LineStack(BossModule module, uint aidMarker, uint aidResolve, double activationDelay = 5.1d, float range = 50f, float halfWidth = 4f, int minStackSize = 4, int maxStackSize = int.MaxValue, int maxCasts = 1, bool markerIsFinalTarget = true, uint iconID = default)
    : GenericBaitStack(module)
{
    public LineStack(BossModule module, uint iconID, uint aidResolve, double activationDelay = 5.1d, float range = 50f, float halfWidth = 4f, int minStackSize = 4, int maxStackSize = int.MaxValue, int maxCasts = 1, bool markerIsFinalTarget = true)
        : this(module, default, aidResolve, activationDelay, range, halfWidth, minStackSize, maxStackSize, maxCasts, markerIsFinalTarget, iconID)
    {
    }

    private readonly uint _aidMarker = aidMarker;
    private readonly uint _aidResolve = aidResolve;
    private readonly uint _iconID = iconID;
    private readonly float _activationDelay = (float)activationDelay;
    private readonly int _maxCasts = maxCasts;
    private readonly int _minStackSize = minStackSize;
    private readonly int _maxStackSize = maxStackSize;
    private readonly bool _markerIsFinalTarget = markerIsFinalTarget;
    private readonly AOEShapeRect _shape = new(range, halfWidth);
    private int _castCounter;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (_aidMarker != default && spell.Action.ID == _aidMarker && WorldState.Actors.Find(spell.MainTargetID) is { } target)
        {
            CurrentBaits.Add(new(caster, target, _shape, WorldState.FutureTime(_activationDelay), MaxCasts: _maxCasts));
        }
        else if (spell.Action.ID == _aidResolve)
        {
            Resolve(spell.MainTargetID);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (_iconID != default && iconID == _iconID && WorldState.Actors.Find(targetID) is { } target)
            CurrentBaits.Add(new(actor, target, _shape, WorldState.FutureTime(_activationDelay), MaxCasts: _maxCasts));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_aidMarker == default && _iconID == default && spell.Action.ID == _aidResolve && WorldState.Actors.Find(spell.TargetID) is { } target)
            CurrentBaits.Add(new(caster, target, _shape, Module.CastFinishAt(spell), MaxCasts: _maxCasts));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aidMarker == default && _iconID == default && spell.Action.ID == _aidResolve)
            Resolve(spell.TargetID);
    }

    private void Resolve(ulong targetID)
    {
        _ = _minStackSize + _maxStackSize;
        if (++_castCounter < _maxCasts)
            return;

        _castCounter = 0;
        if (_markerIsFinalTarget && targetID != default)
        {
            var index = CurrentBaits.FindIndex(b => b.Target.InstanceID == targetID);
            if (index >= 0)
            {
                CurrentBaits.RemoveAt(index);
                ++NumCasts;
                return;
            }
        }

        if (CurrentBaits.Count != 0)
        {
            CurrentBaits.RemoveAt(0);
            ++NumCasts;
        }
    }
}

public abstract class CleansableDebuff(BossModule module, uint statusID, string noun = "Doom", string adjective = "doomed") : BossComponent(module)
{
    private readonly List<Actor> _affected = [];
    private readonly List<Actor> _pending = [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == statusID && !_affected.Contains(actor))
            _affected.Add(actor);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == statusID)
            _pending.Add(actor);
    }

    public override void Update()
    {
        foreach (var actor in _pending)
            if (actor.FindStatus(statusID) == null)
                _affected.Remove(actor);
        _pending.Clear();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_affected.Count == 0)
            return;

        if (_affected.Contains(actor))
            hints.Add(actor.Class.CanEsuna() ? $"Cleanse yourself! ({noun})." : $"You were {adjective}! Get cleansed fast.");
        else if (actor.Class.CanEsuna())
            foreach (var affected in _affected)
                hints.Add($"Cleanse {affected.Name}! ({noun})");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!actor.Class.CanEsuna())
            return;

        var action = actor.Class == Class.BRD ? ActionID.MakeSpell(BRD.AID.WardensPaean) : ActionID.MakeSpell(ClassShared.AID.Esuna);
        foreach (var affected in _affected)
            hints.ActionsToExecute.Push(action, affected, ActionQueue.Priority.High, castTime: action.ID == (uint)ClassShared.AID.Esuna ? 1f : 0);
    }
}
