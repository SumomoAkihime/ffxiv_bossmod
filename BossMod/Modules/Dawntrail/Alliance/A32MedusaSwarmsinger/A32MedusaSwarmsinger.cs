namespace BossMod.Dawntrail.Alliance.A32Medusa;

public enum OID : uint
{
    Boss = 0x4DA6, // R1.800, x1
    Acrolith1 = 0x4DA4, // R3.000, x3
    LamiaRover1 = 0x4DA3, // R1.100, x6
    Acrolith2 = 0x4DB5, // R3.000, x3
    LamiaRover2 = 0x4DB4, // R1.100, x2
    LamiaJaeger = 0x4DA5, // R1.100, x1 (spawn during fight)
    QutrubForayer = 0x4DA7, // R1.200, x2
    LamiaNo2 = 0x4DA8, // R1.650, x0 (spawn during fight)
    NemeanLion = 0x4DA9, // R4.400, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x2 (spawn during fight), Helper type
    AssaultBhoot = 0x4DAA, // R1.170, x0 (spawn during fight)
    MedusaSwarmsinger = 0x4DAB, // R2.250, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 870, // 4DB4/Boss/4DA7/4DA9/4DAB->player/4DAF/4DAC, no cast, single-target
    AcrolithAuto = 872, // 4DB5/4DAA->player/4DB1/4DB2/4DAD, no cast, single-target
    LamiaAuto = 873, // 4DA5/4DA8->player, no cast, single-target
    Earthshatter = 50085, // 4DA4->self, 4.0s cast, range 8 circle
    TranscendentShot = 50086, // 4DA5->self, 5.0s cast, range 60 width 5 rect
    LeapingCleaveVisual = 50087, // Boss->location, 3.5+1.5s cast, single-target
    Unk1 = 50639, // Helper->4D66, 5.0s cast, single-target
    LeapingCleave = 50481, // Helper->location, 5.0s cast, range 40 circle
    FeralLungeCast = 50088, // 4DA7->location, 2.0+2.0s cast, single-target
    FeralLunge = 50482, // Helper->location, 4.0s cast, range 10 circle
    WhirlingSlash = 50659, // Boss->self, 3.0s cast, range 6 circle
    Perdition = 50094, // 4DAA->self, 4.0s cast, range 9 circle
    Tourbillion = 50091, // 4DA9->self, 5.0s cast, range 40 width 50 rect
    PinningShotCast = 50089, // 4DA8->self, 7.0s cast, single-target
    PinningShot = 50090, // Helper->player, 7.0s cast, range 13 circle
    FulminationKhalkeos = 50092, // 4DA9->self, 4.0s cast, range 70 circle
    DanceToDustVisual = 50095, // 4DAB->self, 5.0s cast, single-target
    DanceToDustFirst = 50096, // Helper->self, 5.0s cast, range 7 circle
    DanceToDustRest = 50097, // Helper->self, no cast, range 7 circle
    RightShadowSlash = 50098, // 4DAB->self, 5.0s cast, range 60 180-degree cone
    LeftShadowSlash = 50099, // 4DAB->self, 5.0s cast, range 60 180-degree cone
    BellowingGrunt = 50103, // 4DAB->self, 4.0s cast, range 60 circle
    DisregardRaidwide = 50100, // 4DAB->self, 4.0s cast, range 60 circle
    DisregardRect = 50101, // Helper->self, 4.0s cast, range 55 width 10 rect
    Petrifaction = 50102, // 4DAB->self, 5.0s cast, range 60 circle
    DivineArrowCone = 50130, // Helper->self, 1.0s cast, range 45 90-degree cone
    DivineArrowClose2 = 50131, // Helper->self, no cast, range 10 circle
    DivineArrowMid2 = 50132, // Helper->self, no cast, range 10-23 donut
    DivineArrowFar2 = 50133, // Helper->self, no cast, range 23-36 donut
    DivineArrowClose = 50134, // Helper->self, 13.5s cast, range 10 circle
    DivineArrowMid = 50135, // Helper->self, 11.5s cast, range 10-23 donut
    DivineArrowFar = 50136, // Helper->self, 9.5s cast, range 23-36 donut
    DivineArrowLines = 50137, // Helper->self, 3.5s cast, range 60 width 10 rect
    DivineArrowLines2 = 50138, // Helper->self, 5.5s cast, range 60 width 10 rect
    DivineArrowSpamCone = 50478, // Helper->self, no cast, range 45 90-degree cone
}

