namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

class DarkNebula(BossModule module) : Components.Knockback(module)
{
    public readonly List<Actor> Casters = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var caster in Casters.Take(2))
        {
            var dir = caster.CastInfo?.Rotation ?? caster.Rotation;
            var kind = dir.ToDirection().OrthoL().Dot(actor.Position - caster.Position) > 0 ? Kind.DirLeft : Kind.DirRight;
            yield return new(caster.Position, 20, Module.CastFinishAt(caster.CastInfo), null, dir, kind);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DarkNebulaShort or AID.DarkNebulaLong)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DarkNebulaShort or AID.DarkNebulaLong)
        {
            ++NumCasts;
            Casters.Remove(caster);
        }
    }
}
