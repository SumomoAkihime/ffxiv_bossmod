namespace BossMod.Stormblood.Ultimate.UWU;

class P3Upheaval(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Upheaval, 24, true)
{
    private ArcList SafeAngles = new(default, 0);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action.ID == WatchedAction)
        {
            SafeAngles = new(spell.LocXZ, 29);

            SafeAngles.ForbidInverseCircle(Arena.Center, 16);

            foreach (var boulder in Module.Enemies(OID.BombBoulder))
                SafeAngles.ForbidCircle(boulder.Position, 6.3f);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BombBoulder)
            SafeAngles.ForbidCircle(actor.Position, 6.3f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var source in ActiveKnockbacks(slot, actor))
        {
            List<ShapeDistance> destinations = [];
            foreach (var (min, max) in SafeAngles.Allowed(1.Degrees()))
                destinations.Add(new SDPrecisePosition(SafeAngles.Center + ((max + min) * 0.5f).ToDirection() * 4,
                    new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f));
            if (destinations.Count > 0)
                hints.AddForbiddenZone(new SDIntersection([.. destinations]), source.Activation);
        }
    }
}

class P3Burst(BossModule module) : Components.GenericAOEs(module, (uint)AID.Burst)
{
    private readonly IReadOnlyList<Actor> _bombs = module.Enemies(OID.BombBoulder);
    private readonly Dictionary<ulong, DateTime?> _bombActivation = [];

    private static readonly AOEShape _shape = new AOEShapeCircle(6.3f);

    private readonly List<AOEInstance> _portedAOEView = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _portedAOEView.Clear();
        foreach (var b in _bombs)
        {
            var activation = _bombActivation.GetValueOrDefault(b.InstanceID);
            if (activation != null)
                _portedAOEView.Add(new(_shape, b.Position, b.Rotation, Module.CastFinishAt(b.CastInfo, 0, activation.Value)));
        }
        return CollectionsMarshal.AsSpan(_portedAOEView);
    }

    public override void Update()
    {
        foreach (var b in _bombs.Where(b => !_bombActivation.ContainsKey(b.InstanceID)))
            _bombActivation[b.InstanceID] = WorldState.FutureTime(6.5f);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _bombActivation[caster.InstanceID] = null;
    }
}
