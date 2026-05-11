#pragma warning disable CA1707

namespace BossMod.Dawntrail.Trial.T08Enuo;

public enum OID : uint
{
    Enuo = 0x4DB9,
    YawningVoid = 0x4DBA,
    Helper = 0x233C,
    LoomingShadow = 0x4DBC,
    UncastShadow = 0x4DBD,
    BeaconInTheDark = 0x4DBE,
    Zero = 0x4DBF,
}

public enum AID : uint
{
    Meteorain = 49971,
    NaughtGrowsAOE = 49933,
    NaughtHunts = 49939,
    EndlessChaseFirst = 48474,
    EndlessChaseRest = 49940,
    GazeOfTheVoid1 = 49952,
    GazeOfTheVoidCones = 49953,
    DeepFreeze = 49966,
    ShroudedHolyStack = 49968,
    MeltdownAOE = 49963,
    MeltdownSpread = 49964,
    NaughtHuntsAnother = 49941,
    SilentTorrentSmall = 49946,
    SilentTorrentMedium = 49947,
    SilentTorrentLarge = 49948,
    VacuumCircle = 49949,
    LoomingEmptinessAOE = 49981,
    EmptyShadowAOE = 50667,
    Nothingness = 49958,
    LightlessWorld = 49959,
    Almagest = 49928,
    NaughtGrowsAOESmall = 49934,
    DimensionZeroRect = 49969,
}

public enum IconID : uint
{
    DeepFreezeFlareIcon = 327,
    EndlessChaseIcon = 172,
    ShroudedHolyStackIcon = 161,
    DimensionZeroIcon = 719,
}

public enum TetherID : uint
{
    NaughtHuntsAnotherTether = 405,
}

sealed class Meteorain(BossModule module) : Components.RaidwideCast(module, AID.Meteorain);
sealed class NaughtGrowsAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NaughtGrowsAOE, new AOEShapeCircle(40f));
sealed class NaughtHunts(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(6f), AID.EndlessChaseFirst, AID.EndlessChaseRest, 2.9f, 0.7f, 13);
sealed class NaughtHuntsJumps(BossModule module) : BossComponent(module)
{
    readonly List<(Actor Source, Actor Target)> _jumps = [];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID != TetherID.NaughtHuntsAnotherTether || WorldState.Actors.Find(tether.Target) is not { } target)
            return;

        _jumps.RemoveAll(j => j.Source == source);
        _jumps.Add((source, target));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.NaughtHuntsAnotherTether)
            _jumps.RemoveAll(j => j.Source == source);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        _jumps.RemoveAll(j => j.Source == actor || j.Target == actor);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (source, target) in _jumps)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(source.Position, target.Position, 0xFF000000, 2);
            Arena.AddLine(source.Position, target.Position, ArenaColor.Danger);
        }
    }
}
sealed class GazeOfTheVoidCones(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GazeOfTheVoidCones, (uint)AID.GazeOfTheVoid1], new AOEShapeCone(20f, 22.5f.Degrees()), 10);
sealed class DeepFreeze(BossModule module) : Components.BaitAwayCast(module, AID.DeepFreeze, new AOEShapeCircle(8f), true, true);
sealed class ShroudedHoly(BossModule module) : Components.StackWithCastTargets(module, AID.ShroudedHolyStack, 7f, minStackSize: 4);
sealed class MeltdownSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.MeltdownSpread, 5f);
sealed class MeltdownAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeltdownAOE, new AOEShapeCircle(5f));
sealed class SilentTorrentSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SilentTorrentSmall, new AOEShapeDonutSector(17f, 19f, 10f.Degrees()));
sealed class SilentTorrentMedium(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SilentTorrentMedium, new AOEShapeDonutSector(17f, 19f, 20f.Degrees()));
sealed class SilentTorrentLarge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SilentTorrentLarge, new AOEShapeDonutSector(17f, 19f, 30f.Degrees()));
sealed class VacuumCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VacuumCircle, new AOEShapeCircle(7f));
sealed class LoomingShadow(BossModule module) : Components.Adds(module, (uint)OID.LoomingShadow);
sealed class Shadows(BossModule module) : Components.Adds(module, (uint)OID.UncastShadow, 1);
sealed class Beacon(BossModule module) : Components.Adds(module, (uint)OID.BeaconInTheDark, 1, true);
sealed class EmptyShadow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EmptyShadowAOE, new AOEShapeCircle(10f));
sealed class LoomingEmptinessAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LoomingEmptinessAOE, 42f);
sealed class Nothingness(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Nothingness, new AOEShapeRect(100f, 2f));
sealed class LightlessWorld(BossModule module) : Components.CastLineOfSightAOE(module, AID.LightlessWorld, 40f, false)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies((uint)OID.Zero);
}
sealed class Almagest(BossModule module) : Components.RaidwideCast(module, AID.Almagest);
sealed class NaughtGrows(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NaughtGrowsAOESmall, 12f);
sealed class DimensionZero(BossModule module) : Components.IconLineStack(module, 4f, 60f, (uint)IconID.DimensionZeroIcon, AID.DimensionZeroRect, 5f);

sealed class ArenaSwitcher : BossComponent
{
    public Actor? BeaconActor;

    public ArenaSwitcher(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BeaconInTheDark)
            BeaconActor = actor;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0 && state == 0x00020001)
            Arena.Bounds = new ArenaBoundsSquare(80f);

        if (index == 0 && state == 0x00080004)
            Arena.Bounds = new ArenaBoundsCircle(20f);
    }
}

sealed class T08EnuoStates : StateMachineBuilder
{
    public T08EnuoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Meteorain>()
            .ActivateOnEnter<NaughtGrowsAOE>()
            .ActivateOnEnter<NaughtHunts>()
            .ActivateOnEnter<NaughtHuntsJumps>()
            .ActivateOnEnter<GazeOfTheVoidCones>()
            .ActivateOnEnter<DeepFreeze>()
            .ActivateOnEnter<ShroudedHoly>()
            .ActivateOnEnter<MeltdownSpread>()
            .ActivateOnEnter<MeltdownAOE>()
            .ActivateOnEnter<SilentTorrentSmall>()
            .ActivateOnEnter<SilentTorrentMedium>()
            .ActivateOnEnter<SilentTorrentLarge>()
            .ActivateOnEnter<VacuumCircle>()
            .ActivateOnEnter<ArenaSwitcher>()
            .ActivateOnEnter<LoomingShadow>()
            .ActivateOnEnter<Shadows>()
            .ActivateOnEnter<Beacon>()
            .ActivateOnEnter<EmptyShadow>()
            .ActivateOnEnter<LoomingEmptinessAOE>()
            .ActivateOnEnter<Nothingness>()
            .ActivateOnEnter<LightlessWorld>()
            .ActivateOnEnter<NaughtGrows>()
            .ActivateOnEnter<Almagest>()
            .ActivateOnEnter<DimensionZero>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(T08EnuoStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), IconIDType = typeof(IconID), PrimaryActorOID = (uint)OID.Enuo, Contributors = "Wen", Expansion = BossModuleInfo.Expansion.Dawntrail, Category = BossModuleInfo.Category.Trial, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1115u, NameID = 14749u, SortOrder = 1, PlanLevel = 0)]
public sealed class T08Enuo(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.LoomingShadow), ArenaColor.Enemy);
    }
}
