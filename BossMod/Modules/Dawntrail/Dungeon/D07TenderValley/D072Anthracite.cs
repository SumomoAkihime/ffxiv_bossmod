namespace BossMod.Dawntrail.Dungeon.D07TenderValley.D072Anthracite;

public enum OID : uint { Boss = 0x41BE, Helper = 0x233C }
public enum AID : uint
{
    Anthrabomb1 = 36401,
    Anthrabomb2 = 36402,
    AnthrabombSpread = 36553,
    HotBlast1 = 36545,
    HotBlast2 = 36551,
    CarbonaceousCombustion = 36557,
    BurningCoals = 36555,
    ChimneySmack = 38468
}

class Anthrabomb1(BossModule module) : Components.StandardAOEs(module, AID.Anthrabomb1, new AOEShapeCircle(10));
class Anthrabomb2(BossModule module) : Components.StandardAOEs(module, AID.Anthrabomb2, new AOEShapeCircle(10));
class AnthrabombSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.AnthrabombSpread, 6);
class HotBlast1(BossModule module) : Components.StandardAOEs(module, AID.HotBlast1, new AOEShapeRect(40, 3));
class HotBlast2(BossModule module) : Components.StandardAOEs(module, AID.HotBlast2, new AOEShapeRect(40, 3));
class CarbonaceousCombustion(BossModule module) : Components.RaidwideCast(module, AID.CarbonaceousCombustion);
class ChimneySmack(BossModule module) : Components.SingleTargetCast(module, AID.ChimneySmack);
class BurningCoals(BossModule module) : Components.StackWithCastTargets(module, AID.BurningCoals, 6, 4, 4);

class D072AnthraciteStates : StateMachineBuilder
{
    public D072AnthraciteStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Anthrabomb1>()
            .ActivateOnEnter<Anthrabomb2>()
            .ActivateOnEnter<AnthrabombSpread>()
            .ActivateOnEnter<ChimneySmack>()
            .ActivateOnEnter<HotBlast1>()
            .ActivateOnEnter<HotBlast2>()
            .ActivateOnEnter<CarbonaceousCombustion>()
            .ActivateOnEnter<BurningCoals>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12853)]
public class D072Anthracite(WorldState ws, Actor primary) : BossModule(ws, primary, new(-130, -51), new ArenaBoundsSquare(18));
