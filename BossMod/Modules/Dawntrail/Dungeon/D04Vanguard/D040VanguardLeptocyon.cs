namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D040VanguardLeptocyon;

public enum OID : uint
{
    Boss = 0x447A,
    VanguardLeptocyon = 0x41D9,
    VanguardSentryS7 = 0x41D6,
    VanguardSentryG7 = 0x4478
}

public enum AID : uint
{
    SpreadShot = 39017
}

sealed class SpreadShot(BossModule module) : Components.StandardAOEs(module, AID.SpreadShot, new AOEShapeCone(12, 45.Degrees()));

sealed class D040VanguardLeptocyonStates : StateMachineBuilder
{
    public D040VanguardLeptocyonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpreadShot>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12778, SortOrder = 1)]
public sealed class D040VanguardLeptocyon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-4, 410), new ArenaBoundsSquare(55))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.VanguardLeptocyon), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.VanguardSentryS7), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.VanguardSentryG7), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = 0;
    }
}
