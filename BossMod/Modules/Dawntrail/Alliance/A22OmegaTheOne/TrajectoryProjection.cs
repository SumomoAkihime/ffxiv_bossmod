namespace BossMod.Dawntrail.Alliance.A22OmegaTheOne;

// Compatibility wrapper: this fork lacks Reborn's offset-bait constructor shape,
// so we reuse existing local offset-bait implementation.
class TrajectoryProjection(BossModule module) : GuidedMissileBait(module);
