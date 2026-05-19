namespace BossMod.Dawntrail.Alliance.A11Prishe;

class AuroralUppercut(BossModule module) : Components.Knockback(module, AID.AuroralUppercutAOE, true)
{
    private readonly CrystallineThorns? _thorns = module.FindComponent<CrystallineThorns>();
    private float _distance;
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_distance > 0)
            yield return new(Module.Center, _distance, _activation);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || (_thorns?.Dangerous(pos) ?? false);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var distance = (AID)spell.Action.ID switch
        {
            AID.AuroralUppercut1 => 12,
            AID.AuroralUppercut2 => 24,
            AID.AuroralUppercut3 => 36,
            _ => 0
        };
        if (distance > 0)
        {
            _distance = distance;
            _activation = Module.CastFinishAt(spell, 1.4f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _distance = 0;
            _activation = default;
        }
    }
}

class AuroralUppercutHint(BossModule module) : BossComponent(module)
{
    private int _activeKnockback; // 0 = inactive, 1 = short, 2 = medium, 3 = long
    private int _arenaPattern; // 1/2 = map layout

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x01)
            return;

        switch (state)
        {
            case 0x00020001:
                _arenaPattern = 1;
                break;
            case 0x02000100:
                _arenaPattern = 2;
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        _activeKnockback = (AID)spell.Action.ID switch
        {
            AID.AuroralUppercut1 => 1,
            AID.AuroralUppercut2 => 2,
            AID.AuroralUppercut3 => 3,
            _ => _activeKnockback
        };
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AuroralUppercutAOE)
            _activeKnockback = 0;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activeKnockback == 0 || _arenaPattern == 0)
            return;

        hints.Add(_activeKnockback switch
        {
            1 => "Short knockback: prepare near center lane.",
            2 => "Medium knockback: prepare in middle lane.",
            3 => "Long knockback: prepare close to inner corner.",
            _ => ""
        });
    }
}
