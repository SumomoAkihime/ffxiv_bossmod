namespace BossMod.Dawntrail.Dungeon.D09YuweyawataFieldStation.D90ForestWoolback;

public enum OID : uint { Boss = 0x46A4, ForestAxeBeak = 0x4611, ForestWoolback = 0x4613, Electrogolem = 0x46A3 }
public enum AID : uint { SweepingGouge = 40668, Thunderball = 40666 }

sealed class SweepingGouge(BossModule module) : Components.StandardAOEs(module, AID.SweepingGouge, new AOEShapeCone(9, 45.Degrees()));
sealed class Thunderball(BossModule module) : Components.StandardAOEs(module, AID.Thunderball, new AOEShapeCircle(8));

sealed class D90ForestWoolbackStates : StateMachineBuilder
{
    public D90ForestWoolbackStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Thunderball>()
            .ActivateOnEnter<SweepingGouge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008, NameID = 13573, SortOrder = 2)]
public sealed class D90ForestWoolback(WorldState ws, Actor primary) : BossModule(ws, primary, new(20, 450), new ArenaBoundsSquare(55))
{
}

