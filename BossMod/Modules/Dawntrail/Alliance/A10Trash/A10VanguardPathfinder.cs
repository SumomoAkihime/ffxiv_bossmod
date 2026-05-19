namespace BossMod.Dawntrail.Alliance.A10Trash.VanguardPathfinder;

public enum OID : uint
{
    Boss = 0x4689,
    VanguardsSlime1 = 0x470E,
    VanguardsSlime2 = 0x4688,
    GoblinReplica = 0x4687
}

public enum AID : uint
{
    AutoAttack1 = 872,
    AutoAttack2 = 870,
    Seismostomp = 41652,
    BombToss = 41655,
    GoblinRush = 41654
}

sealed class BombToss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BombToss, 3f);
sealed class Seismostomp(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Seismostomp, 5f);

public sealed class A10VanguardPathfinderStates : StateMachineBuilder
{
    public A10VanguardPathfinderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Seismostomp>()
            .ActivateOnEnter<BombToss>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13599, SortOrder = 1)]
public sealed class A10VanguardPathfinder(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly WPos ArenaCenter = new(800, 645);
    public static readonly ArenaBoundsCircle DefaultBounds = new(24);
}
