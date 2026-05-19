namespace BossMod.Dawntrail.Alliance.A20Trash;

public enum OID2 : uint
{
    JaggedyEaredJack = 0x4875,
    ForestFunguar = 0x487A,
    ScarabBeetle = 0x4879,
    GoblinThug = 0x4877,
    ForestHare = 0x4876,
    OrcishGrappler = 0x4874,
    GoblinFisher = 0x4878
}

public enum AID2 : uint
{
    DustCloud = 43554
}

sealed class DustCloud(BossModule module) : Components.SimpleAOEs(module, (uint)AID2.DustCloud, new AOEShapeCone(10f, 60f.Degrees()));

sealed class A20JaggedyEaredJackStates : StateMachineBuilder
{
    public A20JaggedyEaredJackStates(A20JaggedyEaredJack module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DustCloud>()
            .Raw.Update = () => module.Enemies(OID2.JaggedyEaredJack).All(e => e.IsDeadOrDestroyed)
                && module.Enemies(OID2.ForestFunguar).All(e => e.IsDeadOrDestroyed)
                && module.Enemies(OID2.ScarabBeetle).All(e => e.IsDeadOrDestroyed)
                && module.Enemies(OID2.GoblinThug).All(e => e.IsDeadOrDestroyed)
                && module.Enemies(OID2.ForestHare).All(e => e.IsDeadOrDestroyed)
                && module.Enemies(OID2.OrcishGrappler).All(e => e.IsDeadOrDestroyed)
                && module.Enemies(OID2.GoblinFisher).All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team (Malediktus), adapted by Codex", PrimaryActorOID = (uint)OID2.JaggedyEaredJack, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058u, NameID = 14080u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 1)]
public sealed class A20JaggedyEaredJack(WorldState ws, Actor primary) : BossModule(ws, primary, new(611f, 804f), arena)
{
    private static readonly ArenaBoundsSquare arena = new(35f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID2.JaggedyEaredJack), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID2.ForestFunguar), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID2.ScarabBeetle), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID2.GoblinThug), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID2.ForestHare), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID2.OrcishGrappler), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID2.GoblinFisher), ArenaColor.Enemy);
    }
}
