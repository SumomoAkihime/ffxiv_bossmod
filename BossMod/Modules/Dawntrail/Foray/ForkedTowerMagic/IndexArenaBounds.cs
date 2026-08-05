namespace BossMod.Dawntrail.Foray.ForkedTowerMagic;

internal static class IndexArenaBounds
{
    private const float OuterHexRadius = 15f;
    private const float InnerHexRadius = 7.5f;
    private const float PlatformCenterRadius = 20.5f;
    private const float PlatformHalfSize = 7.5f;
    private const float OuterHexApothem = 12.990381f;
    private const float InnerHexApothem = 6.4951906f;
    private static readonly Angle[] InitialPlatformAngles = [0f.Degrees(), 120f.Degrees(), -120f.Degrees()];
    private static readonly Angle[] ExpandedPlatformAngles = [0f.Degrees(), 60f.Degrees(), 120f.Degrees(), 180f.Degrees(), -120f.Degrees(), -60f.Degrees()];

    public static ArenaBoundsCustom Initial(WPos center) => Build(center, InitialPlatformAngles);
    public static ArenaBoundsCustom Expanded(WPos center) => Build(center, ExpandedPlatformAngles);

    public static AOEShapeCustom PlatformRegion(WPos center, Angle angle)
    {
        var direction = angle.ToDirection();
        var side = direction.OrthoL();
        var innerMidpoint = center + InnerHexApothem * direction;
        var outerMidpoint = center + OuterHexApothem * direction;
        Shape[] shapes =
        [
            new Square(center + PlatformCenterRadius * direction, PlatformHalfSize, angle),
            new PolygonCustom(
            [
                innerMidpoint + 3.75f * side,
                outerMidpoint + PlatformHalfSize * side,
                outerMidpoint - PlatformHalfSize * side,
                innerMidpoint - 3.75f * side
            ])
        ];
        return new(shapes, origin: center);
    }

    private static ArenaBoundsCustom Build(WPos center, Angle[] platformAngles)
    {
        Shape[] platforms =
        [
            new Polygon(center, OuterHexRadius, 6, 30f.Degrees()),
            .. platformAngles.Select(angle => new Square(center + PlatformCenterRadius * angle.ToDirection(), PlatformHalfSize, angle))
        ];
        Shape[] centralHole = [new Polygon(center, InnerHexRadius, 6, 30f.Degrees())];
        return new(platforms, centralHole, MapResolution: 0.25f);
    }
}
