using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Dawntrail.Raid.M05NDancingGreen;

public enum OID : uint
{
    Boss = 0x47B6,
    Frogtourage = 0x47B7,
    Spotlight = 0x47B8,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 44484,
    Teleport = 42693,
    DoTheHustle1 = 42697,
    DoTheHustle2 = 42698,
    TwoSnapTwistFirst1 = 42704,
    TwoSnapTwistFirst2 = 42701,
    TwoSnapTwistFirst3 = 42699,
    TwoSnapTwistFirst4 = 42702,
    TwoSnapTwistFirst5 = 42197,
    TwoSnapTwistFirst6 = 42700,
    TwoSnapTwistFirst7 = 42703,
    TwoSnapTwistFirst8 = 42198,
    TwoSnapTwist2 = 42705,
    TwoSnapTwist3 = 42706,
    FourSnapTwistFirst1 = 42719,
    FourSnapTwistFirst2 = 42720,
    FourSnapTwistFirst3 = 42716,
    FourSnapTwistFirst4 = 42718,
    FourSnapTwistFirst5 = 42201,
    FourSnapTwistFirst6 = 42717,
    FourSnapTwistFirst7 = 42721,
    FourSnapTwistFirst8 = 42202,
    FourSnapTwist2 = 42722,
    FourSnapTwist3 = 42723,
    FourSnapTwist4 = 42724,
    FourSnapTwist5 = 42725,
    DeepCutVisual = 42694,
    DeepCut = 42695,
    FunkyFloorVisual = 42741,
    FunkyFloor = 42742,
    FullBeatVisual = 42750,
    FullBeat = 42751,
    DiscoInfernal = 42745,
    Shame = 42747,
    CelebrateGoodTimes = 42696,
    RideTheWavesVisual = 42743,
    RideTheWaves = 42744,
    EnsembleAssemble = 39472,
    ArcadyNightFever = 42757,
    FrogtourageDanceVisualRight = 42764,
    FrogtourageDanceVisualLeft = 42765,
    LetsDanceVisual1 = 42772,
    LetsDanceVisual2 = 42775,
    LetsDanceVisual3 = 42776,
    LetsDance = 39900,
    LetsPose1 = 42777,
    LetsPose2 = 42778,
    Frogtourage = 42756,
    Moonburn1 = 42784,
    Moonburn2 = 42783,
    EighthBeatsVisual = 42754,
    EighthBeats = 42755
}

public enum IconID : uint
{
    DeepCut = 471,
    BurnBabyBurn = 601
}

public enum SID : uint
{
    BurnBabyBurn = 4460
}

sealed class DoTheHustle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DoTheHustle1, (uint)AID.DoTheHustle2], new AOEShapeCone(50f, 90f.Degrees()));
sealed class DeepCut(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60f, 22.5f.Degrees()), (uint)IconID.DeepCut, AID.DeepCut, 5f);
sealed class FullBeat(BossModule module) : Components.StackWithCastTargets(module, AID.FullBeat, 6f, 8, 8);
sealed class CelebrateGoodTimes(BossModule module) : Components.RaidwideCast(module, AID.CelebrateGoodTimes);
sealed class DiscoInfernal(BossModule module) : Components.RaidwideCast(module, AID.DiscoInfernal);
sealed class LetsPose1(BossModule module) : Components.RaidwideCast(module, AID.LetsPose1);
sealed class LetsPose2(BossModule module) : Components.RaidwideCast(module, AID.LetsPose2);
sealed class EighthBeats(BossModule module) : Components.SpreadFromCastTargets(module, AID.EighthBeats, 5f);
sealed class Moonburn(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Moonburn1, (uint)AID.Moonburn2], new AOEShapeRect(40f, 7.5f));

