namespace BossMod.RealmReborn.Dungeon.D31AmdaporKeepHard.D312Boogyman;

public enum OID : uint
{
    Luminescence = 0xD64, // R1.000, x1 (spawn during fight)
    Boss = 0xD62, // R1.800, x1
}

public enum SID : uint
{
    Invisible = 616, // Boss->Boss, extra=0x0
    Irradiated = 617, // none->player, extra=0x0
}

class InvisibilityMechanic(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _invisibles = [];
    private BitMask _irradiated;

    private readonly HashSet<Actor> _luminescences = [];
    private WPos? _lastLuminescence;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_invisibles.Count > 0)
        {
            if (!_irradiated[slot])
            {
                if (_luminescences.Count > 0)
                {
                    if (_lastLuminescence.HasValue)
                    {
                        hints.GoalZones.Add(AIHints.GoalProximity(_lastLuminescence.Value, 0.25f, 10));
                    }
                }
                else
                {
                    foreach (var luminescence in _luminescences)
                    {
                        hints.GoalZones.Add(AIHints.GoalProximity(luminescence.Position, 0.5f, 10f));
                        hints.FindEnemy(luminescence)?.ForcePriority(1);
                    }
                }
            }
            else
            {
                hints.GoalZones.Add(AIHints.GoalSingleTarget(_invisibles.First(), 1f));
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Luminescence)
            _luminescences.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.Luminescence)
        {
            _lastLuminescence = actor.Position;
            _luminescences.Remove(actor);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Invisible)
        {
            _invisibles.Add(actor);
        }
        else if (status.ID == (uint)SID.Irradiated)
        {
            _irradiated.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Invisible && actor == Module.PrimaryActor)
        {
            _invisibles.Remove(actor);
        }
        else if (status.ID == (uint)SID.Irradiated)
        {
            _irradiated.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }
}

class D312BoogymanStates : StateMachineBuilder
{
    public D312BoogymanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<InvisibilityMechanic>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "awgil", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 29, NameID = 3274)]
public class D312Boogyman(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, -150), new ArenaBoundsSquare(20));
