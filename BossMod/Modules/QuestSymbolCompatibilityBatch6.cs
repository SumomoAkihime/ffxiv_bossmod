namespace BossMod.Heavensward.Quest.MSQ.FlyFreeMyPretty
{
    class AutoReaperAI(BossModule module) : BossComponent(module);
}

namespace BossMod.RealmReborn.Quest.MSQ.OperationArchon
{
    class MagitekMissilesTartareanShockwave(BossModule module) : BossComponent(module);
    class DrillShotGalesOfTartarus(BossModule module) : BossComponent(module);
    class TartareanShockwaveBig(BossModule module) : BossComponent(module);
    class GalesOfTartarusBig(BossModule module) : BossComponent(module);
}

namespace BossMod.RealmReborn.Quest.MSQ.TheStepsOfFaith
{
    class FireballAOE(BossModule module) : BossComponent(module);
    class Levinshower(BossModule module) : BossComponent(module);
}

namespace BossMod.RealmReborn.Quest.MSQ.TheUltimateWeapon
{
    class ArenaChange(BossModule module) : BossComponent(module);
    class EoD(BossModule module) : BossComponent(module);
}

namespace BossMod.Stormblood.Quest.Job.Samurai.TheFaceOfTrueEvil
{
    abstract class Hissatsu(BossModule module) : BossComponent(module);
}

namespace BossMod.Stormblood.Quest.Job.Summoner.AnArtForTheLiving
{
    abstract class NerveGasLR(BossModule module) : BossComponent(module);
    class NerveGasLeft(BossModule module) : NerveGasLR(module);
    class NerveGasRight(BossModule module) : NerveGasLR(module);
}

namespace BossMod.Stormblood.Quest.MSQ.EmissaryOfTheDawn
{
    class AlphinaudAI(BossModule module) : BossComponent(module);
}
