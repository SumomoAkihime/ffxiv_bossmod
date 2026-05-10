namespace BossMod.Dawntrail.Advanced.Ad01TheMerchantsTale.Ad011PariofPlenty;

public enum OID : uint
{
    PariOfPlenty = 0x4A6D,
    Helper = 0x233C,
    FalseFlame = 0x4A6E,
}

public enum AID : uint
{
    HeatBurst = 45516,
    BurningGleam = 45499,
    BurningGleam1 = 47397,
    BurningGleam2 = 45043,
    CharmdChains = 45199,
    LeftFableflight = 45429,
    RightFableflight = 45428,
    FireOfVictory = 45518,
    SpurningFlames = 45481,
    ImpassionedSparks3 = 45487,
    BurningPillar = 45526,
    FireWell = 45528,
    ScouringScorn = 45490,
}

public enum TetherID : uint
{
    CharmedChain = 9,
}

public enum IconID : uint
{
    Stack = 318,
}

sealed class HeatBurst(BossModule module) : Components.RaidwideCast(module, AID.HeatBurst);
sealed class BurningGleam(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BurningGleam, (uint)AID.BurningGleam1, (uint)AID.BurningGleam2], new AOEShapeCross(40f, 5f));
sealed class CharmedChains(BossModule module) : Components.Chains(module, (uint)TetherID.CharmedChain);
sealed class SimpleFableFlight(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftFableflight, (uint)AID.RightFableflight], new AOEShapeCone(60f, 90f.Degrees()));
sealed class FireOfVictory(BossModule module) : Components.SpreadFromCastTargets(module, AID.FireOfVictory, 4f);
sealed class SpurningFlames(BossModule module) : Components.RaidwideCast(module, AID.SpurningFlames);
sealed class ImpassionedSpark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ImpassionedSparks3, 8f);
sealed class BurningPillar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BurningPillar, 10f);
sealed class FireWell(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, AID.FireWell, 6f, 3f);
sealed class ScouringScorn(BossModule module) : Components.RaidwideCast(module, AID.ScouringScorn);
sealed class FalseFlameDisplay(BossModule module) : Components.AddsPointless(module, (uint)OID.FalseFlame);

sealed class PariOfPlentyStates : StateMachineBuilder
{
    public PariOfPlentyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeatBurst>()
            .ActivateOnEnter<BurningGleam>()
            .ActivateOnEnter<CharmedChains>()
            .ActivateOnEnter<SimpleFableFlight>()
            .ActivateOnEnter<FireOfVictory>()
            .ActivateOnEnter<SpurningFlames>()
            .ActivateOnEnter<ImpassionedSpark>()
            .ActivateOnEnter<BurningPillar>()
            .ActivateOnEnter<FireWell>()
            .ActivateOnEnter<ScouringScorn>()
            .ActivateOnEnter<FalseFlameDisplay>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(PariOfPlentyStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), TetherIDType = typeof(TetherID), IconIDType = typeof(IconID), PrimaryActorOID = (uint)OID.PariOfPlenty, Contributors = "HerStolenLight", Expansion = BossModuleInfo.Expansion.Dawntrail, Category = BossModuleInfo.Category.Variant, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1084u, NameID = 14274u, SortOrder = 1, PlanLevel = 0)]
public sealed class PariOfPlenty(WorldState ws, Actor primary) : BossModule(ws, primary, new(-760f, -805f), new ArenaBoundsSquare(20f));
