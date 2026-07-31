namespace BossMod.Dawntrail.Foray.MagicTowerNormal.MTN3Deathless;

public enum OID : uint
{
    Boss = 0x4BE5, // R5.0
    BarrierHead = 0x4BE6, // R1.41
    Helper = 0x233C
}

public enum AID : uint
{
    HailOfHellflares = 47452, // Boss->self, raidwide visual
    CorpseMangler = 47459, // Boss->player, tankbuster visual
    AncientFireIII = 47455, // Boss->self, range 18 circle
    SeveredFireIIIHead = 47468, // BarrierHead->self, range 18 circle
    SeveredFireIIIBoss = 47465, // Boss->self, range 18 circle
    AncientBlizzardIII = 47456, // Boss->self, range 45 width 15 cross
    SeveredBlizzardIIIHead = 47469, // BarrierHead->self, range 45 width 15 cross
    SeveredBlizzardIIIBoss = 47466, // Boss->self, range 45 width 15 cross
    VacuumWave = 47473, // Boss->self, range 30 180-degree cone
    DeathlyRay = 47475, // BarrierHead->self, 30x6 rect
    DarkCurrentLong = 47477, // Helper->self, 60x10 rect
    DarkCurrentPulse = 47478, // Helper->self, 10x60 rect
    AncientThunder = 47458, // Helper->self, range 60 45-degree cone
    SeveredThunderHead = 47471, // Helper->self, range 60 45-degree cone
    SeveredThunderBoss = 50357 // Helper->self, range 60 45-degree cone
}

sealed class HailOfHellflares(BossModule module) : Components.RaidwideCast(module, (uint)AID.HailOfHellflares);
sealed class CorpseMangler(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CorpseMangler);
sealed class FireIII(BossModule module) : Components.SimpleAOEGroups(module,
    [(uint)AID.AncientFireIII, (uint)AID.SeveredFireIIIHead, (uint)AID.SeveredFireIIIBoss], 18f);
sealed class BlizzardIII(BossModule module) : Components.SimpleAOEGroups(module,
    [(uint)AID.AncientBlizzardIII, (uint)AID.SeveredBlizzardIIIHead, (uint)AID.SeveredBlizzardIIIBoss], new AOEShapeCross(45f, 7.5f));
sealed class VacuumWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VacuumWave, new AOEShapeCone(30f, 90f.Degrees()));
sealed class DeathlyRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeathlyRay, new AOEShapeRect(30f, 3f));
sealed class DarkCurrentLong(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkCurrentLong, new AOEShapeRect(60f, 5f));
sealed class DarkCurrentPulse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkCurrentPulse, new AOEShapeRect(10f, 30f));
sealed class Thunder(BossModule module) : Components.SimpleAOEGroups(module,
    [(uint)AID.AncientThunder, (uint)AID.SeveredThunderHead, (uint)AID.SeveredThunderBoss], new AOEShapeCone(60f, 22.5f.Degrees()));

sealed class MTN3DeathlessStates : StateMachineBuilder
{
    public MTN3DeathlessStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HailOfHellflares>()
            .ActivateOnEnter<CorpseMangler>()
            .ActivateOnEnter<FireIII>()
            .ActivateOnEnter<BlizzardIII>()
            .ActivateOnEnter<VacuumWave>()
            .ActivateOnEnter<DeathlyRay>()
            .ActivateOnEnter<DarkCurrentLong>()
            .ActivateOnEnter<DarkCurrentPulse>()
            .ActivateOnEnter<Thunder>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(MTN3DeathlessStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093,
    NameID = 14503,
    SortOrder = 3)]
public sealed class MTN3Deathless(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 800f), new ArenaBoundsCircle(30f));
