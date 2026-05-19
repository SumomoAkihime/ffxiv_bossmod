namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.DD70Kenko
{
    // Symbol-compat placeholders for Reborn names not present in local implementation.
    class Devour(BossModule module) : BossComponent(module);
    class InnerspaceSpread(BossModule module) : BossComponent(module);
    class InnerspaceVoidzone(BossModule module) : BossComponent(module);
}

namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.DD80Kajigakaka
{
    class HeavenswardHowl(BossModule module) : BossComponent(module);
    class SphereShatter(BossModule module) : BossComponent(module);

    class DD80KajigakakaStates(BossModule module) : StateMachineBuilder(module);

    public class DD80Kajigakaka(WorldState ws, Actor primary)
        : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(24));
}

namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh.DD90Onra
{
    class AncientQuaga(BossModule module) : BossComponent(module);
    class AuraCannon(BossModule module) : BossComponent(module);
    class BurningRave(BossModule module) : BossComponent(module);
    class KnucklePress(BossModule module) : BossComponent(module);
    class Subduction(BossModule module) : BossComponent(module);

    class DD90OnraStates(BossModule module) : StateMachineBuilder(module);

    public class DD90Onra(WorldState ws, Actor primary)
        : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(24));
}

namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh
{
    class HoHArena1;
    class HoHArena2;
    class HoHArenas;
}
