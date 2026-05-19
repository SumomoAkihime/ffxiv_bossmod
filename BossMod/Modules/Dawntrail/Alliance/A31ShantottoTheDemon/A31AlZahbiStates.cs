using BossMod.Dawntrail.Alliance.A31Shantotto;

namespace BossMod.Dawntrail.Alliance.A31AlZahbi;

// State machine alias to canonical builder; do not duplicate mechanic activations.
sealed class A31AlZahbiStates(BossModule module) : A31ShantottoTheDemonStates(module);
