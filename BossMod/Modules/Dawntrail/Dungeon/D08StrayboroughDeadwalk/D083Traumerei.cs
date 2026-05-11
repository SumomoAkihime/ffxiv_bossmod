namespace BossMod.Dawntrail.Dungeon.D08StrayboroughDeadwalk.D083Traumerei;

public enum OID : uint
{
    Boss = 0x421F,
    StrayGeist = 0x4221,
    StrayPhantagenitrix = 0x4220,
    Helper = 0x233C
}

public enum AID : uint
{
    BitterRegret1 = 37139,
    BitterRegret2 = 37147,
    BitterRegret3 = 37340,
    Impact = 37133,
    IllIntent = 39607,
    MaliciousMistTether = 37138,
    Ghostduster = 37146,
    MaliciousMistRaidwide = 37168,
    Fleshbuster = 37148,
    GhostcrusherMarker = 37144,
    Ghostcrusher = 37143
}

class MaliciousMistRaidwide(BossModule module) : Components.RaidwideCast(module, AID.MaliciousMistRaidwide);
class IllIntentMaliciousMist(BossModule module) : Components.SingleTargetCast(module, AID.IllIntent);
class BitterRegret1(BossModule module) : Components.StandardAOEs(module, AID.BitterRegret1, new AOEShapeRect(50, 8));
class BitterRegret2(BossModule module) : Components.StandardAOEs(module, AID.BitterRegret2, new AOEShapeRect(50, 6));
class BitterRegret3(BossModule module) : Components.StandardAOEs(module, AID.BitterRegret3, new AOEShapeRect(40, 2));
class Impact(BossModule module) : Components.StandardAOEs(module, AID.Impact, new AOEShapeRect(40, 2));
class Ghostduster(BossModule module) : Components.SpreadFromCastTargets(module, AID.Ghostduster, 8);
class Ghostcrusher(BossModule module) : Components.StackWithCastTargets(module, AID.Ghostcrusher, 4, 4, 4);
class Fleshbuster(BossModule module) : Components.RaidwideCast(module, AID.Fleshbuster);

class D083TraumereiStates : StateMachineBuilder
{
    public D083TraumereiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Ghostcrusher>()
            .ActivateOnEnter<MaliciousMistRaidwide>()
            .ActivateOnEnter<IllIntentMaliciousMist>()
            .ActivateOnEnter<Ghostduster>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<BitterRegret1>()
            .ActivateOnEnter<BitterRegret2>()
            .ActivateOnEnter<BitterRegret3>()
            .ActivateOnEnter<Fleshbuster>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 981, NameID = 12763)]
public class D083Traumerei(WorldState ws, Actor primary) : BossModule(ws, primary, new(148, -433), new ArenaBoundsSquare(19.5f));
