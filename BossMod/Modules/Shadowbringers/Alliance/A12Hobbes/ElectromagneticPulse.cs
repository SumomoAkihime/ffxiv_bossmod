namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

class ElectromagneticPulse(BossModule module) : Components.GenericAOEs(module, (uint)AID.ElectromagneticPulse)
{
    enum Pattern
    {
        None,
        Odd,
        Even
    }
    private Pattern _pattern;

    public DateTime Activation { get; private set; }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var center = new WPos(-831, -225);
        var angleToCenter = 120.Degrees();
        var startPos = _pattern switch
        {
            Pattern.Odd => center + angleToCenter.ToDirection() * 20,
            Pattern.Even => center + angleToCenter.ToDirection() * 15,
            _ => default
        };
        if (startPos == default)
            return [];

        AOEInstance[] aoes = new AOEInstance[5];
        for (var i = 0; i < 5; i++)
        {
            aoes[i] = new(new AOEShapeRect(5, 20), startPos, -60.Degrees(), activation: Activation);
            startPos += 300.Degrees().ToDirection() * 10;
        }
        return aoes;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 3)
        {
            if (state == 0x00400020)
            {
                _pattern = Pattern.Even;
                Activation = WorldState.FutureTime(4.2f);
            }
            else if (state == 0x02000100)
            {
                _pattern = Pattern.Odd;
                Activation = WorldState.FutureTime(4.2f);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            NumCasts++;
            _pattern = Pattern.None;
        }
    }
}
