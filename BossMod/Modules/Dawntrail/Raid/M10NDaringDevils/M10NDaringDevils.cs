namespace BossMod.Dawntrail.Raid.M10NDaringDevils;

public enum OID : uint
{
    RedHot = 0x4B53,
    DeepBlue = 0x4B54,
    XtremeAether = 0x4B55,
    TheXtremes = 0x4BDE,
    Helper = 0x233C,
}

public enum AID : uint
{
    HotImpact = 46464,
    DeepImpact = 46465,
    DiversDare = 46466,
    DiversDareBlue = 46467,
    AlleyOopInferno1 = 46471,
    Pyrotation1 = 46473,
    HotAerial2 = 46476,
    CutbackBlaze1 = 46479,
    SickSwell1 = 46481,
    SickestTakeOff1 = 46483,
    DeepVarial1 = 46488,
    AlleyOopMaelstrom = 46495,
    AlleyOopMaelstrom2 = 46496,
    XtremeSpectacular2 = 46499,
    XtremeSpectacular3 = 46555,
    XtremeSpectacular4 = 47049,
    SteamBurst = 46507,
    BlastingSnap1 = 46505,
    PlungingSnap1 = 46506,
}

public enum IconID : uint
{
    FireStack = 659,
}

sealed class HotImpact(BossModule module) : Components.CastSharedTankbuster(module, AID.HotImpact, 6f);
sealed class DeepImpact(BossModule module) : Components.BaitAwayCast(module, AID.DeepImpact, new AOEShapeCircle(6f), centerAtTarget: true, endsOnCastEvent: true);
sealed class DiversDare(BossModule module) : Components.RaidwideCast(module, AID.DiversDare);
sealed class DiversDareBlue(BossModule module) : Components.RaidwideCast(module, AID.DiversDareBlue);
sealed class CutbackBlaze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CutbackBlaze1, new AOEShapeCone(60f, 90f.Degrees()));
sealed class AlleyOopInferno(BossModule module) : Components.SpreadFromCastTargets(module, AID.AlleyOopInferno1, 5f);
sealed class AlleyOopMaelstrom(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AlleyOopMaelstrom, (uint)AID.AlleyOopMaelstrom2], new AOEShapeCone(60f, 30f.Degrees()));
sealed class PyrotationStack(BossModule module) : Components.StackWithIcon(module, (uint)IconID.FireStack, AID.Pyrotation1, 6f, 5f, minStackSize: 2, maxStackSize: 8);
sealed class HotAerial(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HotAerial2, 6f);
sealed class DeepVarialCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeepVarial1, new AOEShapeCone(60f, 60f.Degrees()));
sealed class SickestTakeOff(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SickestTakeOff1, new AOEShapeRect(50f, 7.5f));
sealed class SickSwellKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.SickSwell1, 10f, kind: Components.SimpleKnockbacks.Kind.DirForward, stopAtWall: false);
sealed class SteamBurst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SteamBurst, 9f);
sealed class XtremeSpectacularEdge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.XtremeSpectacular2, new AOEShapeRect(50f, 18f));
sealed class XtremeSpectacularRaidwide(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.XtremeSpectacular2, (uint)AID.XtremeSpectacular3, (uint)AID.XtremeSpectacular4], 60f);
sealed class InsaneAirSnaps(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BlastingSnap1, (uint)AID.PlungingSnap1], new AOEShapeCone(60f, 15f.Degrees()));

sealed class M10NDaringDevilsStates : StateMachineBuilder
{
    public M10NDaringDevilsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HotImpact>()
            .ActivateOnEnter<DeepImpact>()
            .ActivateOnEnter<DiversDare>()
            .ActivateOnEnter<DiversDareBlue>()
            .ActivateOnEnter<CutbackBlaze>()
            .ActivateOnEnter<AlleyOopInferno>()
            .ActivateOnEnter<AlleyOopMaelstrom>()
            .ActivateOnEnter<PyrotationStack>()
            .ActivateOnEnter<HotAerial>()
            .ActivateOnEnter<DeepVarialCone>()
            .ActivateOnEnter<SickestTakeOff>()
            .ActivateOnEnter<SickSwellKB>()
            .ActivateOnEnter<SteamBurst>()
            .ActivateOnEnter<XtremeSpectacularEdge>()
            .ActivateOnEnter<XtremeSpectacularRaidwide>()
            .ActivateOnEnter<InsaneAirSnaps>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(M10NDaringDevilsStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), IconIDType = typeof(IconID), PrimaryActorOID = (uint)OID.RedHot, Contributors = "The Combat Reborn Team (Malediktus), CN merge", Expansion = BossModuleInfo.Expansion.Dawntrail, Category = BossModuleInfo.Category.Raid, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1070u, NameID = 14370u, SortOrder = 1, PlanLevel = 0)]
public sealed class M10NDaringDevils(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f))
{
    public Actor? DeepBlue { get; private set; }

    protected override void UpdateModule()
    {
        DeepBlue ??= Enemies((uint)OID.DeepBlue).FirstOrDefault();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        if (DeepBlue != null)
            Arena.Actor(DeepBlue, ArenaColor.Enemy);
    }
}
