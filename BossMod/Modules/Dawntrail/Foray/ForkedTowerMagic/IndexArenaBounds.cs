namespace BossMod.Dawntrail.Foray.ForkedTowerMagic;

internal static class IndexArenaBounds
{
    private const float PlatformCenterRadius = 20.5f;
    private const float PlatformHalfSize = 7.5f;
    private const float OuterHexApothem = 12.990381f;
    private const float InnerHexApothem = 5f;
    private const float InnerHexHalfSide = 2.88675f;

    // Extracted from material 0x00007004; coordinates are relative to the mechanic center.
    private static readonly WDir[] InitialBoundary =
    [
        new(7.50198f, 12.99390f), new(7.49990f, 27.99988f), new(-7.50010f, 27.99988f),
        new(-7.50079f, 27.99933f), new(-7.50276f, 12.99420f), new(-15.00425f, -0.00012f),
        new(-27.99880f, -7.50494f), new(-20.49879f, -20.49530f), new(-7.50275f, -12.99445f),
        new(7.50200f, -12.99408f), new(20.49863f, -20.49530f), new(27.99863f, -7.50494f),
        new(15.00408f, -0.00012f)
    ];

    private static readonly WDir[] ExpandedBoundary =
    [
        new(27.99862f, 7.50470f), new(20.49862f, 20.49506f), new(7.50198f, 12.99390f),
        new(7.49990f, 27.99988f), new(-7.50010f, 27.99988f), new(-7.50079f, 27.99933f),
        new(-7.50276f, 12.99420f), new(-20.49881f, 20.49506f), new(-27.99881f, 7.50470f),
        new(-15.00425f, -0.00012f), new(-27.99880f, -7.50494f), new(-20.49879f, -20.49530f),
        new(-7.50275f, -12.99445f), new(-7.50076f, -28.00049f), new(0.73911f, -28.00031f),
        new(7.49962f, -28.00043f), new(7.49992f, -28.00012f), new(7.50200f, -12.99408f),
        new(20.49863f, -20.49530f), new(27.99863f, -7.50494f), new(15.00408f, -0.00012f)
    ];

    private static readonly WDir[] InnerBoundary =
    [
        new(-2.88752f, 4.99896f), new(0.62856f, 4.99957f), new(2.88607f, 4.99933f),
        new(5.77356f, -0.00012f), new(2.88633f, -5.00024f), new(-2.88692f, -5.00024f),
        new(-5.77374f, -0.00012f)
    ];

    public static ArenaBoundsCustom Initial(WPos center) => Build(center, InitialBoundary);
    public static ArenaBoundsCustom Expanded(WPos center) => Build(center, ExpandedBoundary);

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
                innerMidpoint + InnerHexHalfSide * side,
                outerMidpoint + PlatformHalfSize * side,
                outerMidpoint - PlatformHalfSize * side,
                innerMidpoint - InnerHexHalfSide * side
            ])
        ];
        return new(shapes, origin: center);
    }

    private static ArenaBoundsCustom Build(WPos center, WDir[] boundary)
    {
        Shape[] arena = [new PolygonCustom([.. boundary.Select(offset => center + offset)])];
        Shape[] centralHole = [new PolygonCustom([.. InnerBoundary.Select(offset => center + offset)])];
        return new(arena, centralHole, MapResolution: 0.25f, Offset: -1f);
    }
}
