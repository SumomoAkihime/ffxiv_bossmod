namespace BossMod.Dawntrail.Dungeon.D07TenderValley.D071Barreltender;

public enum OID : uint { Boss = 0x4234, Helper = 0x233C }
public enum AID : uint
{
    BarbedBellow = 37392,
    HeavyweightNeedles = 37386,
    BarrelBreaker = 37390,
    NeedleSuperstorm = 37389,
    NeedleStorm = 37388,
    SucculentStomp = 37391,
    PricklyRight = 39154,
    PricklyLeft = 39155,
    TenderFury = 39242
}

class HeavyweightNeedles(BossModule module) : Components.StandardAOEs(module, AID.HeavyweightNeedles, new AOEShapeCone(36, 25.Degrees()));
class NeedleSuperstorm(BossModule module) : Components.StandardAOEs(module, AID.NeedleSuperstorm, new AOEShapeCircle(11));
class NeedleStorm(BossModule module) : Components.StandardAOEs(module, AID.NeedleStorm, new AOEShapeCircle(6));
class PricklyRight(BossModule module) : Components.StandardAOEs(module, AID.PricklyRight, new AOEShapeCone(36, 135.Degrees()));
class PricklyLeft(BossModule module) : Components.StandardAOEs(module, AID.PricklyLeft, new AOEShapeCone(36, 135.Degrees()));
class SucculentStomp(BossModule module) : Components.StackWithCastTargets(module, AID.SucculentStomp, 6, 4, 4);
class BarrelBreaker(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.BarrelBreaker, 20);
class TenderFury(BossModule module) : Components.SingleTargetCast(module, AID.TenderFury);
class BarbedBellow(BossModule module) : Components.RaidwideCast(module, AID.BarbedBellow);

class D071BarreltenderStates : StateMachineBuilder
{
    public D071BarreltenderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BarrelBreaker>()
            .ActivateOnEnter<HeavyweightNeedles>()
            .ActivateOnEnter<NeedleSuperstorm>()
            .ActivateOnEnter<NeedleStorm>()
            .ActivateOnEnter<PricklyRight>()
            .ActivateOnEnter<PricklyLeft>()
            .ActivateOnEnter<TenderFury>()
            .ActivateOnEnter<SucculentStomp>()
            .ActivateOnEnter<BarbedBellow>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12889)]
public class D071Barreltender(WorldState ws, Actor primary) : BossModule(ws, primary, new(-65, 470), new ArenaBoundsSquare(24.5f));
