namespace BossMod.Dawntrail.Trial.T06Arkveld;

class WyvernsVengeance(BossModule module) : Components.Exaflare(module, 6)
{
    private readonly List<ulong> _casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WyvernsVengeance)
        {
            Lines.Add(new Components.Exaflare.Line
            {
                Next = caster.Position,
                Advance = default,
                Rotation = default,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1.6f,
                ExplosionsLeft = 2,
                MaxShownExplosions = 1
            });
            _casters.Add(caster.InstanceID);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.WyvernsVengeance1)
        {
            var idx = _casters.IndexOf(caster.InstanceID);
            if (idx < 0 || idx >= Lines.Count)
                return;

            var line = Lines[idx];
            AdvanceLine(line, spell.TargetXZ);
            if (line.ExplosionsLeft <= 0)
            {
                Lines.RemoveAt(idx);
                _casters.RemoveAt(idx);
            }
        }
    }
}

class WyvernsWealAOE(BossModule module) : Components.GroupedAOEs(module, [AID.WyvernsWeal1, AID.WyvernsWeal4], new AOEShapeRect(60, 3));

class WyvernsWealPulses(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect Shape = new(60, 3);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.WyvernsWeal2)
            _aoes.Add(new(Shape, caster.Position, caster.Rotation, WorldState.FutureTime(0.8f)));
    }

    public override void Update()
    {
        var now = WorldState.CurrentTime;
        _aoes.RemoveAll(a => a.Activation <= now);
    }
}

class WyvernsWealIrregularCastLane(BossModule module) : Components.GenericAOEs(module)
{
    private const float LenFront = 60;
    private const float LenBack = 20;
    private const float Narrow = 1;
    private const float Wide = 60;

    private AOEShapeRect? _shape;
    private WPos _origin;
    private Angle _rotation;
    private DateTime _until;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_shape == null || WorldState.CurrentTime >= _until)
            yield break;

        yield return new(_shape, _origin, _rotation, _until);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (caster != Module.PrimaryActor)
            return;

        var aid = (AID)spell.Action.ID;
        if (aid is not (AID.WyvernsWeal or AID.WyvernsWeal3))
            return;

        var dangerOnRight = aid == AID.WyvernsWeal;
        _rotation = spell.Rotation;
        var forward = _rotation.ToDirection();
        var left = new WDir(-forward.Z, forward.X);
        var lw = dangerOnRight ? Narrow : Wide;
        var rw = dangerOnRight ? Wide : Narrow;
        var halfW = (lw + rw) * 0.5f;
        var lateralShift = (lw - rw) * 0.5f;

        _origin = caster.Position + lateralShift * left - forward * LenBack;
        _shape = new AOEShapeRect(LenFront, halfW, LenBack);
        _until = Module.CastFinishAt(spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var aid = (AID)spell.Action.ID;
        if (caster == Module.PrimaryActor && aid is AID.WyvernsWeal or AID.WyvernsWeal3)
            _shape = null;
    }
}
