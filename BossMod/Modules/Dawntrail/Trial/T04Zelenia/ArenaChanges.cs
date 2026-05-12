namespace BossMod.Dawntrail.Trial.T04Zelenia;

class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Circle = new(4);
    private AOEInstance[] _aoe = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.QueensCrusade)
            _aoe = [new(Circle, Arena.Center, default, Module.CastFinishAt(spell, 0.1f))];
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x01)
            return;

        switch (state)
        {
            case 0x00020001u:
                _aoe = [];
                Arena.Bounds = T04Zelenia.DonutArena;
                break;
            case 0x00080004u:
                Arena.Bounds = T04Zelenia.DefaultArena;
                break;
        }
    }
}
