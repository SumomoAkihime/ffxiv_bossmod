namespace BossMod.Dawntrail.Alliance.A10Groundskeeper;

public enum OID : uint
{
    Boss = 0x4691,
    Groundskeeper = 0x4712,
    Sprinkler = 0x4690
}

public enum AID : uint
{
    AutoAttack = 872,
    IsleDrop = 41670,
    MysteriousLight = 41667,
    DoubleRay = 41668
}

sealed class IsleDrop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IsleDrop, new AOEShapeCircle(6f));
sealed class MysteriousLight(BossModule module) : Components.CastGaze(module, AID.MysteriousLight);

public sealed class A10GroundskeeperStates : StateMachineBuilder
{
    public A10GroundskeeperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IsleDrop>()
            .ActivateOnEnter<MysteriousLight>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13607, SortOrder = 5)]
public sealed class A10Groundskeeper(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly WPos ArenaCenter = new(-550, -605);
    public static readonly ArenaBoundsCircle DefaultBounds = new(24);
}
