namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D040VanguardAerostat1;

public enum OID : uint
{
    Boss = 0x41DA,
    Aerostat2 = 0x447B
}

public enum AID : uint
{
    IncendiaryRing = 38452
}

sealed class IncendiaryRing(BossModule module) : Components.StandardAOEs(module, AID.IncendiaryRing, new AOEShapeDonut(3, 12));

sealed class D040VanguardAerostat1States : StateMachineBuilder
{
    public D040VanguardAerostat1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IncendiaryRing>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12780, SortOrder = 4)]
public sealed class D040VanguardAerostat1(WorldState ws, Actor primary) : BossModule(ws, primary, new(-50, -15), new ArenaBoundsRect(7.7f, 25))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.Aerostat2), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = 0;
    }
}
