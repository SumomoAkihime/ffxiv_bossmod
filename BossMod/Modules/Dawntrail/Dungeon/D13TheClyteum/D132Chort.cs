namespace BossMod.Dawntrail.Dungeon.D13TheClyteum.D132Chort;

public enum OID : uint
{
    Chort = 0x4C3F,
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 45308, // Chort->player, no cast, single-target
    UnknownWeaponskill = 48888, // Chort->location, no cast, single-target

    BasicVomit = 50361, // Chort->self, 5.0s cast, range 50 120.000-degree cone

    RipplesOfGloom = 48884, // Chort->self, 5.0s cast, single-target
    RipplesOfGloom1 = 50408, // Helper->self, 5.7s cast, range 40 circle

    MortifyingFlesh = 48871, // Chort->self, 5.0s cast, single-target //From inside arena: Rectangle across half arena
    MortifyingFlesh1 = 50400, // Helper->self, 3.0s cast, range 40 width 16 rect //From outside arena.  rect across whole arena
    MortifyingFlesh2 = 48876, // Helper->self, 3.0s cast, range 40 width 16 rect //From outside arena.  rect across whole arena
    MortifyingFlesh3 = 48869, // Chort->self, 5.0s cast, single-target
    MortifyingFlesh4 = 48872, // Chort->self, no cast, single-target

    BodyweightExorcism = 48877, // Chort->self, no cast, single-target
    BodyweightExorcism1 = 48878, // Helper->self, 2.8s cast, range 20 circle : knockback from center
    BodyweightExorcism2 = 48879, // Chort->location, 5.0s cast, single-target : 4 towers cast animation
    BodyweightExorcism4 = 48880, // Chort->location, no cast, single-target
    BodyweightExorcism5 = 48881, // Chort->self, no cast, single-target
    BodyweightExorcismTower = 48882, // Helper->location, 6.0s cast, range 4 circle : soak tower spawns

    EvilEmission = 50417, // Chort->self, 4.5s cast, single-target
    EvilEmission1 = 48885, // Helper->player, 5.0s cast, range 5 circle

    ProfanePressure = 48886, // Chort->self, 4.5s cast, single-target
    ProfanePressure1 = 48887, // Helper->players, 5.0s cast, range 6 circle : stack marker
}

public enum IconID : uint
{
    EvilEmissionSpread = 558, // player->self : spread marker
    ProfanePressureIcon = 161 // stack marker
}

sealed class MortifyingFlesh(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MortifyingFlesh, new AOEShapeRect(40f, 4f));
sealed class MortifyingFleshAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.MortifyingFlesh1, (uint)AID.MortifyingFlesh2], new AOEShapeRect(40f, 8f));

sealed class BodyWeightExorcism1(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.BodyweightExorcism1, 8)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }
}

sealed class BasicVomit(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BasicVomit, new AOEShapeCone(50f, 60f.Degrees()));

sealed class BodyweightTower(BossModule module) : Components.CastTowers(module, AID.BodyweightExorcismTower, 4f, 4, 4)
{ }

sealed class EvilEmission(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.EvilEmissionSpread, AID.EvilEmission1, 4, 5);

sealed class ProfanePressure(BossModule module) : Components.StackWithCastTargets(module, AID.ProfanePressure1, 6f, 4);

sealed class D132ChortStates : StateMachineBuilder
{
    public D132ChortStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MortifyingFlesh>()
            .ActivateOnEnter<MortifyingFleshAOEs>()
            .ActivateOnEnter<BodyWeightExorcism1>()
            .ActivateOnEnter<BasicVomit>()
            .ActivateOnEnter<BodyweightTower>()
            .ActivateOnEnter<ProfanePressure>()
            .ActivateOnEnter<EvilEmission>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
    StatesType = typeof(D132ChortStates),
    ConfigType = null, // replace null with typeof(ChortConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Chort,
    Contributors = "Wen",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Dungeon,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1011u,
    NameID = 14734u,
    SortOrder = 1,
    PlanLevel = 0)]
public sealed class Chort(WorldState ws, Actor primary) : BossModule(ws, primary, new(660f, -141f), new ArenaBoundsCircle(15f));
