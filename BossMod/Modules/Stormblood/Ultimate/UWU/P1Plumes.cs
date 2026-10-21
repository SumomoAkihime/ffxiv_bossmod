namespace BossMod.Stormblood.Ultimate.UWU;

// Satin plumes apply raidwide sleep after about 21 seconds.
class P1Plumes(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor Plume, DateTime Spawn)> _plumes = [];

    public bool Active => _plumes.Any(p => p.Plume.IsTargetable);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_plumes.Select(p => p.Plume), Colors.Enemy);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.TargetID == actor.InstanceID)
            return;

        var destination = Module.PrimaryActor.Position + new WDir(0, 3);
        foreach (var (plume, spawn) in _plumes)
        {
            if (hints.FindEnemy(plume) is not { } enemy)
                continue;

            enemy.ForbidDOTs = true;
            enemy.DesiredPosition = destination;

            if (plume.OID == (uint)OID.SpinyPlume)
            {
                var stacks = WorldState.Actors.Find(plume.TargetID)?.FindStatus((uint)SID.ThermalLow, DateTime.MaxValue)?.Extra ?? 0;
                if (plume.Tether.ID == 0)
                {
                    if (assignment != PartyRolesConfig.Assignment.OT)
                        enemy.Priority = AIHints.Enemy.PriorityForbidden;
                }
                else
                {
                    enemy.Priority = 0;
                }

                if (plume.PendingHPRatio < 0.15f)
                    enemy.Priority = stacks == 2 ? 1 : AIHints.Enemy.PriorityForbidden;
            }

            if (plume.OID == (uint)OID.SatinPlume)
            {
                enemy.Priority = 0;
                var deadline = spawn.AddSeconds(20.7f);
                if (deadline < WorldState.FutureTime(5))
                    enemy.Priority = 1;
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.SpinyPlume or OID.SatinPlume or OID.RazorPlume)
            _plumes.Add((actor, WorldState.CurrentTime));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        _plumes.RemoveAll(p => p.Plume == actor);
    }
}

class P1Friction(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Friction, 5, 8)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.Enemies((uint)OID.SpinyShield).FirstOrDefault() is not { } bubble)
            return;

        var toCenter = (Arena.Center - bubble.Position).Normalized();
        var target = bubble.Position + toCenter * 6;

        hints.AddForbiddenZone(new SDInvertedCircle(target, 5), DateTime.MaxValue);
        if (IsStackTarget(actor))
            hints.AddForbiddenZone(new SDInvertedCircle(target, 2), Stacks[0].Activation);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class P1SpinyShield(BossModule module) : BossComponent(module)
{
    public BitMask Forbidden;
    public DateTime Deadline = DateTime.MaxValue;

    private int _numFriction;
    private WPos? _pending;
    private Actor? _bubble;
    private DateTime _bubbleExpire;

    private WPos? Bubble => _bubble?.Position ?? _pending;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Bubble is not { } position || Forbidden[pcSlot])
            return;

        if (MiniArena.Config.ShowOutlinesAndShadows)
            Arena.AddCircleUnfilled(position, 6, 0xFF000000, 2);
        Arena.AddCircleUnfilled(position, 6, Colors.Safe);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Bubble is { } position && Forbidden[pcSlot])
            Arena.ZoneCircle(position, 6, Colors.AOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Bubble is not { } position)
            return;

        var safe = Forbidden[slot] != actor.Position.InCircle(position, 6);
        if (!safe)
            hints.Add(Forbidden[slot] ? "GTFO from bubble!" : "Go to bubble!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Bubble is not { } position)
            return;

        hints.AddForbiddenZone(Forbidden[slot] ? new SDCircle(position, 6) : new SDInvertedCircle(position, 6), Deadline);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Gigastorm:
                _pending = caster.Position;
                Forbidden = Raid.WithSlot(false, true, true).WhereActor(a => a.FindStatus((uint)SID.ThermalLow, DateTime.MaxValue) != null).Mask();
                break;
            case AID.MistralShriek:
                Forbidden.Reset();
                break;
            case AID.Friction:
                ++_numFriction;
                if (_numFriction == 2)
                {
                    var assignments = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(Raid);
                    if (assignments.Length > 0)
                        Forbidden.Clear(assignments[(int)PartyRolesConfig.Assignment.M1]);
                }
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.Friction)
            return;

        Forbidden.Reset();
        Deadline = Module.CastFinishAt(spell);

        var assignments = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(Raid);
        if (assignments.Length == 0)
        {
            ReportError("Don't know who should leave for Friction; bubble assignment is disabled.");
            return;
        }

        if (_numFriction == 0)
        {
            Forbidden.Set(assignments[(int)PartyRolesConfig.Assignment.MT]);
            Forbidden.Set(assignments[(int)PartyRolesConfig.Assignment.M1]);
            Forbidden.Set(assignments[(int)PartyRolesConfig.Assignment.M2]);
        }
        else if (_numFriction == 1)
        {
            Forbidden = new(0xff);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID != OID.SpinyShield)
            return;

        _bubble = actor;
        _pending = null;
        _bubbleExpire = WorldState.FutureTime(22.9f);
    }

    public override void Update()
    {
        if (_bubble?.EventState == 7)
            _bubble = null;

        if (_bubbleExpire <= WorldState.CurrentTime)
            return;

        var assignments = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(Raid);
        if (assignments.Length > 0 && _bubbleExpire < WorldState.FutureTime(3))
            Forbidden.Clear(assignments[(int)PartyRolesConfig.Assignment.M2]);
    }
}
