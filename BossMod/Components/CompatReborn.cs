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

public class SimpleKnockbackGroups(BossModule module, uint[] aids, float distance, bool ignoreImmunes = false, int maxCasts = int.MaxValue, AOEShape? shape = null, Knockback.Kind kind = Knockback.Kind.AwayFromOrigin, float minDistance = default, bool minDistanceBetweenHitboxes = false, bool stopAtWall = false, bool stopAfterWall = false)
    : KnockbackFromCastTarget(module, (Enum)Enum.ToObject(typeof(CompatAID), default(uint)), distance, ignoreImmunes, maxCasts, shape, kind, minDistance, minDistanceBetweenHitboxes, stopAtWall || stopAfterWall)
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

public class CastHints(BossModule module, uint[] aids, string hint, bool showCastTimeLeft = false) : CastHint(module, (Enum)Enum.ToObject(typeof(CompatAID), default(uint)), hint, showCastTimeLeft)
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
}

public class RaidwideCastsDelay(BossModule module, uint[] aidsVisual, uint[] aidsAOE, double delay, string hint = "Raidwide")
    : RaidwideCastDelay(module, (Enum)Enum.ToObject(typeof(CompatAID), default(uint)), (Enum)Enum.ToObject(typeof(CompatAID), default(uint)), (float)delay, hint)
{
    private readonly ActionID[] _aidsVisual = [.. aidsVisual.Select(a => ActionID.MakeSpell((Enum)Enum.ToObject(typeof(CompatAID), a)))];
    private readonly ActionID[] _aidsAOE = [.. aidsAOE.Select(a => ActionID.MakeSpell((Enum)Enum.ToObject(typeof(CompatAID), a)))];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_aidsVisual.Contains(spell.Action))
            Activation = Module.CastFinishAt(spell, Delay);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aidsAOE.Contains(spell.Action))
        {
            ++NumCasts;
            Activation = default;
        }
    }
}

public class Dispel(BossModule module, uint statusID, uint action = default) : DispelHint(module, statusID, action == default ? null : (Enum)Enum.ToObject(typeof(CompatAID), action), includeTargetName: true)
{
}

public class SingleTargetCasts(BossModule module, uint[] aids, string hint = "Tankbuster") : SingleTargetCast(module, (Enum)Enum.ToObject(typeof(CompatAID), default(uint)), hint)
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

public class SingleTargetEventDelay(BossModule module, uint actionVisual, uint actionAOE, double delay, string hint = "Tankbuster")
    : SingleTargetInstant(module, (Enum)Enum.ToObject(typeof(CompatAID), actionAOE), (float)delay, hint)
{
    public readonly ActionID ActionVisual = ActionID.MakeSpell((Enum)Enum.ToObject(typeof(CompatAID), actionVisual));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action != ActionVisual)
            return;

        var target = spell.MainTargetID != caster.InstanceID ? spell.MainTargetID : caster.TargetID;
        Targets.Add((Raid.FindSlot(target), WorldState.FutureTime(Delay)));
    }
}

