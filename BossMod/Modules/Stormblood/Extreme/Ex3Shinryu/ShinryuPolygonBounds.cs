namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

// Upstream polygons use a fixed mechanic center even when the remaining tiles are asymmetric.
// Keep that coordinate system while reusing the local polygon queries and map rasterization.
sealed class ShinryuPolygonBounds : ArenaBounds
{
    private readonly ArenaBoundsCustom _bounds;
    private readonly WDir _centerOffset;

    public ShinryuPolygonBounds(float radius, RelSimplifiedComplexPolygon polygon, float mapResolution = 0.5f)
        : base(radius, mapResolution)
    {
        Shape = polygon;
        List<Shape> platforms = [];
        List<Shape> holes = [];
        foreach (var part in polygon.Parts)
        {
            platforms.Add(new PolygonCustom(part.Exterior.ToArray().Select(v => v.ToWPos()).ToArray()));
            for (var index = 0; index < part.HoleStarts.Count; ++index)
                holes.Add(new PolygonCustom(part.Interior(index).ToArray().Select(v => v.ToWPos()).ToArray()));
        }
        _bounds = new([.. platforms], [.. holes], MapResolution: mapResolution);
        _centerOffset = _bounds.Center.ToWDir();
    }

    public static WDir[] Rectangle(WDir center, WDir direction, float halfWidth, float halfHeight)
        => CurveApprox.Rect(direction, halfWidth, halfHeight).Select(vertex => center + vertex).ToArray();

    public override bool Contains(in WDir offset) => _bounds.Contains(offset - _centerOffset);
    public override float IntersectRay(in WDir originOffset, in WDir dir) => _bounds.IntersectRay(originOffset - _centerOffset, dir);
    public override WDir ClampToBounds(in WDir offset) => _bounds.ClampToBounds(offset - _centerOffset) + _centerOffset;
    public override void PathfindMap(Pathfinding.Map map, WPos center) => _bounds.PathfindMap(map, center + _centerOffset);
}