public enum IconID : uint
{
    DivineArrowN = 691, // Boss->self
    DivineArrowS = 692, // Boss->self
    DivineArrowE = 693, // Boss->self
    DivineArrowW = 694, // Boss->self
    DivineArrowNR = 695, // Boss->self
    DivineArrowSR = 696, // Boss->self
    DivineArrowER = 697, // Boss->self
    DivineArrowWR = 698, // Boss->self
}

class Earthshatter(BossModule module) : Components.StandardAOEs(module, AID.Earthshatter, 8);
class TranscendentShot(BossModule module) : Components.StandardAOEs(module, AID.TranscendentShot, new AOEShapeRect(60, 2.5f), maxCasts: 4);
class LeapingCleave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.LeapingCleave, 22)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
            if (!IsImmune(slot, src.Activation))
            {
                var center = Arena.Center;
                hints.AddForbiddenZone(p =>
                {
                    var kb = (p - center).Normalized() * 22;
                    return !(p + kb).InRect(center, default(Angle), 20, 20, 25);
                }, src.Activation);
            }
    }
}
class FeralLunge(BossModule module) : Components.StandardAOEs(module, AID.FeralLunge, 10);
class WhirlingSlash(BossModule module) : Components.StandardAOEs(module, AID.WhirlingSlash, 6);
class Perdition(BossModule module) : Components.StandardAOEs(module, AID.Perdition, 9);
class Tourbillion(BossModule module) : Components.StandardAOEs(module, AID.Tourbillion, new AOEShapeRect(40, 25));
class PinningShot(BossModule module) : Components.BaitAwayCast(module, AID.PinningShot, new AOEShapeCircle(13), centerAtTarget: true);
class FulminationKhalkeos(BossModule module) : Components.RaidwideCast(module, AID.FulminationKhalkeos);
class DanceToDust(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(7))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DanceToDustFirst)
            Lines.Add(new()
            {
                Next = caster.Position,
                Advance = caster.Rotation.ToDirection() * 8,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2,
                ExplosionsLeft = caster.Rotation.AlmostEqual(default, 0.1f) || caster.Rotation.AlmostEqual(180.Degrees(), 0.1f) ? 2 : 3,
                MaxShownExplosions = 3
            });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DanceToDustFirst or AID.DanceToDustRest)
        {
            var ix = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 1));
            if (ix >= 0)
                AdvanceLine(Lines[ix], caster.Position);
        }
    }
}
class ShadowSlash(BossModule module) : Components.GroupedAOEs(module, [AID.RightShadowSlash, AID.LeftShadowSlash], new AOEShapeCone(60, 90.Degrees()));
class BellowingGrunt(BossModule module) : Components.RaidwideCast(module, AID.BellowingGrunt);
class Disregard(BossModule module) : Components.RaidwideCast(module, AID.DisregardRaidwide);
class DisregardRect(BossModule module) : Components.StandardAOEs(module, AID.DisregardRect, new AOEShapeRect(55, 5), highlightImminent: true);
class Petrifaction(BossModule module) : Components.CastGaze(module, AID.Petrifaction);
class DivineArrowCone(BossModule module) : Components.GenericRotatingAOE(module)
{
    static readonly AOEShapeCone _cone = new(45, 45.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        Angle starting;
        Angle advance;

        switch ((IconID)iconID)
        {
            case IconID.DivineArrowN:
                starting = 180.Degrees();
                advance = -20.Degrees();
                break;
            case IconID.DivineArrowS:
                starting = default;
                advance = -20.Degrees();
                break;
            case IconID.DivineArrowE:
                starting = 90.Degrees();
                advance = -20.Degrees();
                break;
            case IconID.DivineArrowW:
                starting = -90.Degrees();
                advance = -20.Degrees();
                break;
            case IconID.DivineArrowNR:
                starting = 180.Degrees();
                advance = 20.Degrees();
                break;
            case IconID.DivineArrowSR:
                starting = default;
                advance = 20.Degrees();
                break;
            case IconID.DivineArrowER:
                starting = 90.Degrees();
                advance = 20.Degrees();
                break;
            case IconID.DivineArrowWR:
                starting = -90.Degrees();
                advance = 20.Degrees();
                break;
            default:
                return;
        }

        Sequences.Add(new(_cone, actor.Position, starting, advance, WorldState.FutureTime(13.3f), 0.67f, 18, MaxShownAOEs: 8));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DivineArrowCone or AID.DivineArrowSpamCone)
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
    }
}

