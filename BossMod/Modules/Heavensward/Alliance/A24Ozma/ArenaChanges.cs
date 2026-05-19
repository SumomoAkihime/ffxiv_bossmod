namespace BossMod.Heavensward.Alliance.A24Ozma;

// Reborn arena uses large custom polygons; keep a stable downgraded switcher for minimum playable guidance.
sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    private enum ArenaMode : uint
    {
        Main = 0,
        Split = 1,
        Shade = 2
    }

    private readonly ArenaMode[] _playerMode = new ArenaMode[PartyState.MaxAllianceSize];

    public static readonly WPos MainCenter = new(280, -410);
    public static readonly ArenaBoundsCircle MainBounds = new(25);
    private static readonly WPos SplitCenter = new(300, 260);
    private static readonly ArenaBoundsSquare SplitBounds = new(32);
    private static readonly WPos ShadeCenter = new(301.5f, 205.5f);
    private static readonly ArenaBoundsCircle ShadeBounds = new(30);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var pos = pc.Position;
        if (_playerMode[pcSlot] != ArenaMode.Split && pos.InCircle(SplitCenter, 35))
        {
            Arena.Center = SplitCenter;
            Arena.Bounds = SplitBounds;
            _playerMode[pcSlot] = ArenaMode.Split;
        }
        else if (_playerMode[pcSlot] != ArenaMode.Shade && pos.InCircle(ShadeCenter, 33))
        {
            Arena.Center = ShadeCenter;
            Arena.Bounds = ShadeBounds;
            _playerMode[pcSlot] = ArenaMode.Shade;
        }
        else if (_playerMode[pcSlot] != ArenaMode.Main && pos.InCircle(MainCenter, 45))
        {
            Arena.Center = MainCenter;
            Arena.Bounds = MainBounds;
            _playerMode[pcSlot] = ArenaMode.Main;
        }
    }
}
