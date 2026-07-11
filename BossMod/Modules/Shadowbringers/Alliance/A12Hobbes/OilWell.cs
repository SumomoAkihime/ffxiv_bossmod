namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

class OilWell(BossModule module) : Components.GenericAOEs(module, (uint)AID.OilWell)
{
    public DateTime Activation { get; private set; }
    private bool _inverted;
    private static readonly List<WPos> Platforms = MakePlatforms();

    private static List<WPos> MakePlatforms()
    {
        var centers = CurveApprox.Rect(new(8, 0), new(0, 8));
        WPos platformCenter = new(-779, -225);
        return [.. centers.Select(c => platformCenter + c.Rotate(-120.Degrees()))];
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => Activation == default ? [] : Platforms.Select(c => new AOEInstance(new AOEShapeCircle(6), c, activation: Activation, risky: !_inverted, color: _inverted ? Colors.SafeFromAOE : Colors.AOE)).ToArray();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Activation != default && Relevant(actor))
        {
            var inv = _inverted;
            hints.AddForbiddenZone(p => inv ? !OnPlatform(p) : OnPlatform(p), Activation);
        }
    }

    bool Relevant(Actor a) => a.Position.InCircle(new(-779, -225), 20);
    static bool OnPlatform(WPos a) => Platforms.Any(p => p.InCircle(a, 6));

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (Activation != default && _inverted && Relevant(actor) && !OnPlatform(actor.Position))
            hints.Add("Go to platform!");
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 6)
        {
            switch (state)
            {
                case 0x01000080:
                    Activation = WorldState.FutureTime(4.2f);
                    _inverted = false;
                    break;
                case 0x00200010:
                    Activation = WorldState.FutureTime(4.2f);
                    _inverted = true;
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            NumCasts++;
            Activation = default;
        }
    }
}
