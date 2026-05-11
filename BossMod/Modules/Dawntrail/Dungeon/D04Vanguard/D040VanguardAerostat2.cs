namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D040VanguardAerostat2;

public enum OID : uint
{
    Boss = 0x43D4,
    Turret = 0x41DB,
    SentryR7 = 0x41D6,
    SentryS7 = 0x4478
}

public enum AID : uint
{
    IncendiaryRing = 38452,
    Electrobeam = 38060,
    SpreadShot = 39017
}

sealed class IncendiaryRing(BossModule module) : Components.StandardAOEs(module, AID.IncendiaryRing, new AOEShapeDonut(3, 12));
sealed class Electrobeam(BossModule module) : Components.StandardAOEs(module, AID.Electrobeam, new AOEShapeRect(50, 2));
sealed class SpreadShot(BossModule module) : Components.StandardAOEs(module, AID.SpreadShot, new AOEShapeCone(12, 45.Degrees()));

sealed class D040VanguardAerostat2States : StateMachineBuilder
{
    public D040VanguardAerostat2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IncendiaryRing>()
            .ActivateOnEnter<Electrobeam>()
            .ActivateOnEnter<SpreadShot>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12780, SortOrder = 6)]
public sealed class D040VanguardAerostat2(WorldState ws, Actor primary) : BossModule(ws, primary, new(41, -299), new ArenaBoundsSquare(60))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.Turret), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.SentryR7), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.SentryS7), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = 0;
    }
}
