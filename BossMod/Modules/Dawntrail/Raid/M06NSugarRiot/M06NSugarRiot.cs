using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Dawntrail.Raid.M06NSugarRiot;

public enum OID : uint
{
    Boss = 0x4799,
    Painting = 0x47B4,
    SweetShot = 0x479D,
    ThrowUpTarget = 0x479C,
    HeavenBomb = 0x479B,
    PaintBomb = 0x479A,
    TempestPiece = 0x479E,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 42931,
    Teleport = 42611,
    MousseMural = 42607,
    Burst1 = 42581,
    Burst2 = 42583,
    Rush = 42585,
    PuddingParty = 42606,
    WarmBomb = 42609,
    CoolBomb = 42610,
    SprayPain = 42603,
    MousseTouchUp = 42613,
    TasteOfThunder = 42590,
    TasteOfFire = 42588,
    LightningBolt = 42597,
    Highlightning = 42599,
}

public enum TetherID : uint
{
    ActivateMechanicSingleStyle = 324,
    ActivateMechanicDoubleStyle1 = 319,
    ActivateMechanicDoubleStyle2 = 320
}

public enum IconID : uint
{
    PuddingParty = 305
}

sealed class SprayPain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SprayPain, 10f);
sealed class LightningBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBolt, 4f);
sealed class WarmBomb(BossModule module) : Components.BaitAwayCast(module, AID.WarmBomb, new AOEShapeCircle(4f), true);
sealed class CoolBomb(BossModule module) : Components.BaitAwayCast(module, AID.CoolBomb, new AOEShapeCircle(4f), true);
sealed class MousseTouchUp(BossModule module) : Components.SpreadFromCastTargets(module, AID.MousseTouchUp, 6f);
sealed class TasteOfThunder(BossModule module) : Components.SpreadFromCastTargets(module, AID.TasteOfThunder, 6f);
sealed class TasteOfFire(BossModule module) : Components.StackWithCastTargets(module, AID.TasteOfFire, 6f, 4, 4);
sealed class MousseMural(BossModule module) : Components.RaidwideCast(module, AID.MousseMural);

sealed class PuddingParty(BossModule module) : Components.UniformStackSpread(module, 6f, 0, 8, 8)
{
    private int _numCasts;
    private bool _first = true;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.PuddingParty)
            AddStack(actor, WorldState.FutureTime(5.1f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.PuddingParty)
        {
            ++_numCasts;
            if ((_first && _numCasts == 5) || _numCasts == 6)
            {
                Stacks.Clear();
                _numCasts = 0;
                _first = false;
            }
        }
    }
}

sealed class Quicksand(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Circle = new(23f);
    private readonly List<AOEInstance> _aoe = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x1F and <= 0x23)
        {
            if (state == 0x00020001u)
            {
                var pos = index switch
                {
                    0x1F => Arena.Center,
                    0x20 => new WPos(100f, 80f),
                    0x21 => new WPos(100f, 120f),
                    0x22 => new WPos(120f, 100f),
                    0x23 => new WPos(80f, 100f),
                    _ => default
                };
                if (pos != default)
                {
                    _aoe.Clear();
                    _aoe.Add(new(Circle, pos, default, WorldState.FutureTime(6f)));
                }
            }
            else if (state == 0x00080004u)
            {
                _aoe.Clear();
            }
        }
    }
}

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect RiverHint = new(20f, 5f);
    private readonly List<AOEInstance> _aoe = [];
    private bool _riverActive;
    private bool _fireMode;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x04)
            return;

        switch (state)
        {
            case 0x00020001u:
                _riverActive = true;
                break;
            case 0x00800040u:
                _aoe.Clear();
                _aoe.Add(new(RiverHint, Arena.Center, default, WorldState.FutureTime(7f)));
                _fireMode = false;
                break;
            case 0x00200010u:
                Arena.Bounds = M06NSugarRiot.RiverArenaBounds;
                Arena.Center = M06NSugarRiot.RiverArenaCenter;
                _riverActive = false;
                _aoe.Clear();
                break;
            case 0x08000004u:
                Arena.Bounds = M06NSugarRiot.DefaultArenaBounds;
                Arena.Center = M06NSugarRiot.DefaultArenaCenter;
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TasteOfFire:
                _aoe.Clear();
                _aoe.Add(new(RiverHint, Arena.Center, default, Module.CastFinishAt(spell, 4.2f), ArenaColor.SafeFromAOE));
                _fireMode = true;
                break;
            case (uint)AID.TasteOfThunder:
                _aoe.Clear();
                _aoe.Add(new(RiverHint, Arena.Center, default, Module.CastFinishAt(spell, 4.2f)));
                _fireMode = false;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TasteOfFire or (uint)AID.TasteOfThunder)
            _aoe.Clear();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_riverActive || _aoe.Count == 0)
            return;
        var inside = _aoe[0].Check(actor.Position);
        if (_fireMode)
            hints.Add("Be inside river!", !inside);
        else
            hints.Add("GTFO from river!", inside);
    }
}

