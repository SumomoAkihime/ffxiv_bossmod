namespace BossMod.Dawntrail.Raid.M01NBIackCat;

public enum OID : uint
{
    Boss = 0x429C,
    CopyCat = 0x429D,
    Helper = 0x233C,
}

public enum AID : uint
{
    BiscuitMaker = 37706,
    BloodyScratch = 37696,
    Clawful = 37693,
    GrimalkinGale = 37695,
    OverShadowMarker = 26708,
    Overshadow = 37658,
    Shockwave = 37662,
    OneTwoPaw1 = 37642,
    OneTwoPaw2 = 37643,
    OneTwoPaw3 = 37645,
    OneTwoPaw4 = 37646,
    BlackCatCrossingFirst = 37649,
    BlackCatCrossingRest = 37650,
    MouserTelegraphFirst = 37653,
    MouserTelegraphSecond = 39275,
    Mouser = 38053,
    PredaceousPounceTelegraphCharge1 = 37682,
    PredaceousPounceTelegraphCircle1 = 37683,
    PredaceousPounceTelegraphCharge2 = 37684,
    PredaceousPounceTelegraphCircle2 = 37685,
    PredaceousPounceTelegraphCharge3 = 37686,
    PredaceousPounceTelegraphCircle3 = 37687,
    PredaceousPounceTelegraphCharge4 = 37688,
    PredaceousPounceTelegraphCircle4 = 37689,
    PredaceousPounceTelegraphCharge5 = 37690,
    PredaceousPounceTelegraphCircle5 = 37691,
}

sealed class BloodyScratch(BossModule module) : Components.RaidwideCast(module, AID.BloodyScratch);
sealed class BiscuitMaker(BossModule module) : Components.SingleTargetCast(module, AID.BiscuitMaker);
sealed class Clawful(BossModule module) : Components.StackWithCastTargets(module, AID.Clawful, 5f, 8, 8);
sealed class GrimalkinGale(BossModule module) : Components.SpreadFromCastTargets(module, AID.GrimalkinGale, 5f);
sealed class Overshadow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Overshadow, new AOEShapeRect(60f, 2.5f));
sealed class Shockwave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Shockwave, 18f);
sealed class OneTwoPaw(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone _shape = new(60f, 90f.Degrees());
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count <= NumCasts)
            yield break;

        // Current half-arena hit: imminent (deep yellow); next hit: preview (light yellow).
        yield return _aoes[NumCasts] with { Risky = true, Color = ArenaColor.Danger };
        if (_aoes.Count > NumCasts + 1)
            yield return _aoes[NumCasts + 1] with { Risky = false, Color = ArenaColor.AOE };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.OneTwoPaw1 or AID.OneTwoPaw2 or AID.OneTwoPaw3 or AID.OneTwoPaw4)
        {
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.OneTwoPaw1 or AID.OneTwoPaw2 or AID.OneTwoPaw3 or AID.OneTwoPaw4)
            ++NumCasts;
    }
}
sealed class BlackCatCrossing(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BlackCatCrossingFirst, (uint)AID.BlackCatCrossingRest], new AOEShapeCone(60f, 22.5f.Degrees()));
sealed class MouserTelegraph(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.MouserTelegraphFirst, (uint)AID.MouserTelegraphSecond], new AOEShapeRect(10f, 5f));
sealed class Mouser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mouser, new AOEShapeRect(10f, 5f));
sealed class PredaceousPounce(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PredaceousPounceTelegraphCircle1, (uint)AID.PredaceousPounceTelegraphCircle2, (uint)AID.PredaceousPounceTelegraphCircle3, (uint)AID.PredaceousPounceTelegraphCircle4, (uint)AID.PredaceousPounceTelegraphCircle5], 11f);
sealed class PredaceousPounceCharges(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PredaceousPounceTelegraphCharge1, (uint)AID.PredaceousPounceTelegraphCharge2, (uint)AID.PredaceousPounceTelegraphCharge3, (uint)AID.PredaceousPounceTelegraphCharge4, (uint)AID.PredaceousPounceTelegraphCharge5], new AOEShapeRect(60f, 3f));

sealed class M01NBIackCatStates : StateMachineBuilder
{
    public M01NBIackCatStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BloodyScratch>()
            .ActivateOnEnter<BiscuitMaker>()
            .ActivateOnEnter<Clawful>()
            .ActivateOnEnter<GrimalkinGale>()
            .ActivateOnEnter<Overshadow>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<OneTwoPaw>()
            .ActivateOnEnter<BlackCatCrossing>()
            .ActivateOnEnter<MouserTelegraph>()
            .ActivateOnEnter<Mouser>()
            .ActivateOnEnter<PredaceousPounce>()
            .ActivateOnEnter<PredaceousPounceCharges>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(M01NBIackCatStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), PrimaryActorOID = (uint)OID.Boss, Contributors = "The Combat Reborn Team (Malediktus, LTS), CN merge", Expansion = BossModuleInfo.Expansion.Dawntrail, Category = BossModuleInfo.Category.Raid, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 985, NameID = 12686, SortOrder = 1, PlanLevel = 0)]
public sealed class M01NBIackCat(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
