namespace BossMod.Dawntrail.Raid.M07NBruteAbombinator;

public enum OID : uint
{
    Boss = 0x4781,
    BloomingAbomination = 0x4782,
    Helper = 0x233C,
}

public enum AID : uint
{
    BrutalImpact = 42265,
    NeoBombarianSpecial = 42287,
    BrutalSmashTB1 = 42272,
    BrutalSmashTB2 = 42273,
    BrutishSwingCircle2 = 42270,
    BrutishSwingDonut = 42271,
    BrutishSwingCone1 = 42293,
    BrutishSwingCone2 = 42317,
    BrutishSwingDonutSegment1 = 42303,
    BrutishSwingDonutSegment2 = 42319,
    RevengeOfTheVines1 = 42307,
    Powerslam = 42312,
    SporeSac = 42282,
    Pollen = 42283,
    ItCameFromTheDirt = 42279,
    TheUnpotted = 42280,
    QuarrySwamp = 42285,
    CrossingCrosswinds = 43276,
    WindingWildwinds = 43275,
    GlowerPower = 43339,
    ElectrogeneticForce = 42311,
    Sporesplosion = 42309,
    Explosion = 42286,
    LashingLariat1 = 42322,
    LashingLariat2 = 42324,
    Slaminator = 42328,
    PulpSmash = 42278,
    AbominableBlink = 43154,
}

public enum IconID : uint
{
    BrutalSmashTB = 600,
    PulpSmash = 161,
    AbominableBlink = 327,
}

sealed class BrutalImpact(BossModule module) : Components.RaidwideCast(module, AID.BrutalImpact);
sealed class NeoBombarianSpecialKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.NeoBombarianSpecial, 58f, stopAtWall: true);
sealed class BrutishSwingCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrutishSwingCircle2, 12f);
sealed class BrutishSwingDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrutishSwingDonut, new AOEShapeDonut(9f, 60f));
sealed class BrutishSwingCone1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrutishSwingCone1, new AOEShapeCone(25f, 90f.Degrees()));
sealed class BrutishSwingCone2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrutishSwingCone2, new AOEShapeCone(25f, 90f.Degrees()));
sealed class BrutishSwingDonutSegment1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrutishSwingDonutSegment1, new AOEShapeDonutSector(22f, 88f, 90f.Degrees()));
sealed class BrutishSwingDonutSegment2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrutishSwingDonutSegment2, new AOEShapeDonutSector(22f, 88f, 90f.Degrees()));
sealed class BrutalSmashTB(BossModule module) : Components.GenericSharedTankbuster(module, default, 6f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BrutalSmashTB)
        {
            Source = Module.PrimaryActor;
            Target = actor;
            Activation = WorldState.FutureTime(5.9f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BrutalSmashTB1 or AID.BrutalSmashTB2)
            Source = Target = null;
    }
}
sealed class Powerslam(BossModule module) : Components.RaidwideCast(module, AID.Powerslam);
sealed class BloomingAbominationAdds(BossModule module) : Components.Adds(module, (uint)OID.BloomingAbomination);
sealed class SporeSac(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SporeSac, 8f);
sealed class Pollen(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pollen, 8f);
sealed class ItCameFromTheDirt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ItCameFromTheDirt, 6f);
sealed class TheUnpotted(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheUnpotted, new AOEShapeCone(60f, 15f.Degrees()));
sealed class QuarrySwamp(BossModule module) : Components.CastLineOfSightAOE(module, AID.QuarrySwamp, 60f, false)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies((uint)OID.BloomingAbomination);
}
sealed class CrossingCrosswinds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrossingCrosswinds, new AOEShapeCross(50f, 5f));
sealed class CrossingCrosswindsHint(BossModule module) : Components.CastInterruptHint(module, AID.CrossingCrosswinds, showNameInHint: true);
sealed class WindingWildwinds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindingWildwinds, new AOEShapeDonut(5f, 60f));
sealed class WindingWildwindsHint(BossModule module) : Components.CastInterruptHint(module, AID.WindingWildwinds, showNameInHint: true);
sealed class GlowerPower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GlowerPower, new AOEShapeRect(65f, 7f));
sealed class ElectrogeneticForce(BossModule module) : Components.SpreadFromCastTargets(module, AID.ElectrogeneticForce, 6f);
sealed class Sporesplosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Sporesplosion, 8f, maxCasts: 12);
sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, 25f, maxCasts: 2);
sealed class LashingLariat(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LashingLariat1, (uint)AID.LashingLariat2], new AOEShapeRect(70f, 16f));
sealed class Slaminator(BossModule module) : Components.CastTowers(module, AID.Slaminator, 8f, maxSoakers: 8);
sealed class PulpSmash(BossModule module) : Components.StackWithIcon(module, (uint)IconID.PulpSmash, AID.PulpSmash, 6f, 5.2f, minStackSize: 8, maxStackSize: 8);
sealed class AbominableBlink(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(24f), (uint)IconID.AbominableBlink, AID.AbominableBlink, 6.3f, centerAtTarget: true);
sealed class RevengeOfTheVines(BossModule module) : Components.RaidwideCast(module, AID.RevengeOfTheVines1);

sealed class M07NBruteAbombinatorStates : StateMachineBuilder
{
    public M07NBruteAbombinatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BrutalImpact>()
            .ActivateOnEnter<NeoBombarianSpecialKB>()
            .ActivateOnEnter<BrutishSwingCircle>()
            .ActivateOnEnter<BrutishSwingDonut>()
            .ActivateOnEnter<BrutishSwingCone1>()
            .ActivateOnEnter<BrutishSwingCone2>()
            .ActivateOnEnter<BrutishSwingDonutSegment1>()
            .ActivateOnEnter<BrutishSwingDonutSegment2>()
            .ActivateOnEnter<BrutalSmashTB>()
            .ActivateOnEnter<Powerslam>()
            .ActivateOnEnter<BloomingAbominationAdds>()
            .ActivateOnEnter<SporeSac>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<ItCameFromTheDirt>()
            .ActivateOnEnter<TheUnpotted>()
            .ActivateOnEnter<QuarrySwamp>()
            .ActivateOnEnter<CrossingCrosswinds>()
            .ActivateOnEnter<CrossingCrosswindsHint>()
            .ActivateOnEnter<WindingWildwinds>()
            .ActivateOnEnter<WindingWildwindsHint>()
            .ActivateOnEnter<GlowerPower>()
            .ActivateOnEnter<ElectrogeneticForce>()
            .ActivateOnEnter<Sporesplosion>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<LashingLariat>()
            .ActivateOnEnter<Slaminator>()
            .ActivateOnEnter<PulpSmash>()
            .ActivateOnEnter<AbominableBlink>()
            .ActivateOnEnter<RevengeOfTheVines>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, StatesType = typeof(M07NBruteAbombinatorStates), ObjectIDType = typeof(OID), ActionIDType = typeof(AID), IconIDType = typeof(IconID), PrimaryActorOID = (uint)OID.Boss, Contributors = "The Combat Reborn Team (Malediktus)", Expansion = BossModuleInfo.Expansion.Dawntrail, Category = BossModuleInfo.Category.Raid, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1023, NameID = 13756, SortOrder = 1, PlanLevel = 0)]
public sealed class M07NBruteAbombinator(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies((uint)OID.BloomingAbomination), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            ref var e = ref hints.PotentialTargets.Ref(i);
            e.Priority = e.Actor.OID == (uint)OID.BloomingAbomination ? 1 : 0;
        }
    }
}
