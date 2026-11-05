namespace BossMod.Stormblood.Ultimate.UWU;

class P5AethericBoom : Components.SimpleKnockbacks
{
    readonly DateTime _deadlineDefault;

    public P5AethericBoom(BossModule module) : base(module, (uint)AID.AethericBoom, 10)
    {
        _deadlineDefault = WorldState.FutureTime(6.1f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var activation = Casters.Count > 0 ? Casters[0].Activation : _deadlineDefault;
        if (IsImmune(slot, activation))
            return;
        var side = actor.Role == Role.Tank ? -1 : 1;
        hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center + new WDir(0, 3).Rotate((side * 45).Degrees()), 1), Casters.Count > 0 ? Casters[0].Activation : _deadlineDefault);
    }
}
class P5Aetheroplasm(BossModule module) : Components.CastCounter(module, (uint)AID.AetheroplasmUltima)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Module.Enemies(OID.Ultimaplasm).Where(p => !p.IsDead), Colors.Object, true);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (NumCasts < 2)
        {
            var p1 = new SDInvertedCircle(new(108, 108), 1);
            var p2 = new SDInvertedCircle(new(92, 108), 1);
            hints.AddForbiddenZone(new SDIntersection([p1, p2]));
        }
        else if (NumCasts < 4)
        {
            var p3 = new SDInvertedCircle(new(108, 92), 1);
            var p4 = new SDInvertedCircle(new(92, 92), 1);
            hints.AddForbiddenZone(new SDIntersection([p3, p4]));
        }
    }
}
