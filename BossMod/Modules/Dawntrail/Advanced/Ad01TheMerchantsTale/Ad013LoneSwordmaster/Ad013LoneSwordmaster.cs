namespace BossMod.Dawntrail.Advanced.Ad01TheMerchantsTale.Ad013LoneSwordmaster;

public enum OID : uint
{
    LoneSwordmaster = 0x4B17,
    Helper = 0x233C,
    ForceOfWillSource = 0x4B19,
    ForceOfWillInter = 0x4B1E,
    EchoingHushPuddle = 0x1EBF73,
    ForceOfWill = 0x4C3D,
}

public enum AID : uint
{
    SteelsbreathRelease = 48136,
    LashOfLight = 46655,
    HeavensConfluenceIn1 = 46662,
    HeavensConfluenceIn2 = 46664,
    HeavensConfluenceOut1 = 46663,
    HeavensConfluenceOut2 = 46665,
    NearToHeavenSolo = 46658,
    NearToHeavenMulti = 46660,
    FarFromHeavenSolo = 46659,
    FarFromHeavenMulti = 46661,
    EchoingHush = 46747,
    EchoingHush1 = 46667,
    WolfsCrossing = 46669,
    EchoingEight = 46671,
    StingOfTheScorpion = 46646,
    WaitingWounds = 46676,
    ResoundingSilence = 46678,
    SteelsbreathReleaseArena = 46681,
    WillOfTheUnderworld = 47762,
    WillOfTheUnderworld1 = 46683,
}

public enum IconID : uint
{
    Tankbuster = 218,
    SilentEight = 499,
    Lockon = 648,
    Chains = 653,
}

public enum TetherID : uint
{
    Chains = 163,
}

sealed class SteelsbreathRelease(BossModule module) : Components.RaidwideCast(module, AID.SteelsbreathRelease);
sealed class SteelsbreathReleaseArena(BossModule module) : Components.RaidwideCast(module, AID.SteelsbreathReleaseArena);
sealed class LashOfLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LashOfLight, new AOEShapeCone(40f, 45f.Degrees()));
sealed class HeavensConfluence(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HeavensConfluenceIn1, (uint)AID.HeavensConfluenceIn2], 8f);
sealed class HeavensConfluenceDonut(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HeavensConfluenceOut1, (uint)AID.HeavensConfluenceOut2], new AOEShapeDonut(8f, 60f));
sealed class NearFarFromHeaven(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.NearToHeavenSolo, (uint)AID.NearToHeavenMulti, (uint)AID.FarFromHeavenSolo, (uint)AID.FarFromHeavenMulti], 5f);
sealed class WolfsCrossing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WolfsCrossing, new AOEShapeCross(40f, 4f));
sealed class EchoingHush(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.EchoingHush, (uint)AID.EchoingHush1], 8f);
sealed class EchoingHushPuddle(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 8f, AID.EchoingHush1, m => m.Enemies((uint)OID.EchoingHushPuddle).Where(e => e.EventState != 7), 0.8f);
sealed class EchoingEight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EchoingEight, new AOEShapeCross(40f, 4f));
sealed class StingOfTheScorpion(BossModule module) : Components.SingleTargetCast(module, AID.StingOfTheScorpion);
sealed class WaitingWounds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WaitingWounds, 10f);
sealed class SilentEight(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.SilentEight, AID.ResoundingSilence, 8f, 5f);
sealed class ChainTether(BossModule module) : Components.Chains(module, (uint)TetherID.Chains, chainLength: 20f);
sealed class WillOfTheUnderworld(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WillOfTheUnderworld, new AOEShapeRect(40f, 10f));
sealed class WillOfTheUnderworld1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WillOfTheUnderworld1, new AOEShapeRect(40f, 5f));

sealed class Ad013LoneSwordmasterStates : StateMachineBuilder
{
    public Ad013LoneSwordmasterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SteelsbreathRelease>()
            .ActivateOnEnter<SteelsbreathReleaseArena>()
            .ActivateOnEnter<LashOfLight>()
            .ActivateOnEnter<HeavensConfluence>()
            .ActivateOnEnter<HeavensConfluenceDonut>()
            .ActivateOnEnter<NearFarFromHeaven>()
            .ActivateOnEnter<WolfsCrossing>()
            .ActivateOnEnter<EchoingHush>()
            .ActivateOnEnter<EchoingHushPuddle>()
            .ActivateOnEnter<EchoingEight>()
            .ActivateOnEnter<StingOfTheScorpion>()
            .ActivateOnEnter<WaitingWounds>()
            .ActivateOnEnter<SilentEight>()
            .ActivateOnEnter<ChainTether>()
            .ActivateOnEnter<WillOfTheUnderworld>()
            .ActivateOnEnter<WillOfTheUnderworld1>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(Ad013LoneSwordmasterStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.LoneSwordmaster,
    Contributors = "The Combat Reborn Team, CN minimal port",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Variant,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1084u,
    NameID = 14323u,
    SortOrder = 3,
    PlanLevel = 0)]
public sealed class Ad013LoneSwordmaster(WorldState ws, Actor primary) : BossModule(ws, primary, new(170f, -815f), new ArenaBoundsSquare(20f));