public class SingleTargetDelayableCasts(BossModule module, uint[] aids, string hint = "Tankbuster", AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Tankbuster)
    : SingleTargetCastDelay(module, (Enum)Enum.ToObject(typeof(CompatAID), default(uint)), (Enum)Enum.ToObject(typeof(CompatAID), default(uint)), 0, hint, damageType)
{
    private readonly ActionID[] _aids = [.. aids.Select(a => ActionID.MakeSpell((Enum)Enum.ToObject(typeof(CompatAID), a)))];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (!_aids.Contains(spell.Action))
            return;

        var target = spell.TargetID != caster.InstanceID ? spell.TargetID : caster.TargetID;
        Targets.Add((Raid.FindSlot(target), Module.CastFinishAt(spell, Delay)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (!_aids.Contains(spell.Action))
            return;

        ++NumCasts;
        Targets.RemoveAll(t => Raid[t.slot]?.InstanceID == spell.MainTargetID);
    }
}

public class SimpleAOEGroupsByTimewindow(BossModule module, uint[] aids, AOEShape shape, double timeWindowInSeconds = 1d, int expectedNumCasters = 99, double riskyWithSecondsLeft = default)
    : SimpleAOEGroups(module, aids, shape, int.MaxValue)
{
    public SimpleAOEGroupsByTimewindow(BossModule module, uint[] aids, float radius, double timeWindowInSeconds = 1d, int expectedNumCasters = 99, double riskyWithSecondsLeft = default)
        : this(module, aids, new AOEShapeCircle(radius), timeWindowInSeconds, expectedNumCasters, riskyWithSecondsLeft)
    {
    }

    private readonly float _timeWindowInSeconds = (float)timeWindowInSeconds;
    private readonly double _riskyWithSecondsLeft = riskyWithSecondsLeft;
    private readonly int _expectedNumCasters = expectedNumCasters;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Casters.Count == 0)
            return [];

        _ = _expectedNumCasters;
        var firstActivation = Module.CastFinishAt(Casters[0].CastInfo);
        var deadline = firstActivation.AddSeconds(_timeWindowInSeconds);
        var now = WorldState.CurrentTime;
        return Casters
            .TakeWhile(c => Module.CastFinishAt(c.CastInfo) < deadline)
            .Select(c =>
            {
                var activation = Module.CastFinishAt(c.CastInfo);
                var risky = _riskyWithSecondsLeft == default || activation.AddSeconds(-_riskyWithSecondsLeft) <= now;
                return new AOEInstance(Shape, c.CastInfo!.LocXZ, c.CastInfo.Rotation, activation, Risky: risky);
            });
    }
}

public class SimpleChargeAOEGroups(BossModule module, uint[] aids, float halfWidth, int maxCasts = int.MaxValue, int expectedNumCasters = 99, double riskyWithSecondsLeft = 0d, float extraLengthFront = 0f)
    : GenericAOEs(module)
{
    private readonly ActionID[] _aids = [.. aids.Select(a => ActionID.MakeSpell((Enum)Enum.ToObject(typeof(CompatAID), a)))];
    private readonly List<AOEInstance> _aoes = [];
    private readonly int _expectedNumCasters = expectedNumCasters;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(maxCasts);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (!_aids.Contains(spell.Action))
            return;

        _ = _expectedNumCasters;
        var dir = spell.LocXZ - caster.Position;
        _aoes.Add(new(new AOEShapeRect(dir.Length() + extraLengthFront, halfWidth), caster.Position, Angle.FromDirection(dir), Module.CastFinishAt(spell), Risky: riskyWithSecondsLeft == default));
        _aoes.SortBy(a => a.Activation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aids.Contains(spell.Action))
            _aoes.RemoveAll(a => a.Origin.AlmostEqual(caster.Position, 1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aids.Contains(spell.Action))
            ++NumCasts;
    }
}

public class SimpleExaflare(BossModule module, AOEShape shape, uint aidFirst, uint aidRest, float distance, double timeToMove, int explosionsLeft, int maxShownExplosions, bool castEvent = false, bool locationBased = false, Angle rotation = default)
    : Exaflare(module, shape)
{
    private readonly float _timeToMove = (float)timeToMove;
    private readonly bool _castEvent = castEvent;
    private readonly bool _locationBased = locationBased;
    private readonly Angle _rotation = rotation;
    public int NumLinesFinished;

    public SimpleExaflare(BossModule module, float radius, uint aidFirst, uint aidRest, float distance, double timeToMove, int explosionsLeft, int maxShownExplosions, bool castEvent = false, bool locationBased = false)
        : this(module, new AOEShapeCircle(radius), aidFirst, aidRest, distance, timeToMove, explosionsLeft, maxShownExplosions, castEvent, locationBased)
    {
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == aidFirst)
            Lines.Add(new()
            {
                Next = _locationBased ? spell.LocXZ : caster.Position,
                Advance = distance * caster.Rotation.ToDirection(),
                Rotation = _rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = _timeToMove,
                ExplosionsLeft = explosionsLeft,
                MaxShownExplosions = maxShownExplosions
            });
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (!_castEvent && (spell.Action.ID == aidFirst || spell.Action.ID == aidRest))
            AdvanceMatchingLine(spell.LocXZ);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_castEvent && (spell.Action.ID == aidFirst || spell.Action.ID == aidRest))
            AdvanceMatchingLine(_locationBased ? spell.TargetXZ : caster.Position);
    }

    private void AdvanceMatchingLine(WPos position)
    {
        var index = Lines.FindIndex(l => l.Next.AlmostEqual(position, 1));
        if (index < 0)
            return;

        var line = Lines[index];
        AdvanceLine(line, position);
        if (line.ExplosionsLeft <= 0)
        {
            Lines.RemoveAt(index);
            ++NumLinesFinished;
        }
    }
}

