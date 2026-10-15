namespace BossMod.Shadowbringers.Raid.E12EdensPromise;

public enum OID : uint
{
    Boss = 0x308D, // R7.500, Eden's Promise
    Helper = 0x233C, // R0.500
    Crystal = 0x18D6, // R0.500, primal crystal tether sources
    Guardian = 0x308E, // R115.380, intermission guardian
    JumpPad = 0x1E8536, // R2.000, EventObj
    TreasuredMemory1 = 0x3096, // R3.500, platform 1
    TreasuredMemory2 = 0x3097, // R3.500, platform 2
    TreasuredMemory3 = 0x3098, // R3.500, platform 3
    LissomSculpture = 0x3091, // R2.000
    ChiseledSculpture = 0x3092, // R3.000
    BeastlySculpture = 0x3093, // R2.500
    BombBoulder = 0x308F, // R1.300
    TitanicBombBoulder = 0x3090, // R3.250
}

public enum AID : uint
{
    AutoAttack = 19229, // Boss->player, no cast, single-target
    BossMove = 22649, // Boss->location, no cast

    Maleficium = 22642, // Boss->self, 4.0s cast, range 40 circle
    InitializeRecall = 22617, // Boss->self, no cast
    Cast = 22619, // Boss->self, 7.0s cast, single-target
    CastResolve = 22618, // Boss->self, no cast, range 40 circle
    JudgmentJolt = 22623, // Helper->self, 7.0s cast, range 10 circle
    TemporaryCurrent = 22620, // Helper->self, 7.0s cast, range 40 width 80 rect
    ConflagStrike = 22621, // Helper->self, 7.0s cast, range 40 150-degree cone
    Ferostorm = 22622, // Helper->self, 7.0s cast, range 40 45-degree cone

    FormlessJudgment = 22643, // Boss->self, 5.0s cast, single-target
    FormlessJudgmentVis = 22644, // Boss->self, no cast
    FormlessJudgmentHit1 = 22646, // Helper->player, no cast
    FormlessJudgmentHit2 = 22645, // Helper->player, no cast

    EternalOblivion1 = 22650, // Guardian->self, 7.0s cast
    EternalOblivion2 = 22651, // Guardian->self, 7.0s cast
    EternalOblivion3 = 22652, // Guardian->self, 7.0s cast
    EternalOblivion4 = 22653, // Guardian->self, 7.0s cast
    GuardianMove = 19232, // Guardian->location, no cast
    GuardianMove2 = 19233,
    GuardianMove3 = 19234,
    GuardianMove4 = 19235,

    ObliterationVis = 19267, // Boss->self, 8.0s cast
    Obliteration = 22660, // Helper->TreasuredMemory, 8.0s cast, range 40 width 6 rect
    EarthShaker = 22661, // Helper->self, 3.0s cast, range 60 30-degree cone
    ClassicalSculpture = 22662, // ChiseledSculpture->self, no cast, range 6 circle
    PalmOfTemperance = 19268, // Boss->self, 5.0s cast
    RapturousReachIntermission = 22663, // Guardian->self, 5.0s cast
    RapturousReachRect = 22665, // Helper->self, 5.5s cast, range 30 width 30 rect (knockback)
    LaserEyeVis = 19271, // Boss->self, 5.0s cast
    LaserEyeTarget = 22666, // Guardian->TreasuredMemory, 5.0s cast
    LaserEye = 22667, // Helper->self, 6.5s cast, range 15 circle (knockback)
    ParadiseLostVis = 19272, // Boss->self, 5.0s cast
    ParadiseLostVis2 = 22654, // Guardian->self, 5.0s cast
    ParadiseLost = 22655, // Helper->self, 7.0s cast, range 40 circle

    Stock = 22624, // Boss->self, 5.0s cast
    Release = 22625, // Boss->self, 7.0s cast
    CastStock = 20012, // Boss->self, 7.0s cast
    JudgmentJoltStock = 20016, // Helper->self, 7.0s cast, range 10 circle
    TemporaryCurrentStock = 20013, // Helper->self, 7.0s cast, range 40 width 80 rect
    ConflagStrikeStock = 20014, // Helper->self, 7.0s cast, range 40 150-degree cone
    FerostormStock = 20015, // Helper->self, 7.0s cast, range 40 45-degree cone

