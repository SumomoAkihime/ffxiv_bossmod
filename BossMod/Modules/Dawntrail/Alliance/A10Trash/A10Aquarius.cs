namespace BossMod.Dawntrail.Alliance.A10Aquarius;

public enum OID : uint
{
    Boss = 0x468F,
    ElderGobbue = 0x468D,
    RobberCrab1 = 0x468E,
    RobberCrab2 = 0x4711,
    DeathCap = 0x468C,
    BarkSpider1 = 0x468B,
    BarkSpider2 = 0x4710,
    Skimmer1 = 0x468A,
    Skimmer2 = 0x470F
}

public enum AID : uint
{
    AutoAttack1 = 872,
    AutoAttack2 = 870,
    FrogKick = 41660,
    BubbleShower = 41664,
    Scoop = 41663,
    CursedSphere = 41656,
    WaterIII = 41666,
    Beatdown = 41662,
    SpiderWeb = 41659,
    HundredFists = 40648,
    Agaricus = 41661
}

sealed class CursedSphere(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CursedSphere, new AOEShapeCircle(3f));
sealed class WaterIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WaterIII, new AOEShapeCircle(7f));
sealed class BubbleShower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BubbleShower, new AOEShapeCone(6f, 30f.Degrees()));
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));
sealed class Agaricus(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Agaricus, new AOEShapeCircle(5f));
sealed class Beatdown(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Beatdown, new AOEShapeRect(9f, 1.5f));
sealed class SpiderWeb(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpiderWeb, new AOEShapeCircle(6f));
sealed class HundredFists(BossModule module) : Components.CastInterruptHint(module, AID.HundredFists, showNameInHint: true);

public sealed class A10AquariusStates : StateMachineBuilder
{
    public A10AquariusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WaterIII>()
            .ActivateOnEnter<CursedSphere>()
            .ActivateOnEnter<BubbleShower>()
            .ActivateOnEnter<Scoop>()
            .ActivateOnEnter<Beatdown>()
            .ActivateOnEnter<SpiderWeb>()
            .ActivateOnEnter<HundredFists>()
            .ActivateOnEnter<Agaricus>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13605, SortOrder = 3)]
public sealed class A10Aquarius(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly WPos ArenaCenter = new(-460, 750);
    public static readonly ArenaBoundsCircle DefaultBounds = new(40);
}
