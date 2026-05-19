namespace BossMod.Stormblood.TreasureHunt
{
    // Symbol-compat placeholder for Reborn naming.
    abstract class FinalRoomArena(WorldState ws, Actor primary)
        : BossModule(ws, primary, new(0, -420), new ArenaBoundsCircle(20));
}

namespace BossMod.Stormblood.TreasureHunt.HiddenCanalsOfUznair.Airavata
{
    class DoubleSmash(BossModule module) : BossComponent(module);
    class RingOfFire(BossModule module) : BossComponent(module);
    class StoneII(BossModule module) : BossComponent(module);
}

namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair
{
    class MandragoraAOEs(BossModule module) : BossComponent(module);
}
