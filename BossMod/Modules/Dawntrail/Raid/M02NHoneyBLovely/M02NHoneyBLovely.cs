namespace BossMod.Dawntrail.Raid.M02NHoneyBLovely;

public enum OID : uint
{
    Boss = 0x422A,
    Groupbee = 0x422B,
    PoisonCloud = 0x4230,
    Sweetheart = 0x422C,
    Helper = 0x233C,
}

public enum AID : uint
{
    CallMeHoney = 37220,
    TemptingTwist1 = 39738,
    TemptingTwist2 = 39740,
    HoneyBeeline1 = 39737,
    HoneyBeeline2 = 39739,
    HoneyedBreeze = 37224,
    HoneyBLiveVisual = 37234,
    HoneyBLive = 39550,
    Heartsore = 37242,
    Heartsick = 39821,
    Loveseeker = 39617,
    BlowKiss = 37235,
    HoneyBFinale = 37243,
    DropOfVenom = 37232,
    SplashOfVenom = 37231,
    BlindingLove1 = 39525,
    BlindingLove2 = 39526,
    HeartStruck1 = 37237,
    HeartStruck2 = 37238,
    HeartStruck3 = 37239,
    Fracture = 37240,
    Splinter = 37230,
}

public enum IconID : uint
{
    HoneyedBreezeTB = 230,
}

sealed class CallMeHoney(BossModule module) : Components.RaidwideCast(module, AID.CallMeHoney);
sealed class TemptingTwist(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TemptingTwist1, (uint)AID.TemptingTwist2], new AOEShapeDonut(7f, 30f));
sealed class HoneyBeeline(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HoneyBeeline1, (uint)AID.HoneyBeeline2], new AOEShapeRect(60f, 7f));
sealed class HoneyedBreeze(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(40f, 15f.Degrees()), (uint)IconID.HoneyedBreezeTB, AID.HoneyedBreeze, 5f);
sealed class HoneyBLive(BossModule module) : Components.RaidwideCastDelay(module, AID.HoneyBLiveVisual, AID.HoneyBLive, 8.3f);
sealed class Heartsore(BossModule module) : Components.SpreadFromCastTargets(module, AID.Heartsore, 6f);
sealed class Heartsick(BossModule module) : Components.StackWithCastTargets(module, AID.Heartsick, 6f, 4, 4);
sealed class Loveseeker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Loveseeker, 10f);
sealed class BlowKiss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlowKiss, new AOEShapeCone(40f, 60f.Degrees()));
sealed class HoneyBFinale(BossModule module) : Components.RaidwideCast(module, AID.HoneyBFinale);
sealed class DropOfVenom(BossModule module) : Components.StackWithCastTargets(module, AID.DropOfVenom, 6f, 8, 8);
sealed class SplashOfVenom(BossModule module) : Components.SpreadFromCastTargets(module, AID.SplashOfVenom, 6f);
sealed class BlindingLove(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BlindingLove1, (uint)AID.BlindingLove2], new AOEShapeRect(50f, 4f));
sealed class HeartStruck(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HeartStruck1, (uint)AID.HeartStruck2, (uint)AID.HeartStruck3], 10f);
sealed class Fracture(BossModule module) : Components.CastTowers(module, AID.Fracture, 4f);
sealed class Splinter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Splinter, 8f);

sealed class M02NHoneyBLovelyStates : StateMachineBuilder
{
    public M02NHoneyBLovelyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CallMeHoney>()
            .ActivateOnEnter<TemptingTwist>()
            .ActivateOnEnter<HoneyBeeline>()
            .ActivateOnEnter<HoneyedBreeze>()
            .ActivateOnEnter<HoneyBLive>()
            .ActivateOnEnter<Heartsore>()
            .ActivateOnEnter<Heartsick>()
            .ActivateOnEnter<Loveseeker>()
            .ActivateOnEnter<BlowKiss>()
            .ActivateOnEnter<HoneyBFinale>()
            .ActivateOnEnter<DropOfVenom>()
            .ActivateOnEnter<SplashOfVenom>()
            .ActivateOnEnter<BlindingLove>()
            .ActivateOnEnter<HeartStruck>()
            .ActivateOnEnter<Fracture>()
            .ActivateOnEnter<Splinter>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(M02NHoneyBLovelyStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), IconIDType = typeof(IconID), PrimaryActorOID = (uint)OID.Boss, Contributors = "The Combat Reborn Team (Malediktus, LTS), CN merge", Expansion = BossModuleInfo.Expansion.Dawntrail, Category = BossModuleInfo.Category.Raid, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 987, NameID = 12685, SortOrder = 1, PlanLevel = 0)]
public sealed class M02NHoneyBLovely(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f));
