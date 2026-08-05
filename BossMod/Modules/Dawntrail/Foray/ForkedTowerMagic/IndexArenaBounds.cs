namespace BossMod.Dawntrail.Foray.ForkedTowerMagic;

internal static class IndexArenaBounds
{
    private static readonly Angle[] InitialPlatformAngles = [0f.Degrees(), 120f.Degrees(), -120f.Degrees()];
    private static readonly Angle[] ExpandedPlatformAngles = [0f.Degrees(), 60f.Degrees(), 120f.Degrees(), 180f.Degrees(), -120f.Degrees(), -60f.Degrees()];

    public static ArenaBoundsCustom Initial(WPos center) => Build(center, InitialPlatformAngles);
    public static ArenaBoundsCustom Expanded(WPos center) => Build(center, ExpandedPlatformAngles);

    private static ArenaBoundsCustom Build(WPos center, Angle[] platformAngles)
    {
        Shape[] platforms =
        [
            new Polygon(center, 15f, 6, 30f.Degrees()),
            .. platformAngles.Select(angle => new Square(center + 20.5f * angle.ToDirection(), 7.5f, angle))
        ];
        Shape[] centralHole = [new Polygon(center, 7.5f, 6, 30f.Degrees())];
        return new(platforms, centralHole, MapResolution: 0.25f);
    }
}