    JunctionTitan = 22627, // Boss->self, 3.0s cast
    EarthenFuryVis = 22634, // Boss->self, 5.0s cast
    EarthenFury = 22635, // Helper->self, no cast, range 60 circle
    ImpactSmall = 22636, // BombBoulder->self, no cast, range 4 circle
    ImpactLarge = 22638, // TitanicBombBoulder->self, no cast, range 4 circle
    ExplosionSmall = 22637, // BombBoulder->self, 6.0s cast, range 10 circle
    ExplosionLarge = 22639, // TitanicBombBoulder->self, 6.0s cast, range 22 circle
    RapturousReach = 22648, // Boss->self, 5.0s cast, range 40 210-degree cone
    UnderTheWeight = 22640, // Helper->player, 5.0s cast, range 6 circle

    JunctionShiva = 22626, // Boss->self, 3.0s cast
    DiamondDustVis = 22628, // Boss->self, 5.0s cast
    DiamondDust = 22629, // Helper->self, no cast, range 60 circle
    IceFloor = 22630, // Helper->self, 6.0s cast, range 20 circle
    FrigidStone = 22631, // Helper->player, 5.0s cast, range 5 circle
    IceFloe = 22632, // Helper->player, 5.0s cast, range 6 circle
    FrigidStone2 = 22633, // Helper->player, 5.0s cast, range 5 circle

    AutoAttackLissom = 22657, // LissomSculpture->TreasuredMemory1, no cast, single-target
    AutoAttackChiseled = 22658, // ChiseledSculpture->TreasuredMemory2, no cast, single-target
    AutoAttackBeastly = 22659, // BeastlySculpture->TreasuredMemory3, no cast, single-target
    RapturousReach2 = 22647, // Boss->self, 5.0s cast, range 40 210-degree cone
    BossAnim = 22641, // Boss->self, no cast, single-target
}

public enum TetherID : uint
{
    Leviathan = 142, // Temporary Current
    Ifrit = 143, // Conflag Strike
    Ramuh = 144, // Judgment Jolt
    Garuda = 145, // Ferostorm
    Shiva = 140,
    Titan = 141,
    Sculpture = 17, // sculpture->TreasuredMemory
}

public enum IconID : uint
{
    EarthShaker = 40,
    Stack = 62,
    Spread = 96,
    FormlessJudgment = 218,
}

class Arenas(BossModule module) : BossComponent(module)
{
    public override void Update()
    {
        var pc = Raid.Player();
        if (pc == null)
            return;

        // Only switch once the player has actually arrived — memory actors spawn while
        // the previous arena's jump pad is still the exit.
        foreach (var (oid, fallback) in E12EdensPromise.MemoryPlatforms)
        {
            var mem = Module.Enemies((uint)oid).FirstOrDefault(a => !a.IsDeadOrDestroyed);
            var center = mem?.Position ?? fallback;
            if (pc.Position.InCircle(center, E12EdensPromise.MemoryBounds.Radius + 2))
            {
                Arena.Center = center;
                Arena.Bounds = E12EdensPromise.MemoryBounds;
                return;
            }
        }

        if (pc.Position.InCircle(E12EdensPromise.FinalCenter, E12EdensPromise.MainBounds.Radius + 5))
        {
            Arena.Center = E12EdensPromise.FinalCenter;
            Arena.Bounds = E12EdensPromise.MainBounds;
            return;
        }

        Arena.Center = E12EdensPromise.InitialCenter;
        Arena.Bounds = E12EdensPromise.MainBounds;
    }
}

class JumpPads(BossModule module) : BossComponent(module)
{
    private bool _arena1Pad;
    private bool _mem1Pad;
    private bool _mem2Pad;
    private bool _mem3Pad;

    public static readonly WPos Arena1Pad = new(62.30f, -21.50f);
    public static readonly WPos Memory1Pad = new(36.0f, 66.6f);
    public static readonly WPos Memory2Pad = new(-52.2f, 54.8f);
    public static readonly WPos Memory3Pad = new(-68.2f, -32.7f);