class DivineArrowCircles(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];
    static readonly AOEShapeCircle _closeShape = new(10);
    static readonly AOEShapeDonut _midShape = new(10, 23);
    static readonly AOEShapeDonut _farShape = new(23, 36);
    static readonly HashSet<AID> _watched = [AID.DivineArrowClose, AID.DivineArrowClose2, AID.DivineArrowMid, AID.DivineArrowMid2, AID.DivineArrowFar, AID.DivineArrowFar2];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = null;
        switch ((AID)spell.Action.ID)
        {
            case AID.DivineArrowClose:
            case AID.DivineArrowClose2:
                shape = _closeShape;
                break;
            case AID.DivineArrowMid:
            case AID.DivineArrowMid2:
                shape = _midShape;
                break;
            case AID.DivineArrowFar:
            case AID.DivineArrowFar2:
                shape = _farShape;
                break;
        }

        if (shape != null)
            _predicted.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_watched.Contains((AID)spell.Action.ID))
        {
            ++NumCasts;
            _predicted.RemoveAll(p => p.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}

class DivineArrowLines(BossModule module) : Components.GroupedAOEs(module, [AID.DivineArrowLines, AID.DivineArrowLines2], new AOEShapeRect(60, 5), maxCasts: 6);

class A32MedusaSwarmsingerStates : StateMachineBuilder
{
    public A32MedusaSwarmsingerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Earthshatter>()
            .ActivateOnEnter<TranscendentShot>()
            .ActivateOnEnter<LeapingCleave>()
            .ActivateOnEnter<FeralLunge>()
            .ActivateOnEnter<WhirlingSlash>()
            .ActivateOnEnter<Perdition>()
            .ActivateOnEnter<Tourbillion>()
            .ActivateOnEnter<PinningShot>()
            .ActivateOnEnter<FulminationKhalkeos>()
            .ActivateOnEnter<DanceToDust>()
            .ActivateOnEnter<ShadowSlash>()
            .ActivateOnEnter<BellowingGrunt>()
            .ActivateOnEnter<Disregard>()
            .ActivateOnEnter<DisregardRect>()
            .ActivateOnEnter<Petrifaction>()
            .ActivateOnEnter<DivineArrowCone>()
            .ActivateOnEnter<DivineArrowCircles>()
            .ActivateOnEnter<DivineArrowLines>()
            .Raw.Update = () => module.Enemies(OID.MedusaSwarmsinger).Any(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117, NameID = 14834)]
public class A32MedusaSwarmsinger(WorldState ws, Actor primary) : BossModule(ws, primary, new(721, 720), new ArenaBoundsRect(25, 20))
{
    protected override bool CheckPull() => PrimaryActor.InCombat;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(a => !a.IsAlly), ArenaColor.Enemy);
    }
}