public class CastGazes(BossModule module, uint[] aids, bool inverted = false, float range = 10000f, int maxCasts = int.MaxValue, int expectedNumCasters = 99)
    : GenericGaze(module, (Enum)Enum.ToObject(typeof(CompatAID), default(uint)), inverted)
{
    private readonly ActionID[] _aids = [.. aids.Select(a => ActionID.MakeSpell((Enum)Enum.ToObject(typeof(CompatAID), a)))];
    private readonly List<Actor> _casters = [];
    private readonly int _maxCasts = maxCasts;
    private readonly int _expectedNumCasters = expectedNumCasters;

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor) => _casters.Select(c => new Eye(c.Position, Module.CastFinishAt(c.CastInfo), Range: range));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_aids.Contains(spell.Action))
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        _ = _expectedNumCasters;
        if (_aids.Contains(spell.Action))
            _casters.Remove(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aids.Contains(spell.Action) && NumCasts < _maxCasts)
            ++NumCasts;
    }
}

public abstract class ThinIce(BossModule module, float distance, bool createforbiddenzones = false, uint statusID = 911u, bool stopAtWall = false, bool stopAfterWall = false)
    : GenericKnockback(module, stopAtWall: stopAtWall, stopAfterWall: stopAfterWall)
{
    public readonly uint StatusID = statusID;
    public readonly float Distance = distance;
    public BitMask Mask;
    private readonly bool _createForbiddenZones = createforbiddenzones;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (!Mask[slot])
            return [];

        return new Knockback[1] { new(actor.Position, Distance, default, default, actor.Rotation, Kind.DirForward) };
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == StatusID && Raid.TryFindSlot(actor, out var slot))
            Mask.Set(slot);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == StatusID && Raid.TryFindSlot(actor, out var slot))
            Mask.Clear(slot);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        _ = _createForbiddenZones;
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

public abstract class TemporaryMisdirection(BossModule module, uint aid, string hint = "Applies temporary misdirection")
    : CastHint(module, (Enum)Enum.ToObject(typeof(CompatAID), aid), hint)
{
    private BitMask _mask;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID is 1422u or 2936u or 3694u or 3909u && Raid.TryFindSlot(actor.InstanceID, out var slot))
            _mask.Set(slot);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is 1422u or 2936u or 3694u or 3909u && Raid.TryFindSlot(actor.InstanceID, out var slot))
            _mask.Clear(slot);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_mask[slot])
            hints.AddSpecialMode(AIHints.SpecialMode.Misdirection, default);
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