    public override void Update()
    {
        var pc = Raid.Player();
        if (pc == null)
            return;

        // drop goals once the player has left the platform
        if (_arena1Pad && !pc.Position.InCircle(E12EdensPromise.InitialCenter, E12EdensPromise.MainBounds.Radius + 3))
            _arena1Pad = false;
        if (_mem1Pad && !pc.Position.InCircle(E12EdensPromise.Memory1Center, E12EdensPromise.MemoryBounds.Radius + 3))
            _mem1Pad = false;
        if (_mem2Pad && !pc.Position.InCircle(E12EdensPromise.Memory2Center, E12EdensPromise.MemoryBounds.Radius + 3))
            _mem2Pad = false;
        if (_mem3Pad && !pc.Position.InCircle(E12EdensPromise.Memory3Center, E12EdensPromise.MemoryBounds.Radius + 3))
            _mem3Pad = false;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        switch (index)
        {
            case 0x07:
                // pad becomes step-onable here
                _arena1Pad = state == 0x00020001;
                break;
            case 0x12:
                if (state == 0x00800040)
                    _mem1Pad = true;
                else if (state is 0x00200010 or 0x00080004)
                    _arena1Pad = false;
                if (state == 0x00080004)
                    _mem1Pad = false;
                break;
            case 0x08:
                if (state == 0x00080004)
                    _mem1Pad = false;
                break;
            case 0x13:
                if (state == 0x00800040)
                    _mem2Pad = true;
                else if (state is 0x00200010 or 0x00080004)
                    _mem2Pad = false;
                break;
            case 0x09:
                if (state == 0x00080004)
                    _mem2Pad = false;
                break;
            case 0x14:
                if (state == 0x00800040)
                    _mem3Pad = true;
                else if (state is 0x00200010 or 0x00080004)
                    _mem3Pad = false;
                break;
            case 0x0A:
                if (state == 0x00080004)
                    _mem3Pad = false;
                break;
        }
    }

    private IEnumerable<WPos> ActivePads(Actor pc)
    {
        if (_arena1Pad && pc.Position.InCircle(E12EdensPromise.InitialCenter, E12EdensPromise.MainBounds.Radius + 2))
            yield return Arena1Pad;
        if (_mem1Pad && pc.Position.InCircle(E12EdensPromise.Memory1Center, E12EdensPromise.MemoryBounds.Radius + 2))
            yield return Memory1Pad;
        if (_mem2Pad && pc.Position.InCircle(E12EdensPromise.Memory2Center, E12EdensPromise.MemoryBounds.Radius + 2))
            yield return Memory2Pad;
        if (_mem3Pad && pc.Position.InCircle(E12EdensPromise.Memory3Center, E12EdensPromise.MemoryBounds.Radius + 2))
            yield return Memory3Pad;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var pads = ActivePads(actor).ToList();
        if (pads.Count == 0)
            return;

        hints.GoalZones.Clear();
        foreach (var t in hints.PotentialTargets)
            if (!t.Actor.Position.InCircle(Arena.Center, Arena.Bounds.Radius + 2))
                t.Priority = AIHints.Enemy.PriorityForbidden;

        foreach (var pad in pads)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(pad, E12EdensPromise.JumpPadRadius, 5));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var pad in ActivePads(pc))
            Arena.AddCircleUnfilled(pad, E12EdensPromise.JumpPadRadius, Colors.Safe);
    }
}

class Maleficium(BossModule module) : Components.RaidwideCast(module, (uint)AID.Maleficium);
class ParadiseLost(BossModule module) : Components.RaidwideCast(module, (uint)AID.ParadiseLost);
class EarthenFury(BossModule module) : Components.RaidwideCast(module, (uint)AID.EarthenFuryVis);
class DiamondDust(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiamondDustVis);

class FormlessJudgment(BossModule module) : Components.SingleTargetCast(module, (uint)AID.FormlessJudgment, "Tankbusters");

