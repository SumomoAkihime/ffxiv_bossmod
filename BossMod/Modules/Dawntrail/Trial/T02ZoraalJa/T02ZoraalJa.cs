namespace BossMod.Dawntrail.Trial.T02ZoraalJa;

class SoulOverflowCalamitysEdge(BossModule module) : Components.CastHint(module, null, "Raidwide")
{
    private readonly List<DateTime> _activations = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SoulOverflow1 or AID.SoulOverflow2 or AID.CalamitysEdge)
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

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activations.Count > 0)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), _activations[0]);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SoulOverflow1 or AID.SoulOverflow2 or AID.CalamitysEdge)
        {
            ++NumCasts;
            if (_activations.Count > 0)
                _activations.RemoveAt(0);
        }
    }
}

class PatricidalPique(BossModule module) : Components.SingleTargetCast(module, AID.PatricidalPique);
class Burst(BossModule module) : Components.StandardAOEs(module, AID.Burst, 8);

class VorpalTrail1(BossModule module) : Components.ChargeAOEs(module, AID.VorpalTrail1, 2);
class VorpalTrail2(BossModule module) : Components.ChargeAOEs(module, AID.VorpalTrail2, 2);

class T02ZoraalJaStates : StateMachineBuilder
{
    public T02ZoraalJaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SoulOverflowCalamitysEdge>()
            .ActivateOnEnter<DoubleEdgedSwords>()
            .ActivateOnEnter<PatricidalPique>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<VorpalTrail1>()
            .ActivateOnEnter<VorpalTrail2>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed || !module.PrimaryActor.IsTargetable;
    }
}

public abstract class ZoraalJa(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly Angle ArenaRotation = 45.Degrees();
    public static readonly WPos ArenaCenter = new(100, 100);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20);
    public static readonly ArenaBoundsSquare SmallBounds = new(10);
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 995, NameID = 12881)]
public class T02ZoraalJa(WorldState ws, Actor primary) : ZoraalJa(ws, primary);