public class StretchTetherDuo(BossModule module, float minimumDistance, double activationDelay, uint tetherIDBad = 57u, uint tetherIDGood = 1u, AOEShape? shape = null, uint aid = default, uint enemyOID = default, bool knockbackImmunity = false)
    : GenericBaitAway(module, aid == default ? null : (Enum)Enum.ToObject(typeof(CompatAID), aid), damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public const string HintGood = "Tether is stretched!";
    public const string HintBad = "Stretch tether further!";

    public readonly uint TIDGood = tetherIDGood;
    public readonly uint TIDBad = tetherIDBad;
    public readonly float MinimumDistance = minimumDistance;
    public readonly List<(Actor Actor, uint ID)> TetherOnActor = [];
    public readonly List<(Actor Actor, DateTime Activation)> ActivationDelayOnActor = [];

    private readonly AOEShape _shape = shape ?? new AOEShapeCircle(0.1f);
    private readonly float _minimumDistanceSq = minimumDistance * minimumDistance;
    private readonly float _activationDelay = (float)activationDelay;
    private readonly IReadOnlyList<Actor> _enemies = enemyOID == default ? [] : module.Enemies(enemyOID);
    private readonly bool _knockbackImmunity = knockbackImmunity;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player == null || enemy == null)
            return;

        var activation = WorldState.FutureTime(_activationDelay);
        CurrentBaits.Add(new(enemy, player, _shape, activation));
        TetherOnActor.Add((player, tether.ID));
        ActivationDelayOnActor.Add((player, activation));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player == null || enemy == null)
            return;

        CurrentBaits.RemoveAll(b => b.Source == enemy && b.Target == player);
        TetherOnActor.RemoveAll(t => t.Actor == player && t.ID == tether.ID);
        ActivationDelayOnActor.RemoveAll(t => t.Actor == player);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        _ = _knockbackImmunity;
        foreach (var bait in ActiveBaitsOn(actor))
        {
            if ((bait.Source.Position - actor.Position).LengthSq() < _minimumDistanceSq)
                hints.Add(HintBad);
            else
                hints.Add(HintGood, false);
        }
        base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var bait in ActiveBaits)
        {
            var color = IsTether(bait.Target, TIDGood) || (bait.Source.Position - bait.Target.Position).LengthSq() >= _minimumDistanceSq ? ArenaColor.Safe : ArenaColor.Danger;
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(bait.Source.Position, bait.Target.Position, 0xFF000000, 2);
            Arena.AddLine(bait.Source.Position, bait.Target.Position, color);
        }
    }

    protected bool IsTether(Actor actor, uint id) => TetherOnActor.Any(t => t.Actor == actor && t.ID == id);

    private (Actor? player, Actor? enemy) DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TIDBad && tether.ID != TIDGood)
            return (null, null);

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
            return (null, null);

        var (player, enemy) = source.Type is ActorType.Player or ActorType.DutySupport ? (source, target) : (target, source);
        if (!(player.Type is ActorType.Player or ActorType.DutySupport) || enemy.Type == ActorType.Player || (_enemies.Count != 0 && !_enemies.Contains(enemy)))
        {
            ReportError($"Unexpected tether pair: {source.InstanceID:X} -> {target.InstanceID:X}");
            return (null, null);
        }

        return (player, enemy);
    }
}

public class InterceptTether(BossModule module, uint aid, uint tetherIDBad = 84u, uint tetherIDGood = 17u, uint[]? excludedAllies = null)
    : CastCounter(module, (Enum)Enum.ToObject(typeof(CompatAID), aid))
{
    public readonly uint TIDGood = tetherIDGood;
    public readonly uint TIDBad = tetherIDBad;
    public readonly uint[]? ExcludedAllies = excludedAllies;
    protected readonly List<(Actor Player, Actor Enemy)> _tethers = [];
    protected BitMask _tetheredPlayers;
    protected const string Hint = "Grab the tether!";

    public bool Active => _tethers.Count != 0;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Active && !_tetheredPlayers[slot])
            hints.Add(Hint);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!Active)
            return;

        var excluded = new List<Actor>();
        if (ExcludedAllies != null)
            foreach (var oid in ExcludedAllies)
                excluded.AddRange(Module.Enemies(oid));

        foreach (var tether in _tethers)
        {
            var color = tether.Player.Role == Role.Tank || IsExcluded(tether.Player, excluded) ? ArenaColor.Safe : ArenaColor.Danger;
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(tether.Enemy.Position, tether.Player.Position, 0xFF000000, 2);
            Arena.AddLine(tether.Enemy.Position, tether.Player.Position, color);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var pair = DetermineTetherSides(source, tether);
        if (pair == null)
            return;

        _tethers.Add(pair.Value);
        if (Raid.TryFindSlot(pair.Value.Player, out var slot))
            _tetheredPlayers.Set(slot);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        var pair = DetermineTetherSides(source, tether);
        if (pair == null)
            return;

        _tethers.RemoveAll(t => t.Player == pair.Value.Player && t.Enemy == pair.Value.Enemy);
        if (Raid.TryFindSlot(pair.Value.Player, out var slot))
            _tetheredPlayers.Clear(slot);
    }

    private (Actor Player, Actor Enemy)? DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TIDBad && tether.ID != TIDGood)
            return null;

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
            return null;

        var (player, enemy) = source.Type is ActorType.Player or ActorType.DutySupport ? (source, target) : (target, source);
        if (!(player.Type is ActorType.Player or ActorType.DutySupport) || enemy.Type == ActorType.Player)
        {
            ReportError($"Unexpected tether pair: {source.InstanceID:X} -> {target.InstanceID:X}");
            return null;
        }

        return (player, enemy);
    }

    private static bool IsExcluded(Actor actor, List<Actor> excluded) => excluded.Contains(actor);
}