sealed class SingleDoubleStyle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle Circle = new(15f);
    private bool _target;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        switch (tether.ID)
        {
            case (uint)TetherID.ActivateMechanicSingleStyle:
                HandleSingle(source, 8.7f, _target ? 11.7f : 10.7f);
                break;
            case (uint)TetherID.ActivateMechanicDoubleStyle1:
                HandleDoubleOne(source, 9.6f);
                break;
            case (uint)TetherID.ActivateMechanicDoubleStyle2:
                HandleDoubleTwo(source, _target ? 10.7f : 9.6f);
                break;
        }
    }

    private void HandleSingle(Actor source, float bombT, float shotT)
    {
        switch (source.OID)
        {
            case (uint)OID.PaintBomb:
                _aoes.Add(new(Circle, source.Position, default, WorldState.FutureTime(bombT)));
                break;
            case (uint)OID.HeavenBomb:
                _aoes.Add(new(Circle, source.Position + 16f * source.Rotation.ToDirection(), default, WorldState.FutureTime(bombT)));
                break;
            case (uint)OID.SweetShot:
                AddSweet(source, shotT);
                break;
            case (uint)OID.ThrowUpTarget:
                _target = true;
                break;
        }
    }

    private void HandleDoubleOne(Actor source, float t)
    {
        switch (source.OID)
        {
            case (uint)OID.PaintBomb:
                _aoes.Add(new(Circle, source.Position, default, WorldState.FutureTime(t)));
                break;
            case (uint)OID.HeavenBomb:
                _aoes.Add(new(Circle, source.Position + 16f * source.Rotation.ToDirection(), default, WorldState.FutureTime(t)));
                break;
            case (uint)OID.ThrowUpTarget:
                _target = true;
                break;
        }
    }

    private void HandleDoubleTwo(Actor source, float shotT)
    {
        switch (source.OID)
        {
            case (uint)OID.SweetShot:
                AddSweet(source, shotT);
                break;
            case (uint)OID.PaintBomb:
                _aoes.Add(new(Circle, source.Position, default, WorldState.FutureTime(9.6f)));
                break;
        }
    }

    private void AddSweet(Actor source, float t)
    {
        var direction = _target ? (Arena.Center - source.Position).Normalized() : source.Rotation.ToDirection();
        _aoes.Add(new(new AOEShapeRect(60f, 3.5f), source.Position, Angle.FromDirection(direction), WorldState.FutureTime(t)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.Burst1 or (uint)AID.Burst2 or (uint)AID.Rush)
        {
            _aoes.Clear();
            _target = false;
        }
    }
}

sealed class Highlightning(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Circle = new(21f);
    private readonly List<AOEInstance> _aoe = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TempestPiece)
        {
            _aoe.Clear();
            _aoe.Add(new(Circle, actor.Position, default, WorldState.FutureTime(6.5f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Highlightning)
        {
            ++NumCasts;
            if (NumCasts >= 3)
            {
                NumCasts = 0;
                _aoe.Clear();
            }
            else
            {
                _aoe.Clear();
                _aoe.Add(new(Circle, caster.Position, default, WorldState.FutureTime(10f)));
            }
        }
    }
}

sealed class M06NSugarRiotStates : StateMachineBuilder
{
    public M06NSugarRiotStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<SingleDoubleStyle>()
            .ActivateOnEnter<MousseMural>()
            .ActivateOnEnter<SprayPain>()
            .ActivateOnEnter<WarmBomb>()
            .ActivateOnEnter<CoolBomb>()
            .ActivateOnEnter<PuddingParty>()
            .ActivateOnEnter<MousseTouchUp>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<TasteOfFire>()
            .ActivateOnEnter<TasteOfThunder>()
            .ActivateOnEnter<Quicksand>()
            .ActivateOnEnter<Highlightning>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1021, NameID = 13822)]
public sealed class M06NSugarRiot(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultArenaCenter, DefaultArenaBounds)
{
    public static readonly WPos DefaultArenaCenter = new(100f, 100f);
    public static readonly ArenaBoundsSquare DefaultArenaBounds = new(20f);
    public static readonly WPos RiverArenaCenter = new(100f, 100f);
    public static readonly ArenaBoundsRect RiverArenaBounds = new(20f, 5f);
}
