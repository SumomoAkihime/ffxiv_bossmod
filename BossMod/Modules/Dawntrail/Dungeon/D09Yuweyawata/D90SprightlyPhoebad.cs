namespace BossMod.Dawntrail.Dungeon.D09YuweyawataFieldStation.D90SprightlyPhoebad;

public enum OID : uint { Boss = 0x4619, SprightlyStone = 0x461D, SprightlyMole = 0x461A, SprightlyLoamkeep = 0x461C }
public enum AID : uint { Plummet = 40676, Landslip = 41118 }

sealed class Landslip(BossModule module) : Components.StandardAOEs(module, AID.Landslip, new AOEShapeCone(12, 60.Degrees()));
sealed class Plummet(BossModule module) : Components.StandardAOEs(module, AID.Plummet, new AOEShapeCircle(10));

sealed class D90SprightlyPhoebadStates : StateMachineBuilder
{
    public D90SprightlyPhoebadStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Landslip>()
            .ActivateOnEnter<Plummet>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008, NameID = 13580, SortOrder = 7)]
public sealed class D90SprightlyPhoebad(WorldState ws, Actor primary) : BossModule(ws, primary, new(106, -250), new ArenaBoundsSquare(55))
{
}

