namespace BossMod.Dawntrail.Trial.T02ZoraalJaP2;

class SmitingCircuitHalfCircuitDonut(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SmitingCircuitDonut, (uint)AID.HalfCircuitDonut], new AOEShapeDonut(10, 30));
class SmitingCircuitHalfCircuitCircle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SmitingCircuitCircle, (uint)AID.HalfCircuitCircle], 10);
class BitterReaping(BossModule module) : Components.SingleTargetCast(module, AID.BitterReaping);
class FireIII(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, AID.FireIII, 6, 5.1f);

class DawnOfAnAgeActualize(BossModule module) : Components.CastHint(module, null, "Raidwide")
{
    private readonly List<DateTime> _activations = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DawnOfAnAge or AID.Actualize)
        {
            _activations.Add(Module.CastFinishAt(spell));
            _activations.Sort();
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_activations.Count > 0)
            hints.Add(Hint);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DawnOfAnAge or AID.Actualize)
        {
            ++NumCasts;
            if (_activations.Count > 0)
                _activations.RemoveAt(0);
        }
    }
}

abstract class HalfRect(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(60, 30));

class HalfFull(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ChasmOfVollok _chasm = module.FindComponent<ChasmOfVollok>()!;
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect Shape = new(60, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HalfFull)
            _aoes.Add(new(Shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void Update()
    {
        if (_chasm.AOEs.Count != 0)
            _aoes.Clear();
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HalfFull)
            _aoes.Clear();
    }
}

class HalfCircuitRect(BossModule module) : HalfRect(module, (uint)AID.HalfCircuitRect);

class DutysEdge(BossModule module) : Components.SimpleLineStack(
    module,
    4,
    100,
    AID.DutysEdgeMarker,
    AID.DutysEdge,
    5.4f);

class T02ZoraalJaP2States : StateMachineBuilder
{
    public T02ZoraalJaP2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DawnOfAnAgeArenaChange>()
            .ActivateOnEnter<SmitingCircuitHalfCircuitDonut>()
            .ActivateOnEnter<SmitingCircuitHalfCircuitCircle>()
            .ActivateOnEnter<DawnOfAnAgeActualize>()
            .ActivateOnEnter<BitterReaping>()
            .ActivateOnEnter<ChasmOfVollok>()
            .ActivateOnEnter<ForgedTrack>()
            .ActivateOnEnter<HalfFull>()
            .ActivateOnEnter<HalfCircuitRect>()
            .ActivateOnEnter<FireIII>()
            .ActivateOnEnter<DutysEdge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 995, NameID = 12882)]
public class T02ZoraalJaP2(WorldState ws, Actor primary) : T02ZoraalJa.ZoraalJa(ws, primary);
