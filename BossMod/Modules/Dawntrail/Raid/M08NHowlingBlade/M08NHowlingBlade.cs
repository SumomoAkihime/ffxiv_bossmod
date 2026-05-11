namespace BossMod.Dawntrail.Raid.M08NHowlingBlade;

public enum OID : uint
{
    Boss = 0x4722,
    Helper = 0x233C,
}

public enum AID : uint
{
    ExtraplanarPursuit = 42830,
    TitanicPursuit = 42832,
    GreatDivide = 41869,
    Heavensearth1 = 41866,
    Heavensearth2 = 41859,
    Gust = 41858,
    TargetedQuake = 41864,
    GrowlingWind = 41893,
    WealOfStone1 = 41895,
    WealOfStone2 = 43401,
    WealOfStone3 = 43405,
    FangedCharge = 41861,
    MoonbeamsBite1 = 43388,
    MoonbeamsBite2 = 43389,
    MoonbeamsBite3 = 41847,
    MoonbeamsBite4 = 41846,
    RoaringWind = 43397,
    Shadowchase1 = 43393,
    Shadowchase2 = 41844,
    TerrestrialTitans = 43317,
    Towerfall = 43315,
    TrackingTremors = 42211,
    RavenousSaber5 = 41855,
    WolvesReignRect1 = 43384,
    WolvesReignRect2 = 43368,
    WolvesReignTeleport2 = 43383,
    WolvesReignTeleport4 = 41841,
    WolvesReignCone1 = 43386,
    WolvesReignCone2 = 42928,
    WolvesReignCircle1 = 43380,
    WolvesReignCircle2 = 43381,
    WolvesReignCircle3 = 43382,
    WolvesReignCircle4 = 43379,
    WolvesReignCircle5 = 43302,
    WolvesReignCircle6 = 43303,
    WolvesReignCircle7 = 43304,
    WolvesReignCircle8 = 43311,
}

public enum IconID : uint
{
    TrackingTremors = 316,
}

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut ShrinkWarning = new(12f, 17f);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x00)
            return;

        // 0x00200010: warning before arena shrink
        if (state == 0x00200010u)
        {
            _aoes.Clear();
            _aoes.Add(new(ShrinkWarning, Arena.Center, default, WorldState.FutureTime(11.2f)));
        }
        // 0x00020001: shrink applied
        else if (state == 0x00020001u)
        {
            _aoes.Clear();
            Arena.Bounds = M08NHowlingBlade.EndArena;
            Arena.Center = M08NHowlingBlade.ArenaCenter;
        }
    }
}

sealed class ExtraplanarTitanicPursuit(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ExtraplanarPursuit, (uint)AID.TitanicPursuit], 40f);
sealed class RavenousSaber(BossModule module) : Components.RaidwideCast(module, AID.RavenousSaber5, "Raidwide x5");
sealed class GreatDivide(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GreatDivide, new AOEShapeRect(60f, 3f));
sealed class Heavensearth1(BossModule module) : Components.StackWithCastTargets(module, AID.Heavensearth1, 6f, minStackSize: 8, maxStackSize: 8);
sealed class Heavensearth2(BossModule module) : Components.StackWithCastTargets(module, AID.Heavensearth2, 6f, minStackSize: 8, maxStackSize: 8);
sealed class Gust(BossModule module) : Components.SpreadFromCastTargets(module, AID.Gust, 5f);
sealed class TargetedQuake(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TargetedQuake, 4f);
sealed class GrowlingWindWealofStone(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GrowlingWind, (uint)AID.WealOfStone1, (uint)AID.WealOfStone2, (uint)AID.WealOfStone3], new AOEShapeRect(40f, 3f));
sealed class FangedCharge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FangedCharge, new AOEShapeRect(46f, 3f));
sealed class MoonbeamsBite(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.MoonbeamsBite1, (uint)AID.MoonbeamsBite2, (uint)AID.MoonbeamsBite3, (uint)AID.MoonbeamsBite4], new AOEShapeRect(40f, 10f));
sealed class RoaringWindShadowchase(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RoaringWind, (uint)AID.Shadowchase1, (uint)AID.Shadowchase2], new AOEShapeRect(40f, 4f));
sealed class TerrestrialTitans(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TerrestrialTitans, 3f);
sealed class WolvesReignRect1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WolvesReignRect1, new AOEShapeRect(36f, 5f));
sealed class WolvesReignRect2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WolvesReignRect2, new AOEShapeRect(28f, 5f));
sealed class WolvesReignCircle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.WolvesReignCircle1, (uint)AID.WolvesReignCircle2, (uint)AID.WolvesReignCircle3, (uint)AID.WolvesReignCircle4, (uint)AID.WolvesReignCircle5, (uint)AID.WolvesReignCircle6, (uint)AID.WolvesReignCircle7, (uint)AID.WolvesReignCircle8], 6f);

