namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

// Compatibility wrappers for Reborn split-file naming.
sealed class AlexandrianBanishIII(BossModule module) : BanishIII(module);
sealed class ValorousAscension(BossModule module) : ValorousAscensionRaidwide(module);
sealed class SpearpointPushAOE(BossModule module) : SpearpointAOE(module);
sealed class SpearpointPushBait(BossModule module) : SpearpointBait(module);
sealed class Towers2(BossModule module) : AddsExplosion(module);
sealed class Towers1(BossModule module) : P1Explosion(module);
sealed class ShockAOE(BossModule module) : ShockAOEs(module);
sealed class ShockSpread(BossModule module) : ShockCircleBait(module);
sealed class AlexandrianThunderIIIAOE(BossModule module) : Bloom4AlexandrianThunderIIIAOE(module);
sealed class AlexandrianThunderIIISpread(BossModule module) : Bloom4AlexandrianThunderIII(module);
