namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D040VanguardSentryR7;

public enum OID : uint
{
    Boss = 0x4479,
    SentryR7 = 0x41D8
}

public enum AID : uint
{
    Swoop = 38051,
    FloaterTurn = 38451,
    SpinningAxle = 39018
}

sealed class Swoop(BossModule module) : Components.ChargeAOEs(module, AID.Swoop, 2.5f);
sealed class FloaterTurn(BossModule module) : Components.StandardAOEs(module, AID.FloaterTurn, new AOEShapeDonut(4, 10));
sealed class SpinningAxle(BossModule module) : Components.StandardAOEs(module, AID.SpinningAxle, new AOEShapeCircle(6));

sealed class D040VanguardSentryR7States : StateMachineBuilder
{
    public D040VanguardSentryR7States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Swoop>()
            .ActivateOnEnter<FloaterTurn>()
            .ActivateOnEnter<SpinningAxle>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team, CN compatibility port", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12778, SortOrder = 2)]
public sealed class D040VanguardSentryR7(WorldState ws, Actor primary) : BossModule(ws, primary, new(-84, 301), new ArenaBoundsSquare(40))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.SentryR7), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = 0;
    }
}
