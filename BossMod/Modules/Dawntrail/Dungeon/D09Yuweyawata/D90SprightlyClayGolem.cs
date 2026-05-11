namespace BossMod.Dawntrail.Dungeon.D09YuweyawataFieldStation.D90SprightlyClayGolem;

public enum OID : uint { Boss = 0x461B, SprightlyStone1 = 0x46B1, SprightlyStone2 = 0x461D, SprightlyDhara = 0x4618 }
public enum AID : uint { WildHorn = 40675, Plummet = 40676 }

sealed class WildHorn(BossModule module) : Components.StandardAOEs(module, AID.WildHorn, new AOEShapeCone(15, 60.Degrees()));
sealed class Plummet(BossModule module) : Components.StandardAOEs(module, AID.Plummet, new AOEShapeCircle(10));

sealed class D90SprightlyClayGolemStates : StateMachineBuilder
{
    public D90SprightlyClayGolemStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WildHorn>()
            .ActivateOnEnter<Plummet>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008, NameID = 13582, SortOrder = 8)]
public sealed class D90SprightlyClayGolem(WorldState ws, Actor primary) : BossModule(ws, primary, new(114, -365), new ArenaBoundsSquare(55))
{
}

