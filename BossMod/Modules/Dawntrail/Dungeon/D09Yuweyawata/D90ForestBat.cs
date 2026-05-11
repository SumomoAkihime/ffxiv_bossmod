namespace BossMod.Dawntrail.Dungeon.D09YuweyawataFieldStation.D90ForestBat;

public enum OID : uint { Boss = 0x4612, ForestWoolback = 0x4613, Electrogolem1 = 0x46A3, Electrogolem2 = 0x4610 }
public enum AID : uint { SweepingGouge = 40668, LineVoltage = 40665 }

sealed class SweepingGouge(BossModule module) : Components.StandardAOEs(module, AID.SweepingGouge, new AOEShapeCone(9, 45.Degrees()));
sealed class LineVoltage(BossModule module) : Components.StandardAOEs(module, AID.LineVoltage, new AOEShapeRect(14, 2));

sealed class D90ForestBatStates : StateMachineBuilder
{
    public D90ForestBatStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SweepingGouge>()
            .ActivateOnEnter<LineVoltage>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008, NameID = 13572, SortOrder = 1)]
public sealed class D90ForestBat(WorldState ws, Actor primary) : BossModule(ws, primary, new(-15, 508), new ArenaBoundsSquare(45))
{
}

