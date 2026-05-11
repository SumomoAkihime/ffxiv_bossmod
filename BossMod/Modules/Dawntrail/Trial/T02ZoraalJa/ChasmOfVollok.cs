namespace BossMod.Dawntrail.Trial.T02ZoraalJaP2;

class ChasmOfVollok(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private const float PlatformOffset = 21.2132f;
    private static readonly AOEShapeRect Rect = new(5, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.ChasmOfVollok1)
            return;

        if (Arena.InBounds(caster.Position))
        {
            AOEs.Add(new(Rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
        else
        {
            var pos = spell.LocXZ;
            var offset = new WDir(pos.X > Arena.Center.X ? -PlatformOffset : PlatformOffset, pos.Z > Arena.Center.Z ? -PlatformOffset : PlatformOffset);
            AOEs.Add(new(Rect, pos + offset, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ChasmOfVollok1 or AID.ChasmOfVollok2)
            AOEs.Clear();
    }
}
