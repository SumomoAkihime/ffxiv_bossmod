using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Dawntrail.Raid.M04NWickedThunder;

public enum OID : uint
{
    Boss = 0x4263,
    WickedThunder = 0x4569,
    WickedReplica = 0x4264,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 39146,
    AutoAttack2 = 36759,
    Teleport = 37577,

    BewitchingFlightVisual1 = 37545,
    BewitchingFlightVisual2 = 36324,
    BewitchingFlightVisual3 = 36326,
    BewitchingFlight = 37560,

    Burst = 37561,

    ShadowsSabbath1 = 38044,
    ShadowsSabbath2 = 39871,

    SidewiseSpark1 = 37564,
    SidewiseSpark2 = 37565,
    SidewiseSpark3 = 37566,
    SidewiseSpark4 = 37567,
    SidewiseSpark5 = 39429,
    SidewiseSpark6 = 39439,

    SoaringSoulpressVisual1 = 37568,
    SoaringSoulpressVisual2 = 37546,
    SoaringSoulpress = 37569,

    StampedingThunderVisualWest = 37547,
    StampedingThunderVisualEast = 37548,
    StampedingThunderVisual0 = 37543,
    StampedingThunderVisual1 = 35334,
    StampedingThunderVisual2 = 36150,
    StampedingThunderVisual = 35335,

    Thunderslam = 37574,
    ThunderstormVisual = 37544,
    Thunderstorm = 37573,

    WickedBoltVisual = 37570,
    WickedBolt = 37571,

    ThreefoldBlast1 = 37549,
    ThreefoldBlast2 = 37552,
    FourfoldBlast1 = 39759,
    FourfoldBlast2 = 39765,
    FivefoldBlast1 = 39766,
    FivefoldBlast2 = 39767,
    WickedCannon1 = 20032,
    WickedCannon2 = 37550,
    WickedCannon3 = 37551,
    WickedCannon4 = 39852,
    WickedCannon5 = 39870,

    WickedHypercannonVisual1 = 37102,
    WickedHypercannonVisual2 = 37553,
    WickedHypercannonVisual3 = 37554,
    WickedHypercannon = 37555,

    WickedJolt = 37576,

    WitchHuntVisual = 37556,
    WitchHuntTelegraph = 37557,
    WitchHunt = 37558,

    WrathOfZeus = 37575
}

public enum SID : uint
{
    WickedReplica = 2056,
    WickedCannon = 2970
}

public enum IconID : uint
{
    SoaringSoulpress = 161,
    WickedBolt = 316,
    Thunderstorm = 345,
    WickedJolt = 471,
}

sealed class WickedJolt(BossModule module) : Components.BaitAwayCast(module, AID.WickedJolt, new AOEShapeRect(60f, 2.5f), endsOnCastEvent: true);
sealed class WickedBolt(BossModule module) : Components.StackWithIcon(module, (uint)IconID.WickedBolt, AID.WickedBolt, 5f, 5f, 8);
sealed class SoaringSoulpress(BossModule module) : Components.StackWithIcon(module, (uint)IconID.SoaringSoulpress, AID.SoaringSoulpress, 6f, 5.4f, 8);
sealed class WrathOfZeus(BossModule module) : Components.RaidwideCast(module, AID.WrathOfZeus);
sealed class BewitchingFlight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BewitchingFlight, new AOEShapeRect(40f, 2.5f));
sealed class Thunderslam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderslam, 5f);
sealed class Thunderstorm(BossModule module) : Components.SpreadFromCastTargets(module, AID.Thunderstorm, 6f);

sealed class SidewiseSpark(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone Cone = new(60f, 90f.Degrees());
    private static readonly AOEShapeRect Rect = new(40f, 8f);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(2);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape) => _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        switch (spell.Action.ID)
        {
            case (uint)AID.SidewiseSpark1:
            case (uint)AID.SidewiseSpark2:
            case (uint)AID.SidewiseSpark3:
            case (uint)AID.SidewiseSpark4:
                AddAOE(Cone);
                break;
            case (uint)AID.Burst:
                AddAOE(Rect);
                break;
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID != (uint)OID.WickedReplica)
            return;

        switch (id)
        {
            case 0x11D8:
                _aoes.Add(new(Cone, actor.Position, actor.Rotation + 90f.Degrees(), WorldState.FutureTime(8f)));
                break;
            case 0x11D6:
                _aoes.Add(new(Cone, actor.Position, actor.Rotation - 90f.Degrees(), WorldState.FutureTime(8f)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.SidewiseSpark1 or (uint)AID.SidewiseSpark2 or (uint)AID.SidewiseSpark3 or (uint)AID.SidewiseSpark4 or (uint)AID.SidewiseSpark5 or (uint)AID.SidewiseSpark6 or (uint)AID.Burst)
            _aoes.RemoveAt(0);
    }
}

