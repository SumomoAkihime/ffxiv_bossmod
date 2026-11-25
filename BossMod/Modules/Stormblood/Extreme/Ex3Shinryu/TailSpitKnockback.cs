namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

class TailSpitKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TailSpit, 15)
{
    private static readonly WPos _spot = Ex3Shinryu.P2Center + 6.5f * (-135f).Degrees().ToDirection();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => false;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
            hints.AddForbiddenZone(new SDInvertedCircle(_spot, 1.5f), Casters[0].Activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Casters.Count > 0 && !actor.Position.InCircle(_spot, 1.5f))
            hints.Add("Stand north-west of the center, the knockback throws you onto the tail!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (Casters.Count > 0)
            Arena.AddCircleUnfilled(_spot, 1.5f, Colors.Safe, 2);
    }
}
