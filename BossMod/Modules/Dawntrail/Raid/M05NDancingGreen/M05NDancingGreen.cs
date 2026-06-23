using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Dawntrail.Raid.M05NDancingGreen;

public enum OID : uint
{
    Boss = 0x47B6, // R4.998
    Frogtourage = 0x47B7, // R3.142
    Spotlight = 0x47B8, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 44484, // Boss->player, no cast, single-target
    Teleport = 42693, // Boss->location, no cast, single-target

    DoTheHustle1 = 42697, // Boss->self, 5.0s cast, range 50 180-degree cone
    DoTheHustle2 = 42698, // Boss->self, 5.0s cast, range 50 180-degree cone

    TwoSnapTwistFirst1 = 42704, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwistFirst2 = 42701, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwistFirst3 = 42699, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwistFirst4 = 42702, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwistFirst5 = 42197, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwistFirst6 = 42700, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwistFirst7 = 42703, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwistFirst8 = 42198, // Boss->self, 5.0s cast, range 20 width 40 rect
    TwoSnapTwist2 = 42705, // Helper->self, 1.5s cast, range 25 width 50 rect
    TwoSnapTwist3 = 42706, // Helper->self, 3.5s cast, range 25 width 50 rect
    FourSnapTwistFirst1 = 42719, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwistFirst2 = 42720, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwistFirst3 = 42716, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwistFirst4 = 42718, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwistFirst5 = 42201, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwistFirst6 = 42717, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwistFirst7 = 42721, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwistFirst8 = 42202, // Boss->self, 5.0s cast, range 20 width 40 rect
    FourSnapTwist2 = 42722, // Helper->self, 1.0s cast, range 25 width 50 rect
    FourSnapTwist3 = 42723, // Helper->self, 1.5s cast, range 25 width 50 rect
    FourSnapTwist4 = 42724, // Helper->self, 2.0s cast, range 25 width 50 rect
    FourSnapTwist5 = 42725, // Helper->self, 3.5s cast, range 25 width 50 rect

    DeepCutVisual = 42694, // Boss->self, 5.0s cast, single-target, tankbuster
    DeepCut = 42695, // Helper->self, no cast, range 60 45-degree cone

    FunkyFloorVisual = 42741, // Boss->self, 2.5+0,5s cast, single-target, checkerboard
    FunkyFloor = 42742, // Helper->self, no cast, ???

    FullBeatVisual = 42750, // Boss->self, 7.0s cast, single-target, stack
    FullBeat = 42751, // Helper->players, 7.0s cast, range 6 circle

    DiscoInfernal = 42745, // Boss->self, 4.0s cast, range 60 circle
    Shame = 42747, // Helper->player, 1.0s cast, single-target, failed spotlight

    CelebrateGoodTimes = 42696, // Boss->self, 5.0s cast, range 60 circle

    RideTheWavesVisual = 42743, // Boss->self, 3.5+0,5s cast, single-target, rectangle exaflares
    RideTheWaves = 42744, // Helper->self, no cast, ???

    EnsembleAssemble = 39472, // Boss->self, 3.0s cast, single-target
    ArcadyNightFever = 42757, // Boss->self, 4.8s cast, single-target
    FrogtourageDanceVisual1 = 42758, // Boss->self, no cast, single-target
    FrogtourageDanceVisual2 = 37825, // Helper->Frogtourage, 1.2s cast, single-target
    FrogtourageDanceVisual3 = 37824, // Boss->self, no cast, single-target
    FrogtourageDanceVisual4 = 37897, // Boss->self, no cast, single-target
    FrogtourageDanceVisual5 = 37909, // Boss->self, no cast, single-target
    FrogtourageDanceVisual6 = 37930, // Boss->self, no cast, single-target
    FrogtourageDanceVisual7 = 37836, // Frogtourage->self, no cast, single-target
    FrogtourageDanceVisualRight = 42764, // Frogtourage->self, 1.7s cast, single-target, frog changes to modelstate 5
    FrogtourageDanceVisualLeft = 42765, // Frogtourage->self, 1.7s cast, single-target, frog changes to modelstate 7
    Stone = 39092, // Boss->self, no cast, single-target

    LetsDanceVisual1 = 42772, // Boss->self, 5.8s cast, single-target
    LetsDanceVisual2 = 42775, // Boss->self, no cast, single-target
    LetsDanceVisual3 = 42776, // Boss->self, no cast, single-target
    LetsDanceVisual4 = 37835, // Frogtourage->self, no cast, single-target, frog changes to modelstate 6
    LetsDance = 39900, // Helper->self, no cast, range 25 width 90 rect

