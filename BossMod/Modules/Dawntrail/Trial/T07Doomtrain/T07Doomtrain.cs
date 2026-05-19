namespace BossMod.Dawntrail.Trial.T07Doomtrain;

public enum OID : uint
{
    Helper = 0x233C,
    Doomtrain = 0x4A30,
    LevinSignal = 0x4A31,
    KinematicTurret = 0x4A32,
    AetherIntermission = 0x4A34,
    DoomtrainIntermission = 0x4B7E,
    GhostTrain = 0x4B80,
    ArcaneRevelation = 0x4A36,
}

public enum AID : uint
{
    LightningBurst = 45661,
    LightningExpress = 45618,
    PlasmaBeamUpper = 45620,
    PlasmaBeamLower = 45619,
    PlasmaBeamMedium = 45621,
    PlasmaBeamShort = 45622,
    WindpipeDrawIn = 45667,
    Blastpipe = 45627,
    UnlimitedExpress = 45624,
    ElectrayLong = 45629,
    ElectrayShort = 45633,
    ElectrayMedium = 45632,
    Electray3 = 45631,
    ElectrayUpper = 45630,
    ThunderousBreathLowerDeck = 45635,
    HeadlightUpperDeck = 45637,
    RunawayTrain = 45638,
    AetherSurge = 45643,
    AetherialRay = 45640,
    RunawayTrainRaidwide = 45645,
    Shockwave = 45647,
    HailOfThunder = 45659,
    DerailmentSiegeStack = 45650,
    DerailmentSiegeCircle = 45649,
    Derail = 45654,
}

public enum IconID : uint
{
    LightningBurstIcon = 343,
    AetherialRayIcon = 412,
}

sealed class LightningBurstTankBuster(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(5f), (uint)IconID.LightningBurstIcon, AID.LightningBurst, 5f, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class LightningExpress(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.LightningExpress, 16f, kind: Kind.DirForward);
sealed class WindpipeDrawIn(BossModule module) : Components.StandardAOEs(module, AID.WindpipeDrawIn, new AOEShapeRect(30f, 10f));
sealed class Blastpipe(BossModule module) : Components.StandardAOEs(module, AID.Blastpipe, new AOEShapeRect(10f, 10f));
sealed class UnlimitedExpress(BossModule module) : Components.StandardAOEs(module, AID.UnlimitedExpress, new AOEShapeRect(70f, 35f));
sealed class PlasmaBeam(BossModule module) : Components.GroupedAOEs(module, [AID.PlasmaBeamUpper, AID.PlasmaBeamLower, AID.PlasmaBeamMedium, AID.PlasmaBeamShort], new AOEShapeRect(30f, 2.5f));
sealed class ElectrayLower(BossModule module) : Components.GroupedAOEs(module, [AID.ElectrayLong, AID.ElectrayShort, AID.ElectrayMedium, AID.Electray3], new AOEShapeRect(25f, 2.5f));
sealed class ElectrayUpper(BossModule module) : Components.StandardAOEs(module, AID.ElectrayUpper, new AOEShapeRect(10f, 2.5f));
sealed class ThunderousBreathLowerDeck(BossModule module) : Components.StandardAOEs(module, AID.ThunderousBreathLowerDeck, new AOEShapeRect(70f, 35f));
sealed class HeadlightUpperDeck(BossModule module) : Components.StandardAOEs(module, AID.HeadlightUpperDeck, new AOEShapeRect(30f, 10f));
sealed class HailOfThunder(BossModule module) : Components.StandardAOEs(module, AID.HailOfThunder, 16f);
sealed class Derail(BossModule module) : Components.StandardAOEs(module, AID.Derail, new AOEShapeRect(30f, 10f));
sealed class DerailmentSiegeSpread(BossModule module) : Components.StandardAOEs(module, AID.DerailmentSiegeStack, 5f);
sealed class DerailmentSiegeStack(BossModule module) : Components.StackWithCastTargets(module, AID.DerailmentSiegeCircle, 5f, minStackSize: 4);
sealed class Shockwave(BossModule module) : Components.RaidwideCast(module, AID.Shockwave);
sealed class RunawayTrain(BossModule module) : Components.RaidwideCast(module, AID.RunawayTrain);
sealed class RunawayTrainRaidwide(BossModule module) : Components.RaidwideCast(module, AID.RunawayTrainRaidwide);
sealed class AetherSurge(BossModule module) : Components.StandardAOEs(module, AID.AetherSurge, new AOEShapeCone(30f, 22.5f.Degrees()));
sealed class AetherialRay(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(50f, 17.5f.Degrees()), (uint)IconID.AetherialRayIcon, AID.AetherialRay, 6f, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class AetherIntermissionAdds(BossModule module) : Components.Adds(module, (uint)OID.AetherIntermission);
sealed class GhostTrainAdds(BossModule module) : Components.Adds(module, (uint)OID.GhostTrain);
sealed class DoomtrainIntermissionAdds(BossModule module) : Components.Adds(module, (uint)OID.DoomtrainIntermission);

sealed class ArcaneRevelation(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.ArcaneRevelation)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, new AOEShapeCircle(16f)));
    }

    public override void Update()
    {
        if (CurrentBaits.Count > 0 && Module.Enemies((uint)OID.ArcaneRevelation).All(x => x.IsDead))
            CurrentBaits.Clear();
    }
}

