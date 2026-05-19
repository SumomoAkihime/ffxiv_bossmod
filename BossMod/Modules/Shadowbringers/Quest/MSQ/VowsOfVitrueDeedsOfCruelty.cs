namespace BossMod.Shadowbringers.Quest.MSQ.VowsOfVirtueDeedsOfCruelty;

// Compatibility bridge for namespace drift:
// Quest.MSQ.VowsOfVirtueDeedsOfCruelty -> Quest.VowsOfVirtueDeedsOfCruelty
public enum OID : uint { }
public enum AID : uint { }

class MagitekRay(BossModule module) : BossMod.Shadowbringers.Quest.VowsOfVirtueDeedsOfCruelty.MagitekRayRightArm(module);
class AngrySalamander(BossModule module) : BossMod.Shadowbringers.Quest.VowsOfVirtueDeedsOfCruelty.AngrySalamander(module);
class TerminusEstRects(BossModule module) : BossMod.Shadowbringers.Quest.VowsOfVirtueDeedsOfCruelty.TerminusEstRects(module);
class TerminusEstCircle(BossModule module) : BossMod.Shadowbringers.Quest.VowsOfVirtueDeedsOfCruelty.TerminusEstCircle(module);
class FireII(BossModule module) : BossMod.Shadowbringers.Quest.VowsOfVirtueDeedsOfCruelty.FireII(module);
class GarleanFire(BossModule module) : BossMod.Shadowbringers.Quest.VowsOfVirtueDeedsOfCruelty.GarleanFire(module);
class MetalCutter(BossModule module) : BossMod.Shadowbringers.Quest.VowsOfVirtueDeedsOfCruelty.MetalCutter(module);
class MagitekRayBits(BossModule module) : BossMod.Shadowbringers.Quest.VowsOfVirtueDeedsOfCruelty.MagitekRayBits(module);
class AtomicRay(BossModule module) : BossMod.Shadowbringers.Quest.VowsOfVirtueDeedsOfCruelty.AtomicRay(module);
class SelfDetonate(BossModule module) : BossMod.Shadowbringers.Quest.VowsOfVirtueDeedsOfCruelty.SelfDetonate(module);
class EstinienAI(WorldState ws) : BossMod.Shadowbringers.Quest.VowsOfVirtueDeedsOfCruelty.EstinienAI(ws);
class AutoEstinien(BossModule module) : BossMod.Shadowbringers.Quest.VowsOfVirtueDeedsOfCruelty.AutoEstinien(module);
class ArchUltimaStates(BossModule module) : BossMod.Shadowbringers.Quest.VowsOfVirtueDeedsOfCruelty.ArchUltimaStates(module);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69218, NameID = 9189)]
public class ArchUltima(WorldState ws, Actor primary) : BossMod.Shadowbringers.Quest.VowsOfVirtueDeedsOfCruelty.ArchUltima(ws, primary);