    LetsPoseVisual1 = 37844, // Frogtourage->self, 5.0s cast, single-target
    LetsPoseVisual2 = 37843, // Frogtourage->self, 5.0s cast, single-target
    LetsPose1 = 42777, // Boss->self, 5.0s cast, range 60 circle
    LetsPose2 = 42778, // Boss->self, 5.0s cast, range 60 circle

    Frogtourage = 42756, // Boss->self, 3.0s cast, single-target
    MoonburnVisualStart = 42781, // Frogtourage->self, no cast, single-target
    MoonburnVisualEnd = 42782, // Frogtourage->self, 1.0s cast, single-target
    Moonburn1 = 42784, // Helper->self, 10.5s cast, range 40 width 15 rect
    Moonburn2 = 42783, // Helper->self, 10.5s cast, range 40 width 15 rect

    EighthBeatsVisual = 42754, // Boss->self, 5.0s cast, single-target
    EighthBeats = 42755 // Helper->player, 5.0s cast, range 5 circle, spread
}

public enum IconID : uint
{
    DeepCut = 471, // player->self
    BurnBabyBurn = 601 // player->self
}

public enum SID : uint
{
    BurnBabyBurn = 4460 // Helper->player, extra=0x0
}

sealed class DoTheHustle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DoTheHustle1, (uint)AID.DoTheHustle2], new AOEShapeCone(50f, 90f.Degrees()));
sealed class DeepCut(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60f, 22.5f.Degrees()), (uint)IconID.DeepCut, AID.DeepCut, 5f);
sealed class FullBeat(BossModule module) : Components.StackWithCastTargets(module, AID.FullBeat, 6f, 8, 8);
sealed class CelebrateGoodTimesDiscoInfernalLetsPose(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.CelebrateGoodTimes, (uint)AID.DiscoInfernal, (uint)AID.LetsPose1, (uint)AID.LetsPose2]);
sealed class EighthBeats(BossModule module) : Components.SpreadFromCastTargets(module, AID.EighthBeats, 5f);
sealed class Moonburn(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Moonburn1, (uint)AID.Moonburn2], new AOEShapeRect(40f, 7.5f));

