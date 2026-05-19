namespace BossMod.Shadowbringers.Alliance.A32HanselGretel;

// Compatibility layer for Reborn naming (A32HanselGretel -> A32HanselAndGretel).
// Enum member shapes are intentionally omitted to keep this a low-risk symbol bridge.
public enum OID : uint { }
public enum AID : uint { }
public enum SID : uint { }
public enum IconID : uint { }
public enum TetherID : uint { }

sealed class WailLamentation(BossModule module) : BossComponent(module);
sealed class CripplingBlow1(BossModule module) : BossMod.Shadowbringers.Alliance.A32HanselAndGretel.CripplingBlow1(module);
sealed class CripplingBlow2(BossModule module) : BossMod.Shadowbringers.Alliance.A32HanselAndGretel.CripplingBlow2(module);
sealed class BloodySweep(BossModule module) : BossMod.Shadowbringers.Alliance.A32HanselAndGretel.BloodySweep(module);
sealed class PassingLance(BossModule module) : BossMod.Shadowbringers.Alliance.A32HanselAndGretel.PassingLance(module);
sealed class UnevenFooting(BossModule module) : BossMod.Shadowbringers.Alliance.A32HanselAndGretel.UnevenFooting(module);
sealed class HungryLance(BossModule module) : BossMod.Shadowbringers.Alliance.A32HanselAndGretel.HungryLance(module);
sealed class Breakthrough(BossModule module) : BossMod.Shadowbringers.Alliance.A32HanselAndGretel.Breakthrough(module);
sealed class SeedOfMagicBeta(BossModule module) : BossMod.Shadowbringers.Alliance.A32HanselAndGretel.SeedOfMagicBeta(module);
sealed class UpgradedShield(BossModule module) : BossMod.Shadowbringers.Alliance.A32HanselAndGretel.UpgradedShield1(module);
sealed class MagicalConfluence(BossModule module) : BossMod.Shadowbringers.Alliance.A32HanselAndGretel.MagicalConfluence(module);
sealed class StrongerTogether(BossModule module) : BossMod.Shadowbringers.Alliance.A32HanselAndGretel.StrongerTogether(module);

class A32HanselGretelStates(BossModule module) : BossMod.Shadowbringers.Alliance.A32HanselAndGretel.A32HanselAndGretelStates((BossMod.Shadowbringers.Alliance.A32HanselAndGretel.A32HanselAndGretel)module);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9990)]
public sealed class A32HanselGretel(WorldState ws, Actor primary) : BossMod.Shadowbringers.Alliance.A32HanselAndGretel.A32HanselAndGretel(ws, primary);