sealed class StampedingThunder(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect Rect = new(40f, 15f);
    private readonly List<AOEInstance> _aoe = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.StampedingThunderVisualWest)
            AddAOE(new(95f, 80f), caster.Rotation);
        else if (spell.Action.ID == (uint)AID.StampedingThunderVisualEast)
            AddAOE(new(105f, 80f), caster.Rotation);

        void AddAOE(WPos position, Angle rotation)
        {
            _aoe.Clear();
            _aoe.Add(new(Rect, position, rotation, WorldState.FutureTime(9.4f)));
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state is 0x00200010u or 0x00020001u)
            _aoe.Clear();
    }
}

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public static readonly ArenaBoundsSquare DefaultBounds = new(20f);
    public static readonly WPos DefaultCenter = new(100f, 100f);
    public static readonly WPos WestRemovedCenter = new(115f, 100f);
    public static readonly WPos EastRemovedCenter = new(85f, 100f);
    private static readonly ArenaBoundsRect DamagedPlatform = new(5f, 20f);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x00)
            return;

        switch (state)
        {
            case 0x00020001u:
                Arena.Bounds = DamagedPlatform;
                Arena.Center = EastRemovedCenter;
                break;
            case 0x00200010u:
                Arena.Bounds = DamagedPlatform;
                Arena.Center = WestRemovedCenter;
                break;
            case 0x00400004u:
            case 0x00800004u:
                Arena.Bounds = DefaultBounds;
                Arena.Center = DefaultCenter;
                break;
        }
    }
}

sealed class WickedCannon(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);
    private static readonly AOEShapeRect Rect = new(40f, 5f);
    private static readonly float[] Delays3 = [9.5f, 8.3f, 7.7f];
    private static readonly float[] Delays4 = [12f, 10.9f, 10.2f, 9.5f];
    private static readonly float[] Delays5 = [14.5f, 13.4f, 12.8f, 12.2f, 11.6f];
    private float[] _currentDelays = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(2);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        _currentDelays = spell.Action.ID switch
        {
            (uint)AID.ThreefoldBlast1 or (uint)AID.ThreefoldBlast2 => Delays3,
            (uint)AID.FourfoldBlast1 or (uint)AID.FourfoldBlast2 => Delays4,
            (uint)AID.FivefoldBlast1 or (uint)AID.FivefoldBlast2 => Delays5,
            _ => []
        };
        _aoes.Clear();
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID != (uint)SID.WickedCannon || _currentDelays.Length <= _aoes.Count)
            return;

        var rotation = status.Extra switch
        {
            0x2D3 => 180f.Degrees(),
            0x2D4 => 90f.Degrees(),
            _ => default
        };
        _aoes.Add(new(Rect, Module.PrimaryActor.Position, rotation, WorldState.FutureTime(_currentDelays[_aoes.Count]), Risky: false));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.WickedCannon1 or (uint)AID.WickedCannon2 or (uint)AID.WickedCannon3 or (uint)AID.WickedCannon4 or (uint)AID.WickedCannon5)
            _aoes.RemoveAt(0);
    }
}

sealed class WickedHypercannon(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect Rect = new(40f, 10f);
    private readonly List<AOEInstance> _aoe = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is not ((uint)AID.WickedHypercannonVisual2) and not ((uint)AID.WickedHypercannonVisual3))
            return;

        WPos? pos = null;
        if (Arena.Center == ArenaChanges.EastRemovedCenter)
            pos = new WPos(85f, 80f);
        else if (Arena.Center == ArenaChanges.WestRemovedCenter)
            pos = new WPos(115f, 80f);

        if (pos != null)
        {
            _aoe.Clear();
            _aoe.Add(new(Rect, pos.Value, default, Module.CastFinishAt(spell, 0.6f), Risky: false));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WickedHypercannon)
        {
            if (++NumCasts >= 10)
            {
                _aoe.Clear();
                NumCasts = 0;
            }
        }
    }
}

sealed class WitchHunt(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(25);
    private static readonly AOEShapeCircle Circle = new(6f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(12);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WitchHuntTelegraph)
            _aoes.Add(new(Circle, spell.LocXZ, default, Module.CastFinishAt(spell, 6.3f), Risky: false));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.WitchHunt)
            _aoes.RemoveAt(0);
    }
}

sealed class M04NWickedThunderStates : StateMachineBuilder
{
    public M04NWickedThunderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<WickedHypercannon>()
            .ActivateOnEnter<WickedJolt>()
            .ActivateOnEnter<WickedBolt>()
            .ActivateOnEnter<WickedCannon>()
            .ActivateOnEnter<WrathOfZeus>()
            .ActivateOnEnter<SidewiseSpark>()
            .ActivateOnEnter<SoaringSoulpress>()
            .ActivateOnEnter<StampedingThunder>()
            .ActivateOnEnter<BewitchingFlight>()
            .ActivateOnEnter<Thunderslam>()
            .ActivateOnEnter<Thunderstorm>()
            .ActivateOnEnter<WitchHunt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 991, NameID = 13057)]
public class M04NWickedThunder(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.DefaultCenter, ArenaChanges.DefaultBounds);