// Low-risk arena progression: follow car swaps by UnlimitedExpress casts.
sealed class ArenaRailcars(BossModule module) : BossComponent(module)
{
    private const float PlayerCenterSnapRange = 45f;
    private static readonly WPos Car1Center = new(100f, 100f);
    private static readonly WPos Car2Center = new(100f, 150f);
    private static readonly WPos Car3Center = new(100f, 200f);
    private static readonly WPos Car4Center = new(100f, 250f);
    private static readonly WPos Car5Center = new(100f, 300f);
    private static readonly WPos IntermissionCenter = new(-400f, -400f);
    private int _car = 1;
    private bool _initialized;
    private bool _intermissionActive;

    public override void Update()
    {
        if (_initialized)
            return;
        Apply();
        _initialized = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.UnlimitedExpress)
        {
            if (_intermissionActive)
                return;
            _car = Math.Min(_car + 1, 5);
            Apply(strictCenter: true);
        }
        else if ((AID)spell.Action.ID == AID.RunawayTrain)
        {
            _intermissionActive = true;
            ApplyIntermission();
        }
        else if ((AID)spell.Action.ID == AID.RunawayTrainRaidwide)
        {
            // End of intermission sequence; return to car 4 layout.
            _intermissionActive = false;
            _car = 4;
            Apply(strictCenter: true);
        }
        else if ((AID)spell.Action.ID == AID.Derail)
        {
            _intermissionActive = false;
            _car = 5;
            Apply(strictCenter: true);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RunawayTrain)
        {
            _intermissionActive = true;
            ApplyIntermission();
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.GhostTrain or OID.DoomtrainIntermission)
        {
            _intermissionActive = true;
            ApplyIntermission();
        }
    }

    private void Apply(bool strictCenter = false)
    {
        switch (_car)
        {
            case 1:
                ApplyCarRect(Car1Center, 10f, 15f, strictCenter);
                break;
            case 2:
                ApplyCarRect(Car2Center, 10f, 15f, strictCenter);
                break;
            case 3:
                ApplyCarRect(Car3Center, 10f, 15f, strictCenter);
                break;
            case 4:
                ApplyCarRect(Car4Center, 10f, 14.6f, strictCenter);
                break;
            case 5:
                ApplyCarRect(Car5Center, 10f, 15f, strictCenter);
                break;
        }
    }

    private void ApplyCarRect(WPos center, float halfW, float halfH, bool strictCenter)
    {
        Arena.Center = strictCenter ? center : CorrectCenterByPlayer(center, allowNearestFallback: true);
        Arena.Bounds = new ArenaBoundsRect(halfW, halfH);
    }

    private void ApplyIntermission()
    {
        // Intermission begins while player can still be in previous car position;
        // force expected center first and avoid pulling arena back to old car.
        Arena.Center = CorrectCenterByPlayer(IntermissionCenter, allowNearestFallback: false);
        Arena.Bounds = new ArenaBoundsCircle(20f);
    }

    private WPos CorrectCenterByPlayer(WPos expected, bool allowNearestFallback)
    {
        var player = WorldState.Party.Player();
        if (player == null)
            return expected;

        var playerPos = player.Position;
        if ((playerPos - expected).LengthSq() <= PlayerCenterSnapRange * PlayerCenterSnapRange)
            return expected;

        if (!allowNearestFallback)
            return expected;

        WPos[] known = [Car1Center, Car2Center, Car3Center, Car4Center, Car5Center, IntermissionCenter];
        var nearest = known.MinBy(c => (playerPos - c).LengthSq());
        return (playerPos - nearest).LengthSq() <= PlayerCenterSnapRange * PlayerCenterSnapRange ? nearest : expected;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, StatesType = typeof(T07DoomtrainStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), IconIDType = typeof(IconID), PrimaryActorOID = (uint)OID.Doomtrain, Expansion = BossModuleInfo.Expansion.Dawntrail, Category = BossModuleInfo.Category.Trial, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1076, NameID = 14284)]
public sealed class T07Doomtrain(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsRect(10f, 15f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.AetherIntermission), ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.DoomtrainIntermission), ArenaColor.Object, true);
        Arena.Actors(Enemies((uint)OID.GhostTrain), ArenaColor.Object, true);
        Arena.Actors(Enemies((uint)OID.ArcaneRevelation), ArenaColor.Object, true);
    }
}
