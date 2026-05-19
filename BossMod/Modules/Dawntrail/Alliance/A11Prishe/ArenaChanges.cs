namespace BossMod.Dawntrail.Alliance.A11Prishe;

class ArenaChanges(BossModule module) : Components.GenericAOEs(module, AID.Thornbite)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCustom TelegraphENVC00020001 = BuildTelegraph(false);
    private static readonly AOEShapeCustom TelegraphENVC02000100 = BuildTelegraph(true);
    public static readonly ArenaBoundsCustom ArenaENVC00020001 = BuildArena(false);
    public static readonly ArenaBoundsCustom ArenaENVC02000100 = BuildArena(true);

    public bool Active => _aoes.Count != 0 || Arena.Bounds != A11Prishe.DefaultBounds;
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x01)
            return;

        switch (state)
        {
            case 0x00020001:
                SetAOE(TelegraphENVC00020001);
                break;
            case 0x02000100:
                SetAOE(TelegraphENVC02000100);
                break;
            case 0x00200010:
                SetArena(ArenaENVC00020001);
                break;
            case 0x08000400:
                SetArena(ArenaENVC02000100);
                break;
            case 0x00080004:
            case 0x00800004:
                Arena.Bounds = A11Prishe.DefaultBounds;
                Arena.Center = A11Prishe.ArenaCenter;
                _aoes.Clear();
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }
    public override void AddHints(int slot, Actor actor, TextHints hints) { }

    private void SetAOE(AOEShapeCustom shape)
    {
        _aoes.Clear();
        _aoes.Add(new(shape, Arena.Center, default, WorldState.FutureTime(5f)));
    }

    private void SetArena(ArenaBoundsCustom bounds)
    {
        Arena.Bounds = bounds;
        Arena.Center = A11Prishe.ArenaCenter;
        _aoes.Clear();
    }

    private static ArenaBoundsCustom BuildArena(bool rotate) => new(35, BuildPolygon(rotate));
    private static AOEShapeCustom BuildTelegraph(bool rotate) => new(BuildPolygon(rotate));

    private static RelSimplifiedComplexPolygon BuildPolygon(bool rotate)
    {
        var poly = new RelPolygonWithHoles([new(-35, +35), new(-35, -5), new(-25, -5), new(-25, -25), new(+5, -25), new(+5, -35), new(+35, -35), new(+35, +5), new(+25, +5), new(+25, +25), new(-5, +25), new(-5, +35)]);
        poly.HoleStarts.Add(poly.Vertices.Count);
        poly.Vertices.AddRange([new(-15, +15), new(-15, -5), new(-5, -5), new(-5, -15), new(+15, -15), new(+15, +5), new(+5, +5), new(+5, +15)]);
        if (rotate)
            foreach (ref var v in poly.Vertices.AsSpan())
                v = v.OrthoR();
        return new([poly]);
    }
}

class CrystallineThornsHint(BossModule module) : BossComponent(module)
{
    private int _pattern; // 0 = inactive, 1/2 = map layouts

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x01)
            return;

        switch (state)
        {
            case 0x00020001:
                _pattern = 1;
                break;
            case 0x02000100:
                _pattern = 2;
                break;
            case 0x00200010:
            case 0x08000400:
                _pattern = 0;
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_pattern == 0)
            return;

        var pos = actor.Position;
        var c = A11Prishe.ArenaCenter;
        var inLane = _pattern == 1
            ? (Math.Abs(pos.X - (c.X - 5)) <= 10 && Math.Abs(pos.Z - (c.Z + 5)) <= 10) || (Math.Abs(pos.X - (c.X + 5)) <= 10 && Math.Abs(pos.Z - (c.Z - 5)) <= 10)
            : (Math.Abs(pos.X - (c.X - 5)) <= 10 && Math.Abs(pos.Z - (c.Z - 5)) <= 10) || (Math.Abs(pos.X - (c.X + 5)) <= 10 && Math.Abs(pos.Z - (c.Z + 5)) <= 10);
        if (!inLane)
            hints.Add("Go into middle to prepare for knockback!");
    }
}
