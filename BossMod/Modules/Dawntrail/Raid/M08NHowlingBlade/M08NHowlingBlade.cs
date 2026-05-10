namespace BossMod.Dawntrail.Raid.M08NHowlingBlade;

public enum OID : uint
{
    Boss = 0x4722,
    Helper = 0x233C,
}

public enum AID : uint
{
    ExtraplanarPursuit = 42830,
    GreatDivide = 41869,
    TargetedQuake = 41864,
    TrackingTremors = 42211,
    RavenousSaber5 = 41855,
}

public enum IconID : uint
{
    TrackingTremors = 316,
}

sealed class ExtraplanarPursuit(BossModule module) : Components.RaidwideCast(module, AID.ExtraplanarPursuit);
sealed class RavenousSaber(BossModule module) : Components.RaidwideCast(module, AID.RavenousSaber5, "Raidwide x5");
sealed class GreatDivide(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GreatDivide, new AOEShapeRect(60f, 3f));
sealed class TargetedQuake(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TargetedQuake, 4f);
sealed class TrackingTremors(BossModule module) : Components.StackWithIcon(module, (uint)IconID.TrackingTremors, AID.TrackingTremors, 6f, 5f, minStackSize: 8, maxStackSize: 8);

sealed class M08NHowlingBladeStates : StateMachineBuilder
{
    public M08NHowlingBladeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ExtraplanarPursuit>()
            .ActivateOnEnter<RavenousSaber>()
            .ActivateOnEnter<GreatDivide>()
            .ActivateOnEnter<TargetedQuake>()
            .ActivateOnEnter<TrackingTremors>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(M08NHowlingBladeStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), IconIDType = typeof(IconID), PrimaryActorOID = (uint)OID.Boss, Contributors = "The Combat Reborn Team (Malediktus)", Expansion = BossModuleInfo.Expansion.Dawntrail, Category = BossModuleInfo.Category.Raid, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1025, NameID = 13843, SortOrder = 1, PlanLevel = 0)]
public sealed class M08NHowlingBlade(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(17f));
