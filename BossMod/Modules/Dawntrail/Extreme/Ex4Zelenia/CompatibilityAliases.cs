namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

// Compatibility wrappers for Reborn split-file naming.
sealed class AlexandrianBanishIII(BossModule module) : BanishIII(module);
sealed class ValorousAscension(BossModule module) : ValorousAscensionRaidwide(module);
sealed class SpearpointPushAOE(BossModule module) : SpearpointAOE(module);
sealed class SpearpointPushBait(BossModule module) : SpearpointBait(module);
sealed class Towers2(BossModule module) : AddsExplosion(module);
sealed class ShockAOE(BossModule module) : ShockAOEs(module);