public class InterceptTetherAOE(BossModule module, uint aid, uint tetherID, float radius, uint[]? excludedAllies = null)
    : InterceptTether(module, aid, tetherID, tetherID, excludedAllies)
{
    public readonly float Radius = radius;
    public DateTime Activation;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (!_tetheredPlayers[slot])
            return;

        if (Raid.WithoutSlot().InRadiusExcluding(actor, Radius).Any())
            hints.Add("GTFO from raid!");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Activation = Module.CastFinishAt(spell);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var tether in _tethers)
            hints.AddForbiddenZone(ShapeContains.Circle(tether.Player.Position, Radius), Activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var tether in _tethers)
            GenericTowers.DrawTower(Arena, tether.Player.Position, Radius, tether.Player == pc);
    }
}

public class InterceptTetherStatus(BossModule module, uint aid, uint tetherID, uint sid, float radius = 0f, uint[]? excludedAllies = null)
    : InterceptTetherAOE(module, aid, tetherID, radius, excludedAllies)
{
    public readonly uint StatusID = sid;
    private BitMask _hasStatus;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == StatusID && Raid.TryFindSlot(actor, out var slot))
            _hasStatus.Set(slot);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == StatusID && Raid.TryFindSlot(actor, out var slot))
            _hasStatus.Clear(slot);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Active && !_tetheredPlayers[slot] && !_hasStatus[slot])
            hints.Add("Grab the tether!");
        else
            base.AddHints(slot, actor, hints);

        if (_tetheredPlayers[slot] && _hasStatus[slot])
            hints.Add("Give tether away!");
    }
}

public class BaitAwayChargeTether(BossModule module, float halfWidth, double activationDelay, uint aidGood, uint aidBad = default, uint tetherIDBad = 57u, uint tetherIDGood = 1u, uint enemyOID = default, float minimumDistance = default)
    : StretchTetherDuo(module, minimumDistance, activationDelay, tetherIDBad, tetherIDGood, new AOEShapeRect(default, halfWidth), default, enemyOID)
{
    public readonly uint AidGood = aidGood;
    public readonly uint AidBad = aidBad;
    public readonly float HalfWidth = halfWidth;

    public override void Update()
    {
        base.Update();
        for (var i = 0; i < CurrentBaits.Count; ++i)
        {
            var bait = CurrentBaits[i];
            var length = (bait.Target.Position - bait.Source.Position).Length();
            bait.Shape = new AOEShapeRect(length, HalfWidth);
            CurrentBaits[i] = bait;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != AidGood && spell.Action.ID != AidBad)
            return;

        ++NumCasts;
        CurrentBaits.RemoveAll(b => b.Target.InstanceID == spell.MainTargetID);
    }
}

