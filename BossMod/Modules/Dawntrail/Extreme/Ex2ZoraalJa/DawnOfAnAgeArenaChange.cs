namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

sealed class DawnOfAnAgeArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom _shape = BuildShape();
    private readonly List<AOEInstance> _aoes = [];

    private static AOEShapeCustom BuildShape()
    {
        var dir = Ex2ZoraalJa.NormalBounds.Orientation;
        var outer = CurveApprox.Rect(dir, 20, 20).ToList();
        var poly = new RelPolygonWithHoles(outer);
        poly.AddHole(CurveApprox.Rect(dir, 10, 10).ToList());
        return new AOEShapeCustom(new RelSimplifiedComplexPolygon([poly]));
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DawnOfAnAge)
            _aoes.Add(new(_shape, Arena.Center, default, Module.CastFinishAt(spell, 0.9f)));
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0B && state == 0x00200010u)
            _aoes.Clear();
    }
}
