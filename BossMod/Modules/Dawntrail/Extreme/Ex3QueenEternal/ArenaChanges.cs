namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override bool KeepOnPhaseChange => true;
    private bool firstEarthArena = true;
    public bool EnrageCastStarted;
    public bool EnrageCastEnded;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AuthorityEternal)
            EnrageCastStarted = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AuthorityEternal)
            EnrageCastEnded = true;
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID != 0x8000000D || param1 > 0x08u)
        {
            return;
        }
        switch (param1)
        {
            case 0x01u: // default arena
                SetArena(Ex3QueenEternal.NormalBounds, Ex3QueenEternal.ArenaCenter);
                break;
            case 0x02u: // x arena (wind)
                SetArena(Ex3QueenEternal.WindBounds, Ex3QueenEternal.WindBounds.Center);
                break;
            case 0x04u: // disjointed rect (Earth) arena
                if (firstEarthArena)
                {
                    var center = Ex3QueenEternal.ArenaCenter;
                    var square = new Square(center, 21f);
                    var split = PolygonClipper.GetCombinedPolygon(center, Trial.T03QueenEternal.T03QueenEternal.SplitArenaRects);
                    var bounds = new ArenaBoundsCustom([square], WorldProjectionLayers: [new(square.ToPolygon(center), 0f, borderY: 0f), new(split, 0f, borderY: 0f)]);
                    SetArena(bounds, center);
                    firstEarthArena = false;
                }
                else
                    SetArena(Ex3QueenEternal.EarthBounds, Ex3QueenEternal.EarthBounds.Center);
                break;
            case 0x08u: // ice arena
                SetArena(Ex3QueenEternal.IceBounds, Ex3QueenEternal.IceBounds.Center);
                break;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x08)
        {
            if (state == 0x01000080u)
                SetArena(Ex3QueenEternal.HalfBounds, Ex3QueenEternal.HalfBoundsCenter);
            else if (state == 0x02000001u)
                SetArena(Ex3QueenEternal.NormalBounds, Ex3QueenEternal.ArenaCenter);
        }
    }

    private void SetArena(ArenaBounds bounds, WPos center)
    {
        Arena.Bounds = bounds;
        Arena.Center = center;
    }
}
