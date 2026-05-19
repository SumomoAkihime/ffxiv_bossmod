namespace BossMod.Dawntrail.Alliance.A12Fafnir;

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut _shape = new(30, 35);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DarkMatterBlast && Arena.Bounds is not ArenaBoundsCircle)
            _aoes.Add(new(_shape, Arena.Center, default, Module.CastFinishAt(spell, 1.1f)));
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x22 && state == 0x00020001u)
        {
            Arena.Bounds = A12Fafnir.DefaultBounds;
            _aoes.Clear();
        }
    }
}

class DragonBreathArenaChange : BossComponent
{
    private DateTime _restoreStart;

    public DragonBreathArenaChange(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID != OID.DragonBreath)
            return;

        if (state == 0x00010002u)
        {
            Arena.Bounds = A12Fafnir.FireArena;
            _restoreStart = default;
        }
        else if (state == 0x00040008u)
        {
            _restoreStart = WorldState.CurrentTime;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x22 && state == 0x00020001u)
        {
            Arena.Bounds = A12Fafnir.DefaultBounds;
            _restoreStart = default;
        }
    }

    public override void Update()
    {
        if (_restoreStart != default && (WorldState.CurrentTime - _restoreStart).TotalSeconds >= 12)
        {
            Arena.Bounds = A12Fafnir.DefaultBounds;
            _restoreStart = default;
        }
    }
}
