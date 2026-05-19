namespace BossMod.Heavensward.Quest.MSQ.CloseEncountersOfTheVIthKind;

public enum OID : uint
{
    Boss = 0xF1C,
    Puddle = 0x1E88F5,
    TerminusEst = 0xF5D,
}

public enum AID : uint
{
    HandOfTheEmpire = 4000,
    TerminusEstBoss = 4005,
    TerminusEstAOE = 3825,
}

class HandOfTheEmpire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HandOfTheEmpire, 2f);
class Voidzone(BossModule module) : Components.Voidzone(module, 8f, m => m.Enemies((uint)OID.Puddle));

class TerminusEst(BossModule module) : Components.GenericAOEs(module)
{
    private bool _active;
    private static readonly AOEShapeRect Rect = new(40f, 2f);

    private List<Actor> AliveTerminus()
    {
        var enemies = Module.Enemies((uint)OID.TerminusEst);
        var res = new List<Actor>(enemies.Count);
        foreach (var e in enemies)
            if (!e.IsDead)
                res.Add(e);
        return res;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_active)
            yield break;
        foreach (var t in AliveTerminus())
            yield return new(Rect, t.Position, t.Rotation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(AliveTerminus(), ArenaColor.Danger, true);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TerminusEstBoss)
            _active = true;
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.TerminusEst)
            _active = false;
    }
}

class RegulaVanHydrusStates : StateMachineBuilder
{
    public RegulaVanHydrusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TerminusEst>()
            .ActivateOnEnter<Voidzone>()
            .ActivateOnEnter<HandOfTheEmpire>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(RegulaVanHydrusStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67203, NameID = 3818)]
public class RegulaVanHydrus(WorldState ws, Actor primary) : BossModule(ws, primary, new(252.75f, 553f), new ArenaBoundsCircle(19.5f));