sealed class WolvesReignCone(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone Shape = new(40f, 60f.Degrees());
    private AOEInstance[] _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WolvesReignTeleport2 or (uint)AID.WolvesReignTeleport4)
        {
            var pos = spell.LocXZ;
            _aoes = [new(Shape, pos, Angle.FromDirection(Arena.Center - pos), Module.CastFinishAt(spell, 5.1f))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WolvesReignCone1 or (uint)AID.WolvesReignCone2)
            _aoes = [];
    }
}

sealed class Towerfall(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect Shape = new(30f, 3f);
    private readonly List<AOEInstance> _aoes = new(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        return _aoes.Take(count > 3 ? 3 : count);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TerrestrialTitans)
            _aoes.Add(new(Shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 7f)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.Towerfall)
            _aoes.RemoveAt(0);
    }
}

sealed class TrackingTremors(BossModule module) : Components.UniformStackSpread(module, 6f, 0, 8, 8)
{
    private int _numCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.TrackingTremors)
            AddStack(actor, WorldState.FutureTime(5f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TrackingTremors && ++_numCasts == 5)
        {
            Stacks.Clear();
            _numCasts = 0;
        }
    }
}

sealed class M08NHowlingBladeStates : StateMachineBuilder
{
    public M08NHowlingBladeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<ExtraplanarTitanicPursuit>()
            .ActivateOnEnter<RavenousSaber>()
            .ActivateOnEnter<GreatDivide>()
            .ActivateOnEnter<Heavensearth1>()
            .ActivateOnEnter<Heavensearth2>()
            .ActivateOnEnter<Gust>()
            .ActivateOnEnter<TargetedQuake>()
            .ActivateOnEnter<GrowlingWindWealofStone>()
            .ActivateOnEnter<FangedCharge>()
            .ActivateOnEnter<MoonbeamsBite>()
            .ActivateOnEnter<RoaringWindShadowchase>()
            .ActivateOnEnter<TerrestrialTitans>()
            .ActivateOnEnter<WolvesReignRect1>()
            .ActivateOnEnter<WolvesReignRect2>()
            .ActivateOnEnter<WolvesReignCircle>()
            .ActivateOnEnter<WolvesReignCone>()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<TrackingTremors>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(M08NHowlingBladeStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), IconIDType = typeof(IconID), PrimaryActorOID = (uint)OID.Boss, Contributors = "The Combat Reborn Team (Malediktus)", Expansion = BossModuleInfo.Expansion.Dawntrail, Category = BossModuleInfo.Category.Raid, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1025, NameID = 13843, SortOrder = 1, PlanLevel = 0)]
public sealed class M08NHowlingBlade(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingArena)
{
    public static readonly WPos ArenaCenter = new(100f, 100f);
    private static readonly ArenaBoundsCircle StartingArena = new(17f);
    public static readonly ArenaBoundsCircle EndArena = new(12f);
}