class JudgmentJolt(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.JudgmentJolt, (uint)AID.JudgmentJoltStock], new AOEShapeCircle(10));
class TemporaryCurrent(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TemporaryCurrent, (uint)AID.TemporaryCurrentStock], new AOEShapeRect(40, 40));
class ConflagStrike(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ConflagStrike, (uint)AID.ConflagStrikeStock], new AOEShapeCone(40, 75.Degrees()));
class Ferostorm(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Ferostorm, (uint)AID.FerostormStock], new AOEShapeCone(40, 22.5f.Degrees()));

class Obliteration(BossModule module) : BossComponent(module)
{
    private Actor? _caster;
    private DateTime _activation;
    private static readonly AOEShapeRect _shape = new(40, 3);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Obliteration)
        {
            _caster = caster;
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Obliteration)
            _caster = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_caster != null && !_shape.Check(actor.Position, _caster.Position, _caster.CastInfo!.Rotation))
            hints.Add("Stack on laser!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_caster?.CastInfo is { } ci)
            hints.AddForbiddenZone(new SDInvertedRect(_caster.Position, ci.Rotation, 40, 0, 3), _activation);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_caster?.CastInfo is { } ci)
            _shape.Draw(Arena, _caster.Position, ci.Rotation, Colors.SafeFromAOE);
    }
}

class EarthShaker(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 15.Degrees()), (uint)IconID.EarthShaker, (uint)AID.EarthShaker, 8.1f)
{
    private Actor? ConeSource()
    {
        foreach (var a in WorldState.Actors)
            if (a.CastInfo?.IsSpell(AID.EarthShaker) == true || a.CastInfo?.IsSpell(AID.Obliteration) == true)
                return a;
        return null;
    }

    public override Actor? BaitSource(Actor target) => ConeSource() ?? Module.PrimaryActor;

    public override void Update()
    {
        if (ConeSource() is not { } src)
            return;
        for (var i = 0; i < CurrentBaits.Count; ++i)
            if (CurrentBaits[i].Source != src)
                CurrentBaits[i] = CurrentBaits[i] with { Source = src };
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var mem = Module.Enemies((uint)OID.TreasuredMemory1).FirstOrDefault(a => a.IsTargetable && !a.IsDeadOrDestroyed);
        if (mem == null)
            return;
        // don't let the cone be aimed at the crystal
        foreach (ref var b in CollectionsMarshal.AsSpan(ActiveBaitsOn(actor)))
            hints.AddForbiddenZone(b.Shape, BaitOrigin(ref b), Angle.FromDirection(mem.Position - BaitOrigin(ref b)), b.Activation);
    }
}

class ClassicalSculptureStack : Components.UniformStackSpread
{
    public ClassicalSculptureStack(BossModule module) : base(module, 6, 0, 2, 4) { }

