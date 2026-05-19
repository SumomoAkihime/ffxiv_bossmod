using BossMod.Dawntrail.Alliance.A31Shantotto;

namespace BossMod.Dawntrail.Alliance.A31AlZahbi;

// Compatibility bridge for Reborn naming drift: keep a single canonical implementation.
public sealed class A31AlZahbi(WorldState ws, Actor primary) : A31ShantottoTheDemon(ws, primary);
