namespace BossMod.Dawntrail.Raid.M09NVampFatale;

public enum OID : uint
{
    Boss = 0x4ADC,
    Coffinmaker = 0x4ADD,
    FatalFlail = 0x4ADE,
    Neckbiter = 0x4AE5,
    Helper = 0x233C,
}

public enum AID : uint
{
    KillerVoice = 45921,
    HalfMoon1 = 45910,
    HalfMoon2 = 45911,
    HalfMoon3 = 45906,
    HalfMoon4 = 45907,
    HalfMoon5 = 45912,
    HalfMoon6 = 45913,
    HalfMoon7 = 45908,
    HalfMoon8 = 45909,
    VampStomp1 = 45899,
    Hardcore1 = 45915,
    Hardcore2 = 45916,
    FlayingFry1 = 45923,
    Coffinfiller = 45878,
    Coffinfiller2 = 45879,
    Coffinfiller3 = 45880,
    BlastBeat = 45901,
    DeadWake1 = 45877,
    PenetratingPitch1 = 45925,
    BrutalRain = 45917,
    CrowdKill = 45886,
    PulpingPulse = 45894,
    Aetherletting1 = 45896,
    Aetherletting2 = 45897,
    InsatiableThirst = 45892,
    Plummet = 45883,
}

public enum IconID : uint
{
    BrutalRain = 305,
}

sealed class KillerVoice(BossModule module) : Components.RaidwideCast(module, AID.KillerVoice);
sealed class HalfMoon(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HalfMoon1, (uint)AID.HalfMoon2, (uint)AID.HalfMoon3, (uint)AID.HalfMoon4, (uint)AID.HalfMoon5, (uint)AID.HalfMoon6, (uint)AID.HalfMoon7, (uint)AID.HalfMoon8], new AOEShapeCone(64f, 90f.Degrees()));
sealed class VampStomp(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VampStomp1, 10f);
sealed class Hardcore(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Hardcore1, (uint)AID.Hardcore2], 6f);
sealed class FlayingFry(BossModule module) : Components.SpreadFromCastTargets(module, AID.FlayingFry1, 5f);
sealed class CoffinFiller(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Coffinfiller, (uint)AID.Coffinfiller2, (uint)AID.Coffinfiller3], new AOEShapeRect(32f, 2.5f));
sealed class BlastBeat(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlastBeat, 8f);
sealed class DeadWake(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeadWake1, new AOEShapeRect(10f, 10f));
sealed class PenetratingPitch(BossModule module) : Components.StackWithCastTargets(module, AID.PenetratingPitch1, 5f, 2, 4);
sealed class BrutalRain(BossModule module) : Components.StackWithIcon(module, (uint)IconID.BrutalRain, AID.BrutalRain, 6f, 0f);
sealed class CrowdKill(BossModule module) : Components.RaidwideCast(module, AID.CrowdKill);
sealed class PulpingPulse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PulpingPulse, 5f);
sealed class AetherlettingHit(BossModule module) : Components.SpreadFromCastTargets(module, AID.Aetherletting1, 6f);
sealed class AetherlettingCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Aetherletting2, new AOEShapeCross(40f, 3f));
sealed class InsatiableThirst(BossModule module) : Components.RaidwideCast(module, AID.InsatiableThirst);
sealed class Plummet(BossModule module) : Components.CastTowers(module, AID.Plummet, 3f);

sealed class M09NVampFataleStates : StateMachineBuilder
{
    public M09NVampFataleStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<KillerVoice>()
            .ActivateOnEnter<HalfMoon>()
            .ActivateOnEnter<VampStomp>()
            .ActivateOnEnter<Hardcore>()
            .ActivateOnEnter<FlayingFry>()
            .ActivateOnEnter<CoffinFiller>()
            .ActivateOnEnter<BlastBeat>()
            .ActivateOnEnter<DeadWake>()
            .ActivateOnEnter<PenetratingPitch>()
            .ActivateOnEnter<BrutalRain>()
            .ActivateOnEnter<CrowdKill>()
            .ActivateOnEnter<PulpingPulse>()
            .ActivateOnEnter<AetherlettingHit>()
            .ActivateOnEnter<AetherlettingCross>()
            .ActivateOnEnter<InsatiableThirst>()
            .ActivateOnEnter<Plummet>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(M09NVampFataleStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), IconIDType = typeof(IconID), PrimaryActorOID = (uint)OID.Boss, Contributors = "The Combat Reborn Team (Malediktus), CN merge", Expansion = BossModuleInfo.Expansion.Dawntrail, Category = BossModuleInfo.Category.Raid, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1068, NameID = 14300, SortOrder = 1, PlanLevel = 0)]
public sealed class M09NVampFatale(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));

