namespace BossMod.Dawntrail.Dungeon.D09YuweyawataFieldStation.D90StationSpecter;

public enum OID : uint { Boss = 0x4616, RottenResearcher1 = 0x4653, RottenResearcher2 = 0x4654, RottenResearcher3 = 0x4615, GiantCorse = 0x4617, StationSpecter = 0x4655 }
public enum AID : uint { GlassPunch = 40671, Catapult = 40672 }

sealed class GlassPunch(BossModule module) : Components.StandardAOEs(module, AID.GlassPunch, new AOEShapeCone(7, 60.Degrees()));
sealed class Catapult(BossModule module) : Components.StandardAOEs(module, AID.Catapult, new AOEShapeCircle(6));

sealed class D90StationSpecterStates : StateMachineBuilder
{
    public D90StationSpecterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GlassPunch>()
            .ActivateOnEnter<Catapult>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008, NameID = 13577, SortOrder = 5)]
public sealed class D90StationSpecter(WorldState ws, Actor primary) : BossModule(ws, primary, new(103, 17), new ArenaBoundsSquare(55))
{
}