sealed class TwoFourSnapTwist(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private readonly AOEShapeRect _rect = new(20f, 20f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(2);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TwoSnapTwistFirst1:
            case (uint)AID.TwoSnapTwistFirst2:
            case (uint)AID.TwoSnapTwistFirst3:
            case (uint)AID.TwoSnapTwistFirst4:
            case (uint)AID.TwoSnapTwistFirst5:
            case (uint)AID.TwoSnapTwistFirst6:
            case (uint)AID.TwoSnapTwistFirst7:
            case (uint)AID.TwoSnapTwistFirst8:
            case (uint)AID.FourSnapTwistFirst1:
            case (uint)AID.FourSnapTwistFirst2:
            case (uint)AID.FourSnapTwistFirst3:
            case (uint)AID.FourSnapTwistFirst4:
            case (uint)AID.FourSnapTwistFirst5:
            case (uint)AID.FourSnapTwistFirst6:
            case (uint)AID.FourSnapTwistFirst7:
            case (uint)AID.FourSnapTwistFirst8:
                AddAOE();
                AddAOE(180f.Degrees(), 3.5f);
                break;
        }

        void AddAOE(Angle offset = default, float delay = default)
        {
            var loc = spell.LocXZ;
            var rot = spell.Rotation;
            _aoes.Add(new(_rect, delay != default ? loc - 5f * rot.ToDirection() : loc, rot + offset, Module.CastFinishAt(spell, delay)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var count = _aoes.Count;
        if (count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.TwoSnapTwist2:
                case (uint)AID.TwoSnapTwist3:
                case (uint)AID.FourSnapTwist4:
                case (uint)AID.FourSnapTwist5:
                    _aoes.RemoveAt(0);
                    if (count == 2)
                    {
                        var aoe = _aoes[0];
                        aoe.Origin -= 5f * aoe.Rotation.ToDirection();
                        _aoes[0] = aoe;
                    }
                    break;
            }
        }
    }
}

sealed class Spotlight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCircle Circle = new(2.5f);
    private bool _active;

    private static readonly WPos[] ThreeSpotlights = [new(87.48f, 112.474f), new(112.474f, 99.992f), new(99.992f, 87.48f), new(112.474f, 87.48f), new(87.48f, 99.992f), new(99.992f, 112.474f)];
    private static readonly WPos[] FourSpotlights1 = [new(94.987f, 87.48f), new(104.997f, 112.474f), new(112.474f, 94.987f), new(87.48f, 104.997f)];
    private static readonly WPos[] FourSpotlights2 = [new(104.997f, 87.48f), new(87.48f, 94.987f), new(112.474f, 104.997f), new(94.987f, 112.474f)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _active ? _aoes.Count == 6 ? _aoes.Take(3) : _aoes : [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BurnBabyBurn)
        {
            _active = true;
            var act = WorldState.FutureTime(8f);
            for (var i = 0; i < _aoes.Count; ++i)
            {
                var aoe = _aoes[i];
                aoe.Activation = act;
                _aoes[i] = aoe;
            }
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (_active && id is 0x2FCF or 0x2FD0)
        {
            var first = Raid.WithoutSlot(true, false, false).Any(p => p.FindStatus((uint)SID.BurnBabyBurn) != null);
            if (first && _aoes.Count == 6)
            {
                _aoes.RemoveRange(0, 3);
            }
            else if (!first)
            {
                _aoes.Clear();
            }
            _active = false;
        }
        else if (_aoes.Count == 0 && id == 0x11DC)
        {
            var position = actor.Position;
            IEnumerable<WPos> positions = position == new WPos(112.5f, 87.5f)
                ? ThreeSpotlights
                : position == new WPos(87.5f, 112.5f)
                ? ThreeSpotlights.Reverse()
                : position == new WPos(95f, 87.5f)
                ? FourSpotlights1
                : position == new WPos(95f, 112.5f)
                ? FourSpotlights2
                : [];

            foreach (var pos in positions)
                _aoes.Add(new(Circle, pos, default, default, ArenaColor.SafeFromAOE, false));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _aoes.Count;
        if (count < 2 || !_active)
            return;

        var countAdj = count == 6 ? 3 : count;
        hints.AddForbiddenZone(ShapeContains.Intersection([.. _aoes.Take(countAdj).Select(aoe => ShapeContains.InvertedCircle(aoe.Origin, 2.5f))]), _aoes[0].Activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_active)
            return;

        var isInside = ActiveAOEs(slot, actor).Any(aoe => aoe.Check(actor.Position));
        hints.Add("Go into a spotlight!", !isInside);
    }
}

sealed class LetsDance(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private readonly AOEShapeRect _rect = new(25f, 45f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(2);

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.Frogtourage && modelState is 5 or 7)
        {
            var count = _aoes.Count;
            var act = count != 0 ? _aoes[0].Activation.AddSeconds(count * 2d) : WorldState.FutureTime(18.2f);
            var rot = modelState == 5 ? 270f.Degrees() : default;
            _aoes.Add(new(_rect, Arena.Center, rot, act));
            if (count == 1)
            {
                var aoe = _aoes[1];
                aoe.Origin += 5f * rot.ToDirection();
                _aoes[1] = aoe;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var count = _aoes.Count;
        if (count != 0 && spell.Action.ID == (uint)AID.LetsDance)
        {
            _aoes.RemoveAt(0);

            if (count > 1)
            {
                var aoe1 = _aoes[0];
                aoe1.Origin -= 5f * aoe1.Rotation.ToDirection();
                _aoes[0] = aoe1;
                if (count > 2)
                {
                    var aoe2 = _aoes[1];
                    aoe2.Origin += 5f * aoe2.Rotation.ToDirection();
                    _aoes[1] = aoe2;
                }
            }
        }
    }
}

sealed class RideTheWaves(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeRect _rect = new(15f, 2.5f);
    private readonly List<AOEInstance> _aoes = new(14);
    private readonly WDir _dir = new(default, 1f);
    private int _exaflaresStarted;
    private int? _safespot;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x04)
            return;

        int[] indices = [];
        switch (state)
        {
            case 0x04000400u:
                indices = [0, 2, 3, 4, 5, 6, 7];
                _safespot = 1;
                break;
            case 0x00020002u:
            case 0x08000800u:
                indices = [0, 1, 3, 4, 5, 6, 7];
                _safespot = 2;
                break;
            case 0x10001000u:
            case 0x00100010u:
                indices = [0, 1, 2, 4, 5, 6, 7];
                _safespot = 3;
                break;
            case 0x20002000u:
            case 0x00200020u:
                indices = [0, 1, 2, 3, 5, 6, 7];
                _safespot = 4;
                break;
            case 0x00400040u:
            case 0x40004000u:
                indices = [0, 1, 2, 3, 4, 6, 7];
                _safespot = 5;
                break;
            case 0x80008000u:
                indices = [0, 1, 2, 3, 4, 5, 7];
                _safespot = 6;
                break;
            case 0x00080004u:
                ++_exaflaresStarted;
                break;
        }

        if (indices.Length != 0)
        {
            for (var i = 0; i < 7; ++i)
                _aoes.Add(new(_rect, new(82.5f + indices[i] * 5f, 70f), default, WorldState.FutureTime(3f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.RideTheWaves)
            return;

        ++NumCasts;
        if (_aoes.Count > 6 && NumCasts is 10 or 16)
        {
            _aoes.RemoveRange(0, 7);
            --_exaflaresStarted;
        }
        if (NumCasts is 3 or 7)
            _safespot = null;
        if (_aoes.Count == 0)
        {
            NumCasts = 0;
            return;
        }

        var max = Math.Min(_exaflaresStarted == 1 ? 7 : 14, _aoes.Count);
        var act = WorldState.FutureTime(2f);
        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            aoe.Origin = new(aoe.Origin.X, aoe.Origin.Z + 5f);
            aoe.Activation = act;
            _aoes[i] = aoe;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        if (_safespot is int index)
            Arena.ZoneRect(new(82.5f + index * 5f, 80f), _dir, 5, default, 2.5f, ArenaColor.SafeFromAOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_safespot is not int index || _aoes.Count == 0)
            return;

        var actIndex = Math.Min(_exaflaresStarted <= 1 ? 0 : 7, _aoes.Count - 1);
        hints.AddForbiddenZone(ShapeContains.InvertedRect(new WPos(82.5f + index * 5f, 80f), _dir, 5f, default, 2.5f), _aoes[actIndex].Activation.AddSeconds(1d));
    }
}

sealed class FunkyFloor(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeRect _square = new(2.5f, 2.5f, 2.5f);
    private readonly List<AOEInstance> _aoes = new(64);
    private readonly WPos[] _envC20001 = GenerateCheckerboard(0); // 03.20001 top left active
    private readonly WPos[] _envC200010 = GenerateCheckerboard(1); // 03.200010 top left inactive
    private bool? _activeSet;
    private bool _first = true;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activeSet is bool set)
            return set ? _aoes.Take(32) : _aoes.Skip(32);
        return [];
    }

    private static WPos[] GenerateCheckerboard(int offset)
    {
        var centers = new WPos[32];
        var index = 0;
        for (var i = 0; i < 8; ++i)
        {
            var z = i * 5;
            var start = (i + offset) & 1;
            for (var j = start; j < 8; j += 2)
                centers[index++] = new(82.5f + j * 5f, 82.5f + z);
        }
        return centers;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x03 || _activeSet != null)
            return;

        var act = WorldState.FutureTime(3.2f);
        AddAOESet(_envC20001, act);
        AddAOESet(_envC200010, act);

        _activeSet = state switch
        {
            0x00020001u => true,
            0x00200010u => false,
            _ => _activeSet
        };

        void AddAOESet(WPos[] positions, DateTime activation)
        {
            for (var i = 0; i < 32; ++i)
                _aoes.Add(new(_square, positions[i], default, activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.FunkyFloor || _activeSet == null)
            return;

        ++NumCasts;
        if (_first && NumCasts == 5 || !_first && NumCasts == 6)
        {
            _aoes.Clear();
            NumCasts = 0;
            _first = false;
            _activeSet = null;
            return;
        }

        _activeSet = !_activeSet;
        var act = WorldState.FutureTime(4f);
        var start = _activeSet == true ? 0 : 32;
        for (var i = 0; i < 32; ++i)
        {
            var aoe = _aoes[start + i];
            aoe.Activation = act;
            _aoes[start + i] = aoe;
        }
    }
}

sealed class M05NDancingGreenStates : StateMachineBuilder
{
    public M05NDancingGreenStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DoTheHustle>()
            .ActivateOnEnter<Spotlight>()
            .ActivateOnEnter<Moonburn>()
            .ActivateOnEnter<DeepCut>()
            .ActivateOnEnter<FullBeat>()
            .ActivateOnEnter<CelebrateGoodTimesDiscoInfernalLetsPose>()
            .ActivateOnEnter<EighthBeats>()
            .ActivateOnEnter<FunkyFloor>()
            .ActivateOnEnter<LetsDance>()
            .ActivateOnEnter<RideTheWaves>()
            .ActivateOnEnter<TwoFourSnapTwist>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1019, NameID = 13778)]
public sealed class M05NDancingGreen(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
