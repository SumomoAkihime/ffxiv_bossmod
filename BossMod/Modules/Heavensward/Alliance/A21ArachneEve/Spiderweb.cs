namespace BossMod.Heavensward.Alliance.A21ArachneEve;

sealed class SpiderWeb(BossModule module) : BossComponent(module)
{
    // Low-risk fallback: switch to a smaller circular arena on elevated layer.
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (pc.Position.InCircle(new(16f, -55f), 30f))
        {
            if (pc.PosRot.Y > 10f)
            {
                Arena.Center = new(16f, -55f);
                Arena.Bounds = new ArenaBoundsCircle(22f);
            }
            else
            {
                Arena.Center = new(16f, -55f);
                Arena.Bounds = new ArenaBoundsCircle(29.5f);
            }
        }
    }
}