    // These stacks may overlap; the local generic component always separates stack targets.
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!EnableHints)
            return;
        if (Stacks.FirstOrDefault(s => s.Target == actor) is var own && own.Target != null)
        {
            var inside = Raid.WithSlot().Where(p => p.Item2.Position.InCircle(actor.Position, own.Radius)).ToArray();
            hints.Add(StackHint, inside.Any(p => own.ForbiddenPlayers[p.Item1]) || inside.Length < own.MinSize || inside.Length > own.MaxSize);
        }
        else
        {
            var allowed = ActiveStacks.Where(s => !s.ForbiddenPlayers[slot]).ToArray();
            var participating = allowed.Any(s => actor.Position.InCircle(s.Target.Position, s.Radius));
            var unsatisfied = allowed.Any(s => Raid.WithoutSlot().Count(p => p.Position.InCircle(s.Target.Position, s.Radius)) < s.MinSize);
            if (participating || unsatisfied)
                hints.Add(StackHint, !participating);
        }
        if (ActiveStacks.Any(s => s.Target != actor && s.ForbiddenPlayers[slot] && actor.Position.InCircle(s.Target.Position, s.Radius)))
            hints.Add("GTFO from forbidden stacks!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var stack in ActiveStacks)
        {
            if (MiniArena.Config.ShowOutlinesAndShadows)
                Arena.AddCircleUnfilled(stack.Target.Position, stack.Radius, 0xFF000000, 2);
            Arena.AddCircleUnfilled(stack.Target.Position, stack.Radius, stack.ForbiddenPlayers[pcSlot] ? Colors.Danger : Colors.Safe);
        }
    }

    private Actor? Memory2 => Module.Enemies((uint)OID.TreasuredMemory2).FirstOrDefault(a => a.IsTargetable && !a.IsDeadOrDestroyed);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Stack && Memory2 != null && !IsStackTarget(actor))
            AddStack(actor, WorldState.FutureTime(5.2f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ClassicalSculpture)
            Stacks.Clear();
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.TreasuredMemory2)
            Stacks.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!EnableHints || Stacks.Count == 0)
            return;

        int NumOthersInside(Stack s) => Raid.WithoutSlot().Count(p => p != actor && p.Position.InCircle(s.Target.Position, s.Radius));
        foreach (var stack in ActiveStacks)
            if (stack.Target != actor && (stack.ForbiddenPlayers[slot] || NumOthersInside(stack) >= stack.MaxSize))
                hints.AddForbiddenZone(new SDCircle(stack.Target.Position, stack.Radius), stack.Activation);

        if (Stacks.FirstOrDefault(s => s.Target == actor) is var own && own.Target != null)
        {
            var closest = Raid.WithSlot().Where(p => p.Item2 != actor && !IsStackTarget(p.Item2) && !IsSpreadTarget(p.Item2) && !own.ForbiddenPlayers[p.Item1])
                .Select(p => p.Item2).MinBy(p => (p.Position - actor.Position).LengthSq());
            if (closest != null)
                hints.AddForbiddenZone(new SDInvertedCircle(closest.Position, own.Radius * 0.5f), own.Activation);
        }
        else
        {
            var closest = ActiveStacks.Where(s => !s.ForbiddenPlayers[slot] && NumOthersInside(s) < s.MaxSize)
                .MinBy(s => (s.Target.Position - actor.Position).LengthSq());
            if (closest.Target != null)
                hints.AddForbiddenZone(new SDInvertedCircle(closest.Target.Position, closest.Radius), closest.Activation);
        }

        if (RaidwideOnResolve)
        {
            var affected = new BitMask();
            var activation = DateTime.MaxValue;
            foreach (var stack in ActiveStacks)
            {
                affected |= Raid.WithSlot().Mask() & ~stack.ForbiddenPlayers;
                activation = activation < stack.Activation ? activation : stack.Activation;
            }
            if (affected.Any())
                hints.AddPredictedDamage(affected, activation, AIHints.PredictedDamageType.Shared);
        }

        var mem = Memory2;
        if (mem == null)
            return;
        // keep stack away from crystal's hitbox
        var act = Stacks[0].Activation;
        hints.AddForbiddenZone(new SDCircle(mem.Position, StackRadius + mem.HitboxRadius + 1.5f), act);
    }
}

class UnderTheWeight(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.UnderTheWeight, 6, 2);
class IceFloe(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.IceFloe, 6, 2);
class FrigidStone(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.FrigidStone, 5);
class FrigidStone2(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.FrigidStone2, 5);

class RapturousReachCone(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RapturousReach, (uint)AID.RapturousReach2], new AOEShapeCone(40, 105.Degrees()));

class RapturousReachKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.RapturousReachRect, 15, kind: Kind.DirForward, shape: new AOEShapeRect(30, 15))
{
    private sealed class UnsafeLanding(Func<WPos, bool> unsafePosition) : ShapeDistance
    {
        public override float Distance(in WPos p) => unsafePosition(p) ? float.MinValue : float.MaxValue;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in ActiveKnockbacks(slot, actor))
        {
            if (IsImmune(slot, s.Activation))
                continue;
            hints.AddForbiddenZone(new UnsafeLanding(p =>
            {
                var dest = p + s.Distance * s.Direction.ToDirection();
                return DestinationUnsafe(slot, actor, dest);
            }), s.Activation);
        }
    }
}

class LaserEyeKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.LaserEye, 12)
{
    private sealed class UnsafeLanding(Func<WPos, bool> unsafePosition) : ShapeDistance
    {
        public override float Distance(in WPos p) => unsafePosition(p) ? float.MinValue : float.MaxValue;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in ActiveKnockbacks(slot, actor))
        {
            if (IsImmune(slot, s.Activation))
                continue;
            hints.AddForbiddenZone(new UnsafeLanding(p =>
            {
                var dest = AwayFromSource(p, s.Origin, s.Distance);
                return DestinationUnsafe(slot, actor, dest);
            }), s.Activation);
        }
    }
}

class ExplosionSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExplosionSmall, 10);
class ExplosionLarge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExplosionLarge, 22);
class IceFloor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IceFloor, 20);

class SculptureAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.LissomSculpture, (uint)OID.ChiseledSculpture, (uint)OID.BeastlySculpture], 1);

class E12EdensPromiseStates : StateMachineBuilder
{
    public E12EdensPromiseStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Arenas>()
            .ActivateOnEnter<JumpPads>()
            .ActivateOnEnter<Maleficium>()
            .ActivateOnEnter<FormlessJudgment>()
            .ActivateOnEnter<JudgmentJolt>()
            .ActivateOnEnter<TemporaryCurrent>()
            .ActivateOnEnter<ConflagStrike>()
            .ActivateOnEnter<Ferostorm>()
            .ActivateOnEnter<Obliteration>()
            .ActivateOnEnter<EarthShaker>()
            .ActivateOnEnter<ClassicalSculptureStack>()
            .ActivateOnEnter<RapturousReachCone>()
            .ActivateOnEnter<RapturousReachKnockback>()
            .ActivateOnEnter<LaserEyeKnockback>()
            .ActivateOnEnter<ParadiseLost>()
            .ActivateOnEnter<EarthenFury>()
            .ActivateOnEnter<ExplosionSmall>()
            .ActivateOnEnter<ExplosionLarge>()
            .ActivateOnEnter<UnderTheWeight>()
            .ActivateOnEnter<DiamondDust>()
            .ActivateOnEnter<IceFloor>()
            .ActivateOnEnter<FrigidStone>()
            .ActivateOnEnter<FrigidStone2>()
            .ActivateOnEnter<IceFloe>()
            .ActivateOnEnter<SculptureAdds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 758, NameID = 9813)]
public class E12EdensPromise(WorldState ws, Actor primary) : BossModule(ws, primary, InitialCenter, MainBounds)
{
    public static readonly WPos InitialCenter = new(71.305f, -23.178f);
    public static readonly WPos FinalCenter = new(0, -75);
    public static readonly WPos Memory1Center = new(44.08f, 60.68f);
    public static readonly WPos Memory2Center = new(-44.08f, 60.68f);
    public static readonly WPos Memory3Center = new(-71.33f, -23.18f);

    public static readonly (OID Oid, WPos Center)[] MemoryPlatforms =
    [
        (OID.TreasuredMemory1, Memory1Center),
        (OID.TreasuredMemory2, Memory2Center),
        (OID.TreasuredMemory3, Memory3Center),
    ];

    public static readonly ArenaBoundsCircle MainBounds = new(20);
    public static readonly ArenaBoundsCircle MemoryBounds = new(15);
    public const float JumpPadRadius = 2f;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, Colors.Enemy);
        Arena.Actors(Enemies((uint)OID.TreasuredMemory1), Colors.Object);
        Arena.Actors(Enemies((uint)OID.TreasuredMemory2), Colors.Object);
        Arena.Actors(Enemies((uint)OID.TreasuredMemory3), Colors.Object);
        Arena.Actors(Enemies((uint)OID.LissomSculpture), Colors.Enemy);
        Arena.Actors(Enemies((uint)OID.ChiseledSculpture), Colors.Enemy);
        Arena.Actors(Enemies((uint)OID.BeastlySculpture), Colors.Enemy);
        Arena.Actors(Enemies((uint)OID.BombBoulder), Colors.Object);
        Arena.Actors(Enemies((uint)OID.TitanicBombBoulder), Colors.Object);
    }
}
