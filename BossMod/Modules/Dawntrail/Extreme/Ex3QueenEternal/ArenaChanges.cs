namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class ArenaChanges : BossComponent
{
    private bool _firstEarthArena = true;

    public ArenaChanges(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID != 0x8000000D || param1 > 0x08)
            return;

        switch (param1)
        {
            case 0x01: // default arena
                Arena.Bounds = Ex3QueenEternal.NormalBounds;
                Arena.Center = new(100, 100);
                break;
            case 0x02: // wind arena
                Arena.Bounds = Ex3QueenEternal.WindBounds;
                Arena.Center = new(100, 100);
                break;
            case 0x04: // earth arena
                if (_firstEarthArena)
                    _firstEarthArena = false; // keep original behavior: skip first earth remap
                else
                {
                    Arena.Bounds = Ex3QueenEternal.EarthBounds;
                    Arena.Center = new(100, 100);
                }
                break;
            case 0x08: // ice arena
                Arena.Bounds = Ex3QueenEternal.IceBounds;
                Arena.Center = new(100, 100);
                break;
        }
    }
}
