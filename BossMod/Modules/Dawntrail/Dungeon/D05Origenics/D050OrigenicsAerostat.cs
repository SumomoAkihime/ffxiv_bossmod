namespace BossMod.Dawntrail.Dungeon.D05Origenics.D050OrigenicsAerostat;

public enum OID : uint
{
    Boss = 0x41E2,
    Aerostat2 = 0x42BA,
    OrigenicsSentryS9 = 0x43D6,
    OrigenicsSentryS92 = 0x4189,
    OrigenicsSentryG10 = 0x43D7
}

public enum AID : uint
{
    IncendiaryCircle = 38328,
    GrenadoShot = 35428
}

sealed class IncendiaryCircle(BossModule module) : Components.StandardAOEs(module, AID.IncendiaryCircle, new AOEShapeDonut(3, 12));
sealed class GrenadoShot(BossModule module) : Components.StandardAOEs(module, AID.GrenadoShot, new AOEShapeCircle(5));

sealed class D050OrigenicsAerostatStates : StateMachineBuilder
{
    public D050OrigenicsAerostatStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IncendiaryCircle>()
            .ActivateOnEnter<GrenadoShot>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 825, NameID = 12895, SortOrder = 2)]
public sealed class D050OrigenicsAerostat(WorldState ws, Actor primary) : BossModule(ws, primary, new(-116, -80), new ArenaBoundsSquare(60))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.Aerostat2), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.OrigenicsSentryS9), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.OrigenicsSentryS92), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.OrigenicsSentryG10), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = 0;
    }
}
