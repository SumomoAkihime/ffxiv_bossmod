namespace BossMod.Heavensward.Quest.MSQ.ASpectacleForTheAges;

public enum OID : uint
{
    Boss = 0x154E,
    Tizona = 0x1552
}

public enum AID : uint
{
    FlamingTizona = 5763,
    TheCurse = 5765,
}

class FlamingTizona(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlamingTizona, 6f);
class TheCurse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheCurse, new AOEShapeDonutSector(2f, 7f, 90f.Degrees()));
class Demoralize(BossModule module) : Components.Voidzone(module, 4f, m => m.Enemies(0x1E9FA8).Where(e => e.EventState != 7));
class Tizona(BossModule module) : Components.Adds(module, (uint)OID.Tizona, 5);

class FlameGeneralAldynnStates : StateMachineBuilder
{
    public FlameGeneralAldynnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlamingTizona>()
            .ActivateOnEnter<TheCurse>()
            .ActivateOnEnter<Demoralize>()
            .ActivateOnEnter<Tizona>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(FlameGeneralAldynnStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67775, NameID = 4739)]
public class FlameGeneralAldynn(WorldState ws, Actor primary) : BossModule(ws, primary, new(-35.75f, -205.5f), new ArenaBoundsCircle(15f));
