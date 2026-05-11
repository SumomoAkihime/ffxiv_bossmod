namespace BossMod.Dawntrail.Dungeon.D09YuweyawataFieldStation.D90RottenResearcher;

public enum OID : uint { Boss = 0x4652, RottenResearcher1 = 0x4654, RottenResearcher2 = 0x4615, RottenResearcher3 = 0x4653, RottenHound1 = 0x46D4, RottenHound2 = 0x4614 }

sealed class D90RottenResearcherStates : StateMachineBuilder
{
    public D90RottenResearcherStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008, NameID = 13575, SortOrder = 4)]
public sealed class D90RottenResearcher(WorldState ws, Actor primary) : BossModule(ws, primary, new(55, 95), new ArenaBoundsSquare(48))
{
}

