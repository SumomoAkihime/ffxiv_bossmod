namespace BossMod.Dawntrail.Dungeon.D07TenderValley.D073YokHuyAttestant;

public enum OID : uint { Boss = 0x4253, Helper = 0x233C }
public enum AID : uint
{
    TectonicShift = 39221,
    AncientWrathLong = 39825,
    AncientWrathShort = 39823,
    AncientWrathMedium = 39824,
    BoulderToss = 38540,
    SunToss = 38539
}

class TectonicShift(BossModule module) : Components.StandardAOEs(module, AID.TectonicShift, new AOEShapeCircle(8));
class BoulderToss(BossModule module) : Components.SingleTargetCast(module, AID.BoulderToss);
class SunToss(BossModule module) : Components.StandardAOEs(module, AID.SunToss, new AOEShapeCircle(6));
class AncientWrathLong(BossModule module) : Components.StandardAOEs(module, AID.AncientWrathLong, new AOEShapeRect(35, 4));
class AncientWrathMedium(BossModule module) : Components.StandardAOEs(module, AID.AncientWrathMedium, new AOEShapeRect(22, 4));
class AncientWrathShort(BossModule module) : Components.StandardAOEs(module, AID.AncientWrathShort, new AOEShapeRect(12, 4));

class D073YokHuyAttestantStates : StateMachineBuilder
{
    public D073YokHuyAttestantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AncientWrathLong>()
            .ActivateOnEnter<AncientWrathMedium>()
            .ActivateOnEnter<AncientWrathShort>()
            .ActivateOnEnter<BoulderToss>()
            .ActivateOnEnter<SunToss>()
            .ActivateOnEnter<TectonicShift>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12801)]
public class D073YokHuyAttestant(WorldState ws, Actor primary) : BossModule(ws, primary, new(-130, -475), new ArenaBoundsRect(17.9f, 22.5f));
