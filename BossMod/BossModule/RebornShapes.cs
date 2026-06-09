namespace BossMod;

// Minimal Reborn geometry surface used by migrated modules. Keep this small and
// map shapes into the existing polygon/bounds pipeline instead of porting a new framework.
public abstract record class Shape
{
    public const float MaxApproxError = CurveApprox.ScreenError;

    public abstract List<WDir> Contour(WPos center);

    public RelSimplifiedComplexPolygon ToPolygon(WPos center) => new(new List<RelPolygonWithHoles> { new(Contour(center)) });
}

public sealed record class Circle(WPos Center, float Radius) : Shape
{
    public override List<WDir> Contour(WPos center)
        => [.. CurveApprox.Circle(Radius, MaxApproxError).Select(p => p + Center - center)];
}

public record class Rectangle(WPos Center, float HalfWidth, float HalfHeight, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var dir = Rotation != default ? Rotation.ToDirection() : new WDir(default, 1f);
        var dx = dir.OrthoL() * HalfWidth;
        var dz = dir * HalfHeight;
        var offset = Center - center;
        return [dx - dz + offset, -dx - dz + offset, -dx + dz + offset, dx + dz + offset];
    }
}

public sealed record class Cross(WPos Center, float Length, float HalfWidth, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var dx = Rotation.ToDirection();
        var dy = dx.OrthoL();
        var dx1 = dx * Length;
        var dx2 = dx * HalfWidth;
        var dy1 = dy * Length;
        var dy2 = dy * HalfWidth;
        var offset = Center - center;
        return
        [
            dx1 + dy2 + offset,
            dx2 + dy2 + offset,
            dx2 + dy1 + offset,
            -dx2 + dy1 + offset,
            -dx2 + dy2 + offset,
            -dx1 + dy2 + offset,
            -dx1 - dy2 + offset,
            -dx2 - dy2 + offset,
            -dx2 - dy1 + offset,
            dx2 - dy1 + offset,
            dx2 - dy2 + offset,
            dx1 - dy2 + offset
        ];
    }
}

public sealed record class Polygon(WPos Center, float Radius, int Edges, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var result = new List<WDir>(Edges);
        var offset = Center - center;
        var angleIncrement = 2f * MathF.PI / Edges;
        for (var i = 0; i < Edges; ++i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(i * angleIncrement + Rotation.Rad);
            result.Add(new WDir(Radius * sin, Radius * cos) + offset);
        }
        return result;
    }
}
