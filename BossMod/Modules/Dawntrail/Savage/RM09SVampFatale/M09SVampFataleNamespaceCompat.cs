using RM09 = BossMod.Dawntrail.Savage.RM09SVampFatale;

namespace BossMod.Dawntrail.Savage.M09SVampFatale;

// Compatibility bridge for Reborn namespace naming.
// No ModuleInfo attribute here to avoid duplicate module activation.
public class M09SVampFatale(WorldState ws, Actor primary) : RM09.RM09SVampFatale(ws, primary);

// Compatibility bridge for type lookups expecting Reborn naming.
class M09SVampFataleStates(BossModule module) : RM09.RM09SVampFataleStates(module)
{
}