sealed class TwoFourSnapTwist(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeRect _rect = new(20f, 20f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(2);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TwoSnapTwistFirst1 or (uint)AID.TwoSnapTwistFirst2 or (uint)AID.TwoSnapTwistFirst3 or (uint)AID.TwoSnapTwistFirst4 or (uint)AID.TwoSnapTwistFirst5 or (uint)AID.TwoSnapTwistFirst6 or (uint)AID.TwoSnapTwistFirst7 or (uint)AID.TwoSnapTwistFirst8 or (uint)AID.FourSnapTwistFirst1 or (uint)AID.FourSnapTwistFirst2 or (uint)AID.FourSnapTwistFirst3 or (uint)AID.FourSnapTwistFirst4 or (uint)AID.FourSnapTwistFirst5 or (uint)AID.FourSnapTwistFirst6 or (uint)AID.FourSnapTwistFirst7 or (uint)AID.FourSnapTwistFirst8)
        {
            _aoes.Add(new(_rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Add(new(_rect, spell.LocXZ - 5f * spell.Rotation.ToDirection(), spell.Rotation + 180f.Degrees(), Module.CastFinishAt(spell, 3.5f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.TwoSnapTwist2 or (uint)AID.TwoSnapTwist3 or (uint)AID.FourSnapTwist4 or (uint)AID.FourSnapTwist5)
            _aoes.RemoveAt(0);
    }
}

sealed class Spotlight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle Circle = new(2.5f);
    private bool _active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _active ? _aoes.Take(3) : [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BurnBabyBurn)
        {
            _active = true;
            var act = WorldState.FutureTime(8f);
            for (var i = 0; i < _aoes.Count; ++i)
            {
                var a = _aoes[i];
                _aoes[i] = a with { Activation = act };
            }
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (_active && id is 0x2FCF or 0x2FD0)
        {
            _active = false;
            _aoes.Clear();
            return;
        }

        if (_aoes.Count == 0 && id == 0x11DC)
        {
            var pos = actor.Position;
            WPos[] positions = pos == new WPos(112.5f, 87.5f)
                ? [new(87.48f, 112.474f), new(112.474f, 99.992f), new(99.992f, 87.48f)]
                : pos == new WPos(87.5f, 112.5f)
                ? [new(99.992f, 112.474f), new(87.48f, 99.992f), new(112.474f, 87.48f)]
                : [];
            for (var i = 0; i < positions.Length; ++i)
                _aoes.Add(new(Circle, positions[i], default, default, ArenaColor.SafeFromAOE, false));
        }
    }
}

sealed class LetsDance(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeRect _rect = new(25f, 45f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(2);

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.Frogtourage && modelState is 5 or 7)
        {
            var rot = modelState == 5 ? 270f.Degrees() : default;
            var act = _aoes.Count > 0 ? _aoes[^1].Activation.AddSeconds(2) : WorldState.FutureTime(18.2f);
            _aoes.Add(new(_rect, Arena.Center, rot, act));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.LetsDance)
            _aoes.RemoveAt(0);
    }
}

sealed class RideTheWaves(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeRect _rect = new(15f, 2.5f);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x04)
            return;

        int[] indices = state switch
        {
            0x04000400u => [0, 2, 3, 4, 5, 6, 7],
            0x00020002u or 0x08000800u => [0, 1, 3, 4, 5, 6, 7],
            0x10001000u or 0x00100010u => [0, 1, 2, 4, 5, 6, 7],
            0x20002000u or 0x00200020u => [0, 1, 2, 3, 5, 6, 7],
            0x00400040u or 0x40004000u => [0, 1, 2, 3, 4, 6, 7],
            0x80008000u => [0, 1, 2, 3, 4, 5, 7],
            _ => []
        };

        if (indices.Length > 0)
        {
            _aoes.Clear();
            for (var i = 0; i < indices.Length; ++i)
                _aoes.Add(new(_rect, new WPos(82.5f + indices[i] * 5f, 70f), default, WorldState.FutureTime(3f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.RideTheWaves || _aoes.Count == 0)
            return;

        ++NumCasts;
        if (NumCasts % 3 == 0)
        {
            _aoes.RemoveAt(0);
        }

        var act = WorldState.FutureTime(2f);
        for (var i = 0; i < _aoes.Count; ++i)
        {
            var a = _aoes[i];
            _aoes[i] = a with { Origin = new(a.Origin.X, a.Origin.Z + 5f), Activation = act };
        }
    }
}

sealed class FunkyFloor(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeRect _square = new(2.5f, 2.5f, 2.5f);
    private readonly List<AOEInstance> _aoes = [];
    private bool _active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _active ? _aoes : [];

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x03)
            return;

        _active = state is 0x00020001u or 0x00200010u;
        if (!_active)
            return;

        _aoes.Clear();
        for (var i = 0; i < 8; ++i)
            for (var j = 0; j < 8; ++j)
                if (((i + j) & 1) == 0)
                    _aoes.Add(new(_square, new WPos(82.5f + j * 5f, 82.5f + i * 5f), default, WorldState.FutureTime(3.2f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FunkyFloor)
            _active = !_active;
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
            .ActivateOnEnter<CelebrateGoodTimes>()
            .ActivateOnEnter<DiscoInfernal>()
            .ActivateOnEnter<LetsPose1>()
            .ActivateOnEnter<LetsPose2>()
            .ActivateOnEnter<EighthBeats>()
            .ActivateOnEnter<FunkyFloor>()
            .ActivateOnEnter<LetsDance>()
            .ActivateOnEnter<RideTheWaves>()
            .ActivateOnEnter<TwoFourSnapTwist>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1019, NameID = 13778)]
public sealed class M05NDancingGreen(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
