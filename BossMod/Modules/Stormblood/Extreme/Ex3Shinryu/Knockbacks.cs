namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

class TidalWave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TidalWave, 35, kind: Kind.DirForward)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<ArenaChanges>() is { } arena && arena.Broken(pos);
}

class AerialBlast(BossModule module) : Components.GenericKnockback(module, (uint)AID.AerialBlast)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<ArenaChanges>() is { } arena && arena.Broken(pos);

    private WPos _origin;
    private DateTime _activation;
    private DateTime _positionDeadline;
    private int _smallPushes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AerialBlast)
        {
            _origin = caster.Position;
            _activation = Module.CastFinishAt(spell, 1.6f);
            _positionDeadline = Module.CastFinishAt(spell, -6.3f);
            _smallPushes = 0;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.SuperCyclone && _activation != default)
            _positionDeadline = ++_smallPushes < 3 ? WorldState.FutureTime(2.1f) : _activation;
    }

    private readonly List<Knockback> _portedKnockbackView = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        _portedKnockbackView.Clear();
        if (_activation != default && WorldState.CurrentTime <= _activation)
            _portedKnockbackView.Add(new(_origin, 15, _activation));
        return CollectionsMarshal.AsSpan(_portedKnockbackView);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation == default || WorldState.CurrentTime > _activation.AddSeconds(0.5f) || IsImmune(slot, _activation))
            return;
        hints.AddForbiddenZone(new SDInvertedCircle(_origin, 10), _positionDeadline);
        if (Module.FindComponent<ArenaChanges>() is not { } arena)
            return;
        var origin = _origin;
        hints.AddForbiddenZone(new BrokenLanding(arena, origin), _positionDeadline);
    }
}


sealed class BrokenLanding(ArenaChanges arena, WPos origin) : ShapeDistance
{
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;
    public override bool Contains(in WPos p) => arena.Broken(Components.GenericKnockback.AwayFromSource(p, origin, 15));
}
