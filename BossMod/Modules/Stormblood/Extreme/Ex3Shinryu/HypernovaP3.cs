namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

class HypernovaP3(BossModule module) : Components.CastHint(module, (uint)AID.HypernovaVisualP3, "Stack together")
{
    private WPos StackSpot() => Raid.WithoutSlot().Aggregate((sum: new WDir(), n: 0), (a, p) => (a.sum + (p.Position - Arena.Center), a.n + 1)) is var t && t.n > 0 ? Arena.Center + t.sum / t.n : Arena.Center;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
            hints.AddForbiddenZone(new SDInvertedCircle(StackSpot(), 2), Module.CastFinishAt(Casters[0].CastInfo));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Casters.Count > 0)
            Arena.AddCircleUnfilled(StackSpot(), 2, Colors.Safe);
    }
}
