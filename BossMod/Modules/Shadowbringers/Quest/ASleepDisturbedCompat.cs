namespace BossMod.Shadowbringers.Quest.ASleepDisturbed;

// Compatibility bridge for historical namespace typo:
// ASleepDistubed -> ASleepDisturbed
class TouchOfShadow(BossModule module) : BossMod.Shadowbringers.Quest.ASleepDistubed.TouchOfShadow(module);
class MarrowOfFlame(BossModule module) : BossMod.Shadowbringers.Quest.ASleepDistubed.MarrowOfFlame(module);
class GraceOfCalamity(BossModule module) : BossMod.Shadowbringers.Quest.ASleepDistubed.GraceOfCalamity(module);
class BurningBeamNPC(BossModule module) : BossMod.Shadowbringers.Quest.ASleepDistubed.BurningBeamNPC(module);
class BurningBeamPlayer(BossModule module) : BossMod.Shadowbringers.Quest.ASleepDistubed.BurningBeamPlayer(module);
class SoundOfHeat(BossModule module) : BossMod.Shadowbringers.Quest.ASleepDistubed.SoundOfHeat(module);
class DeceitOfPain(BossModule module) : BossMod.Shadowbringers.Quest.ASleepDistubed.DeceitOfPain(module);
class BalmOfDisgrace(BossModule module) : BossMod.Shadowbringers.Quest.ASleepDistubed.BalmOfDisgrace(module);

class ASleepDisturbedStates(BossModule module) : BossMod.Shadowbringers.Quest.ASleepDistubed.ASleepDisturbedStates(module);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69301, NameID = 9296)]
public class ASleepDisturbed(WorldState ws, Actor primary) : BossMod.Shadowbringers.Quest.ASleepDistubed.ASleepDisturbed(ws, primary);
