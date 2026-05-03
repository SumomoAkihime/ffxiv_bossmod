namespace BossMod.Dawntrail.Extreme.Ex5Necron;

// Compatibility wrappers for Reborn split-file naming.
sealed class FearOfDeath(BossModule module) : FearOfDeathRaidwide(module);
sealed class FearOfDeathAOE(BossModule module) : FearOfDeathPuddle(module);
