namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut _donut = new(25, 35);
    private readonly List<AOEInstance> _aoe = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Cloudsplitter)
        {
            _aoe.Clear();
            _aoe.Add(new(_donut, Arena.Center, default, Module.CastFinishAt(spell, 1.5f)));
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00020001u)
        {
            Arena.Bounds = A13ArkAngels.DefaultBounds;
            _aoe.Clear();
        }
    }
}
