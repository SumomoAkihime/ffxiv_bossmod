namespace BossMod.Dawntrail.Advanced.Ad01TheMerchantsTale.Ad011PariofPlenty;

public enum OID : uint
{
    PariOfPlenty = 0x4A6D,
    Helper = 0x233C,
    FalseFlame = 0x4A6E,
    FieryBauble = 0x4A6F,
    FlyingCarpet = 0x4A74,
}

public enum AID : uint
{
    HeatBurst = 45516,
    BurningGleam = 45499,
    BurningGleam1 = 47397,
    BurningGleam2 = 45043,
    LeftFireflightFourLongNights = 45462,
    RightFireflightFourLongNights = 45461,
    WheelOfFireflight = 45463,
    WheelOfFireflight1 = 45466,
    WheelOfFireflight2 = 45465,
    WheelOfFireflight3 = 45464,
    CharmdChains = 45199,
    LeftFableflight = 45429,
    RightFableflight = 45428,
    FireOfVictory = 45518,
    ParisCurse = 45520,
    FirePowder = 45521,
    HighFirePowder = 45522,
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
    TurningRight = 624,
    TurningLeft = 625,
    TurningRRight = 644,
    TurningRLeft = 645,
}

sealed class HeatBurst(BossModule module) : Components.RaidwideCast(module, AID.HeatBurst);
sealed class BurningGleam(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BurningGleam, (uint)AID.BurningGleam1, (uint)AID.BurningGleam2], new AOEShapeCross(40f, 5f));
sealed class CharmedChains(BossModule module) : Components.Chains(module, (uint)TetherID.CharmedChain);
sealed class SimpleFableFlight(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftFableflight, (uint)AID.RightFableflight], new AOEShapeCone(60f, 90f.Degrees()));
sealed class FireOfVictory(BossModule module) : Components.SpreadFromCastTargets(module, AID.FireOfVictory, 4f);
sealed class ParisCurse(BossModule module) : Components.RaidwideCast(module, AID.ParisCurse);
sealed class FirePowder(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FirePowder, (uint)AID.HighFirePowder], 15f);
sealed class SpurningFlames(BossModule module) : Components.RaidwideCast(module, AID.SpurningFlames);
sealed class ImpassionedSpark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ImpassionedSparks3, 8f);
sealed class BurningPillar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BurningPillar, 10f);
sealed class FireWell(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, AID.FireWell, 6f, 3f);
sealed class ScouringScorn(BossModule module) : Components.RaidwideCast(module, AID.ScouringScorn);
sealed class FalseFlameDisplay(BossModule module) : Components.AddsPointless(module, (uint)OID.FalseFlame);
sealed class FieryBaubleDisplay(BossModule module) : Components.AddsPointless(module, (uint)OID.FieryBauble);
sealed class FlyingCarpetDisplay(BossModule module) : Components.AddsPointless(module, (uint)OID.FlyingCarpet);
sealed class LeftRightFireflight(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftFireflightFourLongNights, (uint)AID.RightFireflightFourLongNights], new AOEShapeRect(40f, 2f));
sealed class WheelOfFireflight(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _aoes = [];
    bool _startLeft;
    Angle _currentRot;
    uint _prevIcon;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LeftFireflightFourLongNights)
            _startLeft = true;
        else if ((AID)spell.Action.ID == AID.RightFireflightFourLongNights)
            _startLeft = false;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is not ((uint)IconID.TurningRight or (uint)IconID.TurningLeft or (uint)IconID.TurningRRight or (uint)IconID.TurningRLeft))
            return;

        if (_prevIcon == 0)
        {
            var rightTurn = iconID is (uint)IconID.TurningRight or (uint)IconID.TurningRRight;
            _currentRot = _startLeft ? (rightTurn ? 180f.Degrees() : default) : (rightTurn ? default : 180f.Degrees());
        }
        else if (_prevIcon == iconID)
        {
            _currentRot += 180f.Degrees();
        }

        _aoes.Add(new(new AOEShapeCone(40f, 90f.Degrees()), Module.PrimaryActor.Position, _currentRot));
        _prevIcon = iconID;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is not (AID.WheelOfFireflight or AID.WheelOfFireflight1 or AID.WheelOfFireflight2 or AID.WheelOfFireflight3))
            return;

        if (_aoes.Count == 0)
            return;

        _aoes.RemoveAt(0);
        if (_aoes.Count == 0)
        {
            _currentRot = default;
            _prevIcon = 0;
        }
    }
}

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
            .ActivateOnEnter<ParisCurse>()
            .ActivateOnEnter<FirePowder>()
            .ActivateOnEnter<SpurningFlames>()
            .ActivateOnEnter<ImpassionedSpark>()
            .ActivateOnEnter<BurningPillar>()
            .ActivateOnEnter<FireWell>()
            .ActivateOnEnter<ScouringScorn>()
            .ActivateOnEnter<FalseFlameDisplay>()
            .ActivateOnEnter<FieryBaubleDisplay>()
            .ActivateOnEnter<FlyingCarpetDisplay>()
            .ActivateOnEnter<LeftRightFireflight>()
            .ActivateOnEnter<WheelOfFireflight>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(PariOfPlentyStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), TetherIDType = typeof(TetherID), IconIDType = typeof(IconID), PrimaryActorOID = (uint)OID.PariOfPlenty, Contributors = "HerStolenLight", Expansion = BossModuleInfo.Expansion.Dawntrail, Category = BossModuleInfo.Category.Variant, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1084u, NameID = 14274u, SortOrder = 1, PlanLevel = 0)]
public sealed class PariOfPlenty(WorldState ws, Actor primary) : BossModule(ws, primary, new(-760f, -805f), new ArenaBoundsSquare(20f));