public class StretchTetherSingle(BossModule module, uint tetherID, float minimumDistance, AOEShape? shape = null, uint aid = default, uint enemyOID = default, double activationDelay = default, bool knockbackImmunity = false, bool needToKite = false)
    : StretchTetherDuo(module, minimumDistance, activationDelay, tetherID, tetherID, shape, aid, enemyOID, knockbackImmunity)
{
    public const string HintKite = "Kite the add!";

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (needToKite && IsTether(actor, TIDBad))
            hints.Add(HintKite);
        else
            base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (needToKite && IsTether(pc, TIDBad))
            foreach (var bait in ActiveBaitsOn(pc))
                Arena.Actor(bait.Source, ArenaColor.Object, true);
    }
}

public class GenericTowersOpenWorld(BossModule module, uint aid = default, bool prioritizeInsufficient = false, bool prioritizeEmpty = false)
    : GenericTowers(module, aid == default ? null : (Enum)Enum.ToObject(typeof(CompatAID), aid))
{
    public new sealed class Tower(WPos position, float radius, int minSoakers = 1, int maxSoakers = 1, HashSet<Actor>? allowedSoakers = null, DateTime activation = default, ulong actorID = default)
    {
        public WPos Position = position;
        public float Radius = radius;
        public int MinSoakers = minSoakers;
        public int MaxSoakers = maxSoakers;
        public HashSet<Actor>? AllowedSoakers = allowedSoakers;
        public DateTime Activation = activation;
        public ulong ActorID = actorID;

        public bool IsInside(WPos pos) => pos.InCircle(Position, Radius);
        public bool IsInside(Actor actor) => IsInside(actor.Position);
        public int NumInside(BossModule module) => Soakers(module, AllowedSoakers).Count(IsInside);
        public bool CorrectAmountInside(BossModule module) => NumInside(module) is var count && count >= MinSoakers && count <= MaxSoakers;
        public bool InsufficientAmountInside(BossModule module) => NumInside(module) < MaxSoakers;
        public void InitializeAllowedSoakers(BossModule module) => AllowedSoakers ??= Soakers(module, null);
    }

    public readonly List<Tower> RebornTowers = [];
    public bool PrioritizeInsufficient = prioritizeInsufficient;
    public bool PrioritizeEmpty = prioritizeEmpty;

    public virtual IEnumerable<Tower> ActiveTowers(int slot, Actor actor) => RebornTowers;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        SyncTowers(slot, actor);
        base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        SyncTowers(slot, actor);
        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        SyncTowers(pcSlot, pc);
        base.DrawArenaForeground(pcSlot, pc);
    }

    private void SyncTowers(int slot, Actor actor)
    {
        Towers.Clear();
        foreach (var t in ActiveTowers(slot, actor))
            Towers.Add(new(t.Position, t.Radius, t.MinSoakers, t.MaxSoakers, ForbiddenMask(t), t.Activation));
    }

    private BitMask ForbiddenMask(Tower tower)
    {
        var mask = new BitMask();
        if (tower.AllowedSoakers == null)
            return mask;

        foreach (var (slot, actor) in Raid.WithSlot())
            if (!tower.AllowedSoakers.Contains(actor))
                mask.Set(slot);
        return mask;
    }

    private static HashSet<Actor> Soakers(BossModule module, HashSet<Actor>? allowed) => allowed ?? [.. module.Raid.WithoutSlot()];
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

public class StatusStackSpread(BossModule module, uint stackSid, uint spreadSid, float stackRadius, float spreadRadius, int minStackSize = 2, int maxStackSize = int.MaxValue, bool raidwideOnResolve = true, bool includeDeadTargets = false)
    : UniformStackSpread(module, stackRadius, spreadRadius, minStackSize, maxStackSize, raidwideOnResolve: raidwideOnResolve, includeDeadTargets: includeDeadTargets)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == stackSid)
            AddStack(actor, status.ExpireAt);
        else if (status.ID == spreadSid)
            AddSpread(actor, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == stackSid)
            Stacks.RemoveAll(s => s.Target == actor);
        else if (status.ID == spreadSid)
            Spreads.RemoveAll(s => s.Target == actor);
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
