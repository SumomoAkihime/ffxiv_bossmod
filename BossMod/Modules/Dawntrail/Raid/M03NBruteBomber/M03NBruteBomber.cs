using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace BossMod.Dawntrail.Raid.M03NBruteBomber;

public enum OID : uint
{
    Boss = 0x42C2,
    LitFuse = 0x42C3,
    Refbot = 0x42C4,
    LariatHelper = 0x42C5,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 39553,
    Teleport = 37803,

    BrutalBurnVisual = 37928,
    BrutalBurn = 37929,

    BrutalImpactFirst = 37846,
    BrutalImpactRest = 37847,

    BrutalLariatVisual1 = 39670,
    BrutalLariatVisual2 = 39636,
    BrutalLariatVisual3 = 39637,
    BrutalLariatVisual4 = 39808,
    BrutalLariatVisual5 = 39670,
    BrutalLariatVisual6 = 39671,
    BrutalLariat1 = 39638,
    BrutalLariat2 = 39639,

    LariatComboVisual1 = 39644,
    LariatComboVisual2 = 39645,
    LariatComboVisual3 = 39646,
    LariatComboVisual4 = 39647,
    LariatComboVisual5 = 39648,
    LariatComboVisual6 = 39649,
    LariatComboVisual7 = 39650,
    LariatComboVisual8 = 39651,
    LariatCombo1 = 39652,
    LariatCombo2 = 39653,
    LariatCombo3 = 39654,
    LariatCombo4 = 39655,

    DopingDraught = 37822,

    BarbarousBarrage = 37810,
    Explosion = 37811,
    UnmitigatedExplosion = 37812,

    ExplosiveRain1 = 37837,
    ExplosiveRain2 = 37838,
    ExplosiveRain3 = 37839,
    ExplosiveRain4 = 38541,

    FireSpinCW = 37840,
    FireSpinCCW = 37841,
    FireSpinVisual = 37842,
    FireSpinFirst = 39768,
    FireSpinRest = 39769,

    FusesOfFury = 37814,

    InfernalSpinCW = 39746,
    InfernalSpinCCW = 39747,
    InfernalSpinVisual = 39748,
    InfernalSpinFirst = 39770,
    InfernalSpinRest = 39771,

    KnuckleSandwich = 37845,

    MurderousMist = 37813,

    SelfDestruct1 = 37816,
    SelfDestruct2 = 37817
}

public enum SID : uint
{
    LitFuseShort = 4015,
    LitFuseLong = 4016,
    LitFuseShortBurning = 4017,
    LitFuseLongBurning = 4018,
}

sealed class BrutalImpact(BossModule module) : Components.RaidwideCast(module, AID.BrutalImpactFirst);
sealed class KnuckleSandwich(BossModule module) : Components.CastSharedTankbuster(module, AID.KnuckleSandwich, 6f);
sealed class BrutalLariat(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BrutalLariat1, (uint)AID.BrutalLariat2], new AOEShapeRect(50f, 17f));
sealed class MurderousMist(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MurderousMist, new AOEShapeCone(40f, 135f.Degrees()));
sealed class BrutalBurn(BossModule module) : Components.StackWithCastTargets(module, AID.BrutalBurn, 6f, 8, 8);

sealed class BarbarousBarrageTower(BossModule module) : Components.GenericTowers(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x01 && state == 0x00020004u)
        {
            WPos[] positions = [new(106f, 94f), new(94f, 106f), new(106f, 106f), new(94f, 94f)];
            for (var i = 0; i < 4; ++i)
                Towers.Add(new(positions[i], 4f, 1, 1, default, WorldState.FutureTime(10f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Explosion)
            Towers.Clear();
    }
}

sealed class BarbarousBarrageKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Explosion, 22f);

sealed class ExplosiveRainConcentric(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(8f), new AOEShapeDonut(8f, 16f), new AOEShapeDonut(16f, 24f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ExplosiveRain1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count == 0)
            return;

        var order = spell.Action.ID switch
        {
            (uint)AID.ExplosiveRain1 => 0,
            (uint)AID.ExplosiveRain2 => 1,
            (uint)AID.ExplosiveRain3 => 2,
            _ => -1
        };
        if (order >= 0)
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2f));
    }
}

