namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        // suzaku
        if (index == 0x4B)
        {
            if (state == 0x00020001)
                Arena.Bounds = new ArenaBoundsSquare(20);
            else if (state == 0x00080004)
                Arena.Bounds = new ArenaBoundsCircle(29.5f);
        }

        // byakko
        if (index == 0x4C)
        {
            if (state == 0x00020001)
                Arena.Bounds = new ArenaBoundsCircle(27);
            else if (state == 0x00080004)
                Arena.Bounds = new ArenaBoundsCircle(29.5f);
        }
    }
}
