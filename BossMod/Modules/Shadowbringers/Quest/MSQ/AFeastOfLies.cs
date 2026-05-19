namespace BossMod.Shadowbringers.Quest.MSQ.AFeastOfLies;

// Compatibility bridge for namespace drift:
// Quest.MSQ.AFeastOfLies -> Quest.AFeastOfLies
public enum OID : uint { }
public enum AID : uint { }

class UnceremoniousBeheading(BossModule module) : BossMod.Shadowbringers.Quest.AFeastOfLies.UnceremoniousBeheading(module);
class KatunCycle(BossModule module) : BossMod.Shadowbringers.Quest.AFeastOfLies.KatunCycle(module);
class MercilessRight(BossModule module) : BossMod.Shadowbringers.Quest.AFeastOfLies.MercilessRight(module);
class MercilessRight1(BossModule module) : BossMod.Shadowbringers.Quest.AFeastOfLies.MercilessRight1(module);
class MercilessLeft(BossModule module) : BossMod.Shadowbringers.Quest.AFeastOfLies.MercilessLeft(module);
class MercilessLeft1(BossModule module) : BossMod.Shadowbringers.Quest.AFeastOfLies.MercilessLeft1(module);
class Evisceration(BossModule module) : BossMod.Shadowbringers.Quest.AFeastOfLies.Evisceration(module);
class HotPursuit(BossModule module) : BossMod.Shadowbringers.Quest.AFeastOfLies.HotPursuit(module);
class NexusOfThunder(BossModule module) : BossMod.Shadowbringers.Quest.AFeastOfLies.NexusOfThunder(module);
class NexusOfThunder1(BossModule module) : BossMod.Shadowbringers.Quest.AFeastOfLies.NexusOfThunder1(module);
class Burn(BossModule module) : BossMod.Shadowbringers.Quest.AFeastOfLies.Burn(module);
class Spiritcall(BossModule module) : BossMod.Shadowbringers.Quest.AFeastOfLies.Spiritcall(module);
class Electrocution(BossModule module) : BossMod.Shadowbringers.Quest.AFeastOfLies.Electrocution(module);
class SerpentHead(BossModule module) : BossMod.Shadowbringers.Quest.AFeastOfLies.SerpentHead(module);
class RanjitStates(BossModule module) : BossMod.Shadowbringers.Quest.AFeastOfLies.RanjitStates(module);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69167, NameID = 8374)]
public class Ranjit(WorldState ws, Actor primary) : BossMod.Shadowbringers.Quest.AFeastOfLies.Ranjit(ws, primary);
