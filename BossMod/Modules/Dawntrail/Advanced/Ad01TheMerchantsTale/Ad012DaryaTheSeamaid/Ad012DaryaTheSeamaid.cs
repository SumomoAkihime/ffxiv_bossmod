namespace BossMod.Dawntrail.Advanced.Ad01TheMerchantsTale.Ad012DaryaTheSeamaid;

public enum OID : uint
{
    DaryaTheSeaMaid = 0x4A94,
    Helper = 0x233C,
}

public enum AID : uint
{
    PiercingPlunge = 45837,
    SurgingCurrent = 45827,
    AquaBall = 45835,
    Hydrocannon = 45836,
    Hydrobullet = 45816,
    HydrobulletSpread = 45811,
    Tidalspout = 47089,
    TidalWave = 45820,
}

public enum IconID : uint
{
    TankLaserLockon = 471,
}

sealed class PiercingPlunge(BossModule module) : Components.RaidwideCast(module, AID.PiercingPlunge);
sealed class SurgingCurrent(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurgingCurrent, new AOEShapeCone(60f, 45f.Degrees()));
sealed class AquaBall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AquaBall, 5f);
sealed class Hydrocannon(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70f, 3f), (uint)IconID.TankLaserLockon, AID.Hydrocannon);
sealed class Hydrobullet(BossModule module) : Components.SpreadFromCastTargets(module, AID.Hydrobullet, 15f);
sealed class HydrobulletSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.HydrobulletSpread, 15f);
sealed class TidalSpout(BossModule module) : Components.StackWithCastTargets(module, AID.Tidalspout, 6f, 1, 3);
sealed class TidalWave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TidalWave, 25f, kind: Components.SimpleKnockbacks.Kind.DirForward);

sealed class Ad012DaryaTheSeamaidStates : StateMachineBuilder
{
    public Ad012DaryaTheSeamaidStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PiercingPlunge>()
            .ActivateOnEnter<SurgingCurrent>()
            .ActivateOnEnter<AquaBall>()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<Hydrobullet>()
            .ActivateOnEnter<HydrobulletSpread>()
            .ActivateOnEnter<TidalSpout>()
            .ActivateOnEnter<TidalWave>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(Ad012DaryaTheSeamaidStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.DaryaTheSeaMaid,
    Contributors = "The Combat Reborn Team (Malediktus), CN minimal port",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Variant,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1084u,
    NameID = 14291u,
    SortOrder = 2,
    PlanLevel = 0)]
public sealed class Ad012DaryaTheSeaMaid(WorldState ws, Actor primary) : BossModule(ws, primary, new(375f, 530f), new ArenaBoundsSquare(20f));
