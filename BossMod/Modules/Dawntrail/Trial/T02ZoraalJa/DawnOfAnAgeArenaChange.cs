namespace BossMod.Dawntrail.Trial.T02ZoraalJaP2;

class DawnOfAnAgeArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom Shape = BuildShape();
    private readonly List<AOEInstance> _aoes = [];

    private static AOEShapeCustom BuildShape()
    {
        var dir = T02ZoraalJa.ZoraalJa.ArenaRotation.ToDirection();
        var outer = CurveApprox.Rect(dir, 20, 20).ToList();
        var poly = new RelPolygonWithHoles(outer);
        poly.AddHole(CurveApprox.Rect(dir, 10, 10).ToList());
        return new AOEShapeCustom(new RelSimplifiedComplexPolygon([poly]));
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DawnOfAnAge or AID.Actualize)
            _aoes.Add(new(Shape, Arena.Center, default, Module.CastFinishAt(spell, 0.9f)));
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x20 && state == 0x00080004u)
        {
            _aoes.Clear();
            Arena.Bounds = T02ZoraalJa.ZoraalJa.SmallBounds;
        }
        else if (index == 0x1B && state == 0x00080004u)
        {
            Arena.Bounds = T02ZoraalJa.ZoraalJa.DefaultBounds;
        }
    }
}
