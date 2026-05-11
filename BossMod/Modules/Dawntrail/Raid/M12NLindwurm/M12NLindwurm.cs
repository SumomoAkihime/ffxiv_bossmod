namespace BossMod.Dawntrail.Raid.M12NLindwurm;

public enum OID : uint
{
    Lindwurm = 0x4AF7,
    Comet = 0x4AE9,
    Maelstrom = 0x4AEB,
    Helper = 0x233C,
}

public enum AID : uint
{
    TheFixer = 46228,
    SerpentineScourge = 47547,
    RavenousReach = 46189,
    Burst = 46191,
    VisceralBurst = 46227,
    Splattershed = 47557,
    BringDownTheHouse = 46205,
    SplitScourge = 46207,
    VenomousScourge = 46208,
    GrandEntrance1 = 46202,
    GrandEntrance2 = 46203,
    MindlessFlesh1 = 48090,
    MindlessFlesh2 = 48091,
    MindlessFlesh3 = 48092,
    MindlessFlesh4 = 48093,
    MindlessFlesh5 = 48094,
    MindlessFleshBig = 48095,
    Constrictor = 46199,
    CruelCoil = 45339,
    DramaticLysis = 46211,
    FourthWallFusion = 46212,
    HemorrhagicProjection = 46213,
}

sealed class TheFixer(BossModule module) : Components.RaidwideCast(module, AID.TheFixer);
sealed class SerpentineScourge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SerpentineScourge, new AOEShapeRect(30f, 10f));
sealed class RavenousReach(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RavenousReach, new AOEShapeCone(35f, 60f.Degrees()));
sealed class Burst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Burst, 12f);
sealed class VisceralBurst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VisceralBurst, 6f);
sealed class Splattershed(BossModule module) : Components.RaidwideCast(module, AID.Splattershed);
sealed class BringDownTheHouse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BringDownTheHouse, new AOEShapeRect(15f, 5f));
sealed class SplitScourge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SplitScourge, new AOEShapeRect(30f, 5f));
sealed class VenomousScourge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VenomousScourge, 5f);
sealed class GrandEntrance(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GrandEntrance1, (uint)AID.GrandEntrance2], 2f);
sealed class MindlessFlesh(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.MindlessFlesh1, (uint)AID.MindlessFlesh2, (uint)AID.MindlessFlesh3, (uint)AID.MindlessFlesh4, (uint)AID.MindlessFlesh5], new AOEShapeRect(30f, 4f));
sealed class MindlessFleshBig(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MindlessFleshBig, new AOEShapeRect(30f, 17.5f));
sealed class FleshTele(BossModule module) : Components.CastTowers(module, AID.Constrictor, 13f);
sealed class CruelCoil(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CruelCoil, 13f);
sealed class BurstingGrotesquerie(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DramaticLysis, 6f);
sealed class SharedGrotesquerie(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FourthWallFusion, 6f);
sealed class DirectedGrotesquerie(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HemorrhagicProjection, new AOEShapeCone(60f, 30f.Degrees()));
sealed class Maelstrom(BossModule module) : Components.Adds(module, (uint)OID.Maelstrom);

sealed class M12NLindwurmStates : StateMachineBuilder
{
    public M12NLindwurmStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheFixer>()
            .ActivateOnEnter<SerpentineScourge>()
            .ActivateOnEnter<RavenousReach>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<VisceralBurst>()
            .ActivateOnEnter<Splattershed>()
            .ActivateOnEnter<BringDownTheHouse>()
            .ActivateOnEnter<SplitScourge>()
            .ActivateOnEnter<VenomousScourge>()
            .ActivateOnEnter<GrandEntrance>()
            .ActivateOnEnter<MindlessFlesh>()
            .ActivateOnEnter<MindlessFleshBig>()
            .ActivateOnEnter<FleshTele>()
            .ActivateOnEnter<CruelCoil>()
            .ActivateOnEnter<BurstingGrotesquerie>()
            .ActivateOnEnter<SharedGrotesquerie>()
            .ActivateOnEnter<DirectedGrotesquerie>()
            .ActivateOnEnter<Maelstrom>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(M12NLindwurmStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), PrimaryActorOID = (uint)OID.Lindwurm, Contributors = "The Combat Reborn Team (Malediktus), CN merge", Expansion = BossModuleInfo.Expansion.Dawntrail, Category = BossModuleInfo.Category.Raid, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1074u, NameID = 14378u, SortOrder = 1, PlanLevel = 0)]
public sealed class M12NLindwurm(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsRect(20f, 15f));
