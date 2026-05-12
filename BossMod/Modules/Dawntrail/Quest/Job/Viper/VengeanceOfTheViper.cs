namespace BossMod.Dawntrail.Quest.Job.Viper.VengeanceOfTheViper;

public enum OID : uint
{
    Boss = 0x42A7,
    RightWing = 0x4423,
    LeftWing = 0x4424,
    BallOfFire = 0x42A8,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497,
    Teleport = 38737,
    BitingScratch1 = 38721,
    BitingScratch2 = 39552,
    FervidImpactVisual = 38716,
    FrigidPulseVisual = 38718,
    FervidImpact = 38715,
    FrigidPulse = 38717,
    StormkissedFlamesIn = 38795,
    StormkissedFlamesOut = 38719,
    FirestormCycleOut = 38794,
    FirestormCycleIn = 38720,
    SwoopingFrenzy1 = 38714,
    SwoopingFrenzy2 = 38728,
    SwoopingFrenzyTelegraph = 38730,
    SwoopingFrenzy3 = 38729,
    FervidPulseFirstCW1 = 38723,
    FervidPulseFirstCW2 = 38724,
    FervidPulseFirstCCW = 38725,
    FervidPulseRest = 38726,
    FervidPulseTelegraph = 38727,
    GaleripperVisual = 38731,
    Galeripper = 38732,
    CatchingChaos = 38733,
    CatchingChaosEnrage = 38734,
    GreatBallOfFire = 38722,
    Explosion = 38745,
    BrutalStroke = 38735,
    Razorwind = 38736
}

sealed class Razorwind(BossModule module) : Components.StackWithCastTargets(module, AID.Razorwind, 7f, 2, 2);
sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, 15f);
sealed class SwoopingFrenzy1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SwoopingFrenzy1, 15f);

sealed class SwoopingFrenzy2(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle Circle = new(15f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(3);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if ((id == (uint)AID.SwoopingFrenzy2 || id == (uint)AID.SwoopingFrenzyTelegraph) && !HasMatchingAOE(spell.LocXZ))
            _aoes.Add(new(Circle, spell.LocXZ, default, Module.CastFinishAt(spell, 1.5f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.SwoopingFrenzy2 or (uint)AID.SwoopingFrenzy3)
            _aoes.RemoveAt(0);
    }

    private bool HasMatchingAOE(WPos location) => _aoes.Any(a => a.Origin.AlmostEqual(location, 1f));
}

sealed class BrutalStroke(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrutalStroke, 25f);
sealed class CatchingChaos(BossModule module) : Components.RaidwideCast(module, AID.CatchingChaos);
sealed class Galeripper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Galeripper, new AOEShapeCone(60f, 45f.Degrees()));
sealed class BitingScratch(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BitingScratch1, (uint)AID.BitingScratch2], new AOEShapeCone(40f, 45f.Degrees()));
sealed class FervidImpact(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FervidImpact, 12f);
sealed class FrigidPulse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FrigidPulse, new AOEShapeDonut(12f, 60f));

sealed class FirestormCycle(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(12f), new AOEShapeDonut(12f, 60f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FirestormCycleOut)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count == 0)
            return;
        var order = spell.Action.ID switch
        {
            (uint)AID.FirestormCycleOut => 0,
            (uint)AID.FirestormCycleIn => 1,
            _ => -1
        };
        AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2.4f));
    }
}

sealed class StormkissedFlames(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeDonut(12f, 60f), new AOEShapeCircle(12f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StormkissedFlamesIn)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count == 0)
            return;
        var order = spell.Action.ID switch
        {
            (uint)AID.StormkissedFlamesIn => 0,
            (uint)AID.StormkissedFlamesOut => 1,
            _ => -1
        };
        AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2.4f));
    }
}

sealed class FervidPulse(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCross Cross = new(50f, 7f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FervidPulseFirstCCW:
                AddSequence(22.5f.Degrees());
                break;
            case (uint)AID.FervidPulseFirstCW1:
            case (uint)AID.FervidPulseFirstCW2:
                AddSequence((-22.5f).Degrees());
                break;
        }

        void AddSequence(Angle angle) => Sequences.Add(new(Cross, spell.LocXZ, spell.Rotation, angle, Module.CastFinishAt(spell), 2.9f, 5));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FervidPulseFirstCCW or (uint)AID.FervidPulseFirstCW1 or (uint)AID.FervidPulseFirstCW2 or (uint)AID.FervidPulseRest)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

sealed class VengeanceOfTheViperStates : StateMachineBuilder
{
    public VengeanceOfTheViperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BitingScratch>()
            .ActivateOnEnter<Razorwind>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<BrutalStroke>()
            .ActivateOnEnter<SwoopingFrenzy1>()
            .ActivateOnEnter<SwoopingFrenzy2>()
            .ActivateOnEnter<CatchingChaos>()
            .ActivateOnEnter<Galeripper>()
            .ActivateOnEnter<FrigidPulse>()
            .ActivateOnEnter<FervidImpact>()
            .ActivateOnEnter<StormkissedFlames>()
            .ActivateOnEnter<FirestormCycle>()
            .ActivateOnEnter<FervidPulse>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70389, NameID = 12829)]
public sealed class VengeanceOfTheViper(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, ArenaBounds)
{
    private static readonly WPos ArenaCenter = new(-402f, 738f);
    private static readonly ArenaBoundsCircle ArenaBounds = new(19.5f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.RightWing), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.LeftWing), ArenaColor.Enemy);
    }
}
