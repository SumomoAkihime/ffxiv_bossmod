namespace BossMod.Dawntrail.Alliance.A20Trash;

public enum OID : uint
{
    Detector1 = 0x4880,
    Detector2 = 0x4881
}

public enum AID : uint
{
    Electray = 43576,
    Electroswipe = 43559
}

sealed class Electray(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electray, new AOEShapeRect(40f, 2.5f));
sealed class Electroswipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electroswipe, new AOEShapeCone(50f, 60f.Degrees()));
sealed class ElectroswipeHint(BossModule module) : Components.CastInterruptHint(module, AID.Electroswipe, showNameInHint: true);

sealed class A20DetectorStates : StateMachineBuilder
{
    public A20DetectorStates(A20Detector module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<Electroswipe>()
            .ActivateOnEnter<ElectroswipeHint>()
            .Raw.Update = () => module.Enemies(OID.Detector1).All(e => e.IsDeadOrDestroyed) && module.Enemies(OID.Detector2).All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team (Malediktus), adapted by Codex", PrimaryActorOID = (uint)OID.Detector1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058u, NameID = 14072u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 3)]
public sealed class A20Detector(WorldState ws, Actor primary) : BossModule(ws, primary, new(650f, 400f), arena)
{
    private static readonly ArenaBoundsSquare arena = new(25f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.Detector1), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Detector2), ArenaColor.Enemy);
    }
}
