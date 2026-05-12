namespace BossMod.Dawntrail.Quest.Job.Viper.FangsOfTheViper;

public enum OID : uint
{
    Boss = 0x429F,
    FawningPeiste = 0x42A1,
    FawningRaptor = 0x42A3,
    FawningWivre = 0x42A2,
    WanderingGowrow = 0x42A0,
    Helper = 0x43A3
}

public enum AID : uint
{
    AutoAttack1 = 6497,
    AutoAttack2 = 6498,
    Visual1 = 37699,
    Visual2 = 37698,
    Visual3 = 37697,
    BurningCyclone = 37703,
    FoulBreath = 39263,
    BrowHorn = 37704,
    Firebreathe = 37700,
    RightSidedShockwave = 37701,
    LeftSidedShockwave = 37702
}

sealed class BurningCyclone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BurningCyclone, new AOEShapeCone(6f, 60f.Degrees()));
sealed class FoulBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FoulBreath, new AOEShapeCone(7f, 45f.Degrees()));
sealed class BrowHorn(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrowHorn, new AOEShapeRect(6f, 2f));
sealed class Firebreathe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Firebreathe, new AOEShapeCone(60f, 45f.Degrees()));
sealed class Shockwave(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RightSidedShockwave, (uint)AID.LeftSidedShockwave], new AOEShapeCone(20f, 90f.Degrees()));

sealed class FangsOfTheViperStates : StateMachineBuilder
{
    public FangsOfTheViperStates(FangsOfTheViper module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BurningCyclone>()
            .ActivateOnEnter<FoulBreath>()
            .ActivateOnEnter<BrowHorn>()
            .ActivateOnEnter<Firebreathe>()
            .ActivateOnEnter<Shockwave>()
            .Raw.Update = () => (module.BossGowrow?.IsDead ?? false) || module.PrimaryActor.IsDeadOrDestroyed;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70385, NameID = 12825)]
public sealed class FangsOfTheViper(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, ArenaBounds)
{
    private static readonly WPos ArenaCenter = new(264f, 480f);
    private static readonly ArenaBoundsCircle ArenaBounds = new(19.5f);

    public Actor? BossGowrow;

    protected override void UpdateModule()
    {
        BossGowrow ??= Enemies((uint)OID.WanderingGowrow).FirstOrDefault();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.FawningWivre), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.FawningPeiste), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.FawningRaptor), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.WanderingGowrow), ArenaColor.Enemy);
    }

    protected override bool CheckPull() => Raid.Player()!.InCombat;
}