sealed class ExplosiveRainCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExplosiveRain4, new AOEShapeCircle(6f));

sealed class FireSpin(BossModule module) : Components.GenericRotatingAOE(module)
{
    private readonly AOEShapeCone _cone = new(40f, 30f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FireSpinCCW:
            case (uint)AID.InfernalSpinCCW:
                AddSequence(45f.Degrees());
                break;
            case (uint)AID.FireSpinCW:
            case (uint)AID.InfernalSpinCW:
                AddSequence(-45f.Degrees());
                break;
        }

        void AddSequence(Angle increment) => Sequences.Add(new(_cone, spell.LocXZ, spell.Rotation, increment, Module.CastFinishAt(spell, 0.5f), 1f, 8));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FireSpinFirst or (uint)AID.FireSpinRest or (uint)AID.InfernalSpinFirst or (uint)AID.InfernalSpinRest)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

sealed class LariatCombo(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect Rect1 = new(20f, 30f, 5f, -90f.Degrees());
    private static readonly AOEShapeRect Rect2 = new(20f, 30f, 5f, 90f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LariatComboVisual1:
                AddAOEs(Rect2, Rect1);
                break;
            case (uint)AID.LariatComboVisual2:
                AddAOEs(Rect2, Rect2);
                break;
            case (uint)AID.LariatComboVisual3:
                AddAOEs(Rect1, Rect2);
                break;
            case (uint)AID.LariatComboVisual4:
                AddAOEs(Rect1, Rect1);
                break;
        }

        void AddAOEs(AOEShapeRect shape1, AOEShapeRect shape2)
        {
            _aoes.Add(new(shape1, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Add(new(shape2, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 4.4f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.LariatCombo1 or (uint)AID.LariatCombo2 or (uint)AID.LariatCombo3 or (uint)AID.LariatCombo4)
            _aoes.RemoveAt(0);
    }
}

sealed class LitFuse(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private readonly BarbarousBarrageTower _tower = module.FindComponent<BarbarousBarrageTower>()!;
    private static readonly AOEShapeCircle Circle = new(8f);
    private bool _fusesOfFury;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i < 4)
                aoes[i] = count > 4 ? aoe with { Risky = _tower.Towers.Count == 0 } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FusesOfFury)
            _fusesOfFury = true;
    }

    public override void Update()
    {
        var count = _aoes.Count;
        if (_fusesOfFury && _tower.Towers.Count != 0 && count == 8)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var a = ref aoes[i];
                a.Activation = a.Activation.AddSeconds(3d);
            }
            _fusesOfFury = false;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        void AddAOE(DateTime activation)
        {
            _aoes.Add(new(Circle, actor.Position, default, activation));
        }

        switch (status.ID)
        {
            case (uint)SID.LitFuseLong:
                AddAOE(WorldState.FutureTime(10.4f));
                break;
            case (uint)SID.LitFuseShort:
                AddAOE(WorldState.FutureTime(7.4f));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.SelfDestruct1 or (uint)AID.SelfDestruct2)
        {
            _aoes.RemoveAt(0);
            _fusesOfFury = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_aoes.Count != 0 && _tower.Towers.Count != 0)
            hints.Add("Don't panic! AOEs start resolving 3.8s after towers.");
    }
}

sealed class M03NBruteBomberStates : StateMachineBuilder
{
    public M03NBruteBomberStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<KnuckleSandwich>()
            .ActivateOnEnter<BrutalImpact>()
            .ActivateOnEnter<BarbarousBarrageTower>()
            .ActivateOnEnter<BarbarousBarrageKnockback>()
            .ActivateOnEnter<BrutalLariat>()
            .ActivateOnEnter<ExplosiveRainCircle>()
            .ActivateOnEnter<ExplosiveRainConcentric>()
            .ActivateOnEnter<FireSpin>()
            .ActivateOnEnter<LitFuse>()
            .ActivateOnEnter<LariatCombo>()
            .ActivateOnEnter<BrutalBurn>()
            .ActivateOnEnter<MurderousMist>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 989, NameID = 13356)]
public sealed class M03NBruteBomber(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(15f));
