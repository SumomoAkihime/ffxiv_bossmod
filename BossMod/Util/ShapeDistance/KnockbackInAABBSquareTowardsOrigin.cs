namespace BossMod;

[SkipLocalsInit]
public sealed class SDKnockbackInAABBSquareTowardsOrigin(WPos center, WPos origin, float distance, float halfWidth) : ShapeDistance
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WPos p)
    {
        var toOrigin = origin - p;
        var direction = toOrigin.Normalized();
        return direction == default || !(p + distance * direction).InSquare(center, halfWidth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool RowIntersectsShape(WPos rowStart, WDir dx, float width, float cushion = default) => true;
}
