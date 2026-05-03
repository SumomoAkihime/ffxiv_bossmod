namespace BossMod.Components;

// Compatibility adapters for modules authored against the Reborn component naming.
// They map to this fork's existing component implementations.
file enum CompatAID : uint { }

public class SimpleAOEs(BossModule module, uint aid, AOEShape shape, int maxCasts = int.MaxValue) : StandardAOEs(module, (Enum)Enum.ToObject(typeof(CompatAID), aid), shape, maxCasts)
{
    public SimpleAOEs(BossModule module, uint aid, float radius, int maxCasts = int.MaxValue) : this(module, aid, new AOEShapeCircle(radius), maxCasts) { }
}

public class SimpleAOEGroups(BossModule module, uint[] aids, AOEShape shape, int maxCasts = int.MaxValue) : GroupedAOEs(module, [.. aids.Select(a => (Enum)Enum.ToObject(typeof(CompatAID), a))], shape, maxCasts)
{
    public SimpleAOEGroups(BossModule module, uint[] aids, float radius, int maxCasts = int.MaxValue) : this(module, aids, new AOEShapeCircle(radius), maxCasts) { }
}

public class SimpleKnockbacks(BossModule module, uint aid, float distance, AOEShape? shape = null, Knockback.Kind kind = Knockback.Kind.AwayFromOrigin, bool stopAtWall = false)
    : KnockbackFromCastTarget(module, (Enum)Enum.ToObject(typeof(CompatAID), aid), distance, shape: shape, kind: kind, stopAtWall: stopAtWall)
{
}
