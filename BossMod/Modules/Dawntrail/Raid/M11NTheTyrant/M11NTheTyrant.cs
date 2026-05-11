namespace BossMod.Dawntrail.Raid.M11NTheTyrant;

public enum OID : uint
{
    Boss = 0x4AE8,
    Comet = 0x4AE9,
    Maelstrom = 0x4AEB,
    Helper = 0x233C,
}

public enum AID : uint
{
    CrownOfArcadia = 46006,
    SmashdownAxe = 46011,
    SmashdownScythe = 46013,
    SmashdownSword = 46015,
    VoidStardust = 46024,
    Cometite = 46026,
    AssaultEvolvedScythe = 46031,
    AssaultEvolvedSword = 46032,
    AssaultEvolvedAxe = 46030,
    DanceOfDomination = 46033,
    Explosion = 46035,
    RawSteelHit = 46017,
    RawSteelImpact = 46018,
    Charybdistopia = 46039,
    PowerfulGust = 46041,
    OneAndOnly = 46043,
    CosmicKiss = 46046,
    MassiveMeteor = 46051,
    ForegoneFatality = 46048,
    DoubleTyrannhilation = 46053,
    HiddenTyrannhilation = 46215,
    Flatliner = 46056,
    FlatlinerKnockup = 47759,
    MajesticMeteor = 46058,
    MajesticMeteorain = 46059,
    MammothMeteor = 46060,
    FireAndFuryCone1 = 46073,
    FireAndFuryCone2 = 46072,
    ExplosionKnockUp = 46061,
    ArcadionAvalanche = 46066,
    ArcadionAvalancheToss = 46070,
    HeartbreakKick = 46080,
    GreatWallOfFire = 46077,
}

sealed class CrownOfArcadia(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect Shape = new(20f, 6f, 20f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CrownOfArcadia)
        {
            _aoes.Clear();
            _aoes.Add(new(Shape, new WPos(74f, 100f)));
            _aoes.Add(new(Shape, new WPos(126f, 100f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CrownOfArcadia)
            _aoes.Clear();
    }
}
sealed class Smashdown1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SmashdownScythe, new AOEShapeDonut(5f, 60f));
sealed class Smashdown2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SmashdownAxe, 8f);
sealed class Smashdown3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SmashdownSword, new AOEShapeCross(40f, 5f));
sealed class VoidStardust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidStardust, 4f);
sealed class Cometite(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Cometite, 4f);
sealed class AssaultEvolved1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AssaultEvolvedSword, new AOEShapeCross(40f, 5f));
sealed class AssaultEvolved2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AssaultEvolvedScythe, new AOEShapeDonut(5f, 60f));
sealed class AssaultEvolved3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AssaultEvolvedAxe, 8f);
sealed class DanceOfDomination(BossModule module) : Components.RaidwideCast(module, AID.DanceOfDomination);
sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, new AOEShapeRect(60f, 5f));
sealed class RawSteelTankBuster(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RawSteelHit, 6f);
sealed class RawSteelSpreads(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RawSteelImpact, 6f);
sealed class Charybdistopia(BossModule module) : Components.RaidwideCast(module, AID.Charybdistopia);
sealed class Maelstrom(BossModule module) : Components.Adds(module, (uint)OID.Maelstrom);
sealed class PowerfulGust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PowerfulGust, new AOEShapeCone(60f, 22.5f.Degrees()));
sealed class OneAndOnly(BossModule module) : Components.RaidwideCast(module, AID.OneAndOnly);
sealed class CosmicKiss(BossModule module) : Components.CastTowers(module, AID.CosmicKiss, 4f);
sealed class MassiveMeteor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MassiveMeteor, 6f);
sealed class ForegoneFatality(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ForegoneFatality, 6f);
sealed class DoubleTyrannhilation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DoubleTyrannhilation, 30f);
sealed class HiddenTyrannhilation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HiddenTyrannhilation, 30f);
sealed class Flatliner(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Flatliner, new AOEShapeRect(20f, 6f));
sealed class FlatlinerKnockUp(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.FlatlinerKnockup, 15f, stopAtWall: true);
sealed class MajesticMeteor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MajesticMeteor, 6f);
sealed class MajesticMeteorain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MajesticMeteorain, new AOEShapeRect(60f, 5f));
sealed class FireAndFury1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireAndFuryCone1, new AOEShapeCone(60f, 45f.Degrees()));
sealed class FireAndFury2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireAndFuryCone2, new AOEShapeCone(60f, 45f.Degrees()));
sealed class MammothMeteor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MammothMeteor, 22f);
sealed class ExplosionKnockUp(BossModule module) : Components.CastTowers(module, AID.ExplosionKnockUp, 4f, minSoakers: 1, maxSoakers: 8);
sealed class ArcadionAvalanche(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArcadionAvalanche, new AOEShapeRect(40f, 20f));
sealed class ArcadionAvalancheToss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArcadionAvalancheToss, new AOEShapeRect(40f, 20f));
sealed class HeartbreakKick(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeartbreakKick, 4f);
sealed class GreatWallOfFire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GreatWallOfFire, new AOEShapeRect(60f, 3f));

sealed class M11NTheTyrantStates : StateMachineBuilder
{
    public M11NTheTyrantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CrownOfArcadia>()
            .ActivateOnEnter<Smashdown1>()
            .ActivateOnEnter<Smashdown2>()
            .ActivateOnEnter<Smashdown3>()
            .ActivateOnEnter<VoidStardust>()
            .ActivateOnEnter<Cometite>()
            .ActivateOnEnter<AssaultEvolved1>()
            .ActivateOnEnter<AssaultEvolved2>()
            .ActivateOnEnter<AssaultEvolved3>()
            .ActivateOnEnter<DanceOfDomination>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<RawSteelTankBuster>()
            .ActivateOnEnter<RawSteelSpreads>()
            .ActivateOnEnter<Charybdistopia>()
            .ActivateOnEnter<Maelstrom>()
            .ActivateOnEnter<PowerfulGust>()
            .ActivateOnEnter<OneAndOnly>()
            .ActivateOnEnter<CosmicKiss>()
            .ActivateOnEnter<MassiveMeteor>()
            .ActivateOnEnter<ForegoneFatality>()
            .ActivateOnEnter<DoubleTyrannhilation>()
            .ActivateOnEnter<HiddenTyrannhilation>()
            .ActivateOnEnter<Flatliner>()
            .ActivateOnEnter<FlatlinerKnockUp>()
            .ActivateOnEnter<MajesticMeteor>()
            .ActivateOnEnter<MajesticMeteorain>()
            .ActivateOnEnter<FireAndFury1>()
            .ActivateOnEnter<FireAndFury2>()
            .ActivateOnEnter<MammothMeteor>()
            .ActivateOnEnter<ExplosionKnockUp>()
            .ActivateOnEnter<ArcadionAvalanche>()
            .ActivateOnEnter<ArcadionAvalancheToss>()
            .ActivateOnEnter<HeartbreakKick>()
            .ActivateOnEnter<GreatWallOfFire>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(M11NTheTyrantStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), PrimaryActorOID = (uint)OID.Boss, Contributors = "The Combat Reborn Team (Malediktus), CN merge", Expansion = BossModuleInfo.Expansion.Dawntrail, Category = BossModuleInfo.Category.Raid, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1071u, NameID = 14372u, SortOrder = 1, PlanLevel = 0)]
public sealed class M11NTheTyrant(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsRect(20f, 20f));
