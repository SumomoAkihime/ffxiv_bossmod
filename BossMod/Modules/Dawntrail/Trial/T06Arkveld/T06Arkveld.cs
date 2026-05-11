namespace BossMod.Dawntrail.Trial.T06Arkveld;

class Roar(BossModule module) : Components.RaidwideCast(module, AID.Roar);
class Roar1(BossModule module) : Components.RaidwideCast(module, AID.Roar1);

class ForgedFury(BossModule module) : Components.CastHint(module, null, "Raidwide")
{
    private readonly List<DateTime> _activations = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ForgedFury1 or AID.ForgedFury2 or AID.ForgedFury3)
        {
            _activations.Add(Module.CastFinishAt(spell));
            _activations.Sort();
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_activations.Count > 0)
            hints.Add(Hint);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activations.Count > 0)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), _activations[0]);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ForgedFury1 or AID.ForgedFury2 or AID.ForgedFury3)
        {
            ++NumCasts;
            if (_activations.Count > 0)
                _activations.RemoveAt(0);
        }
    }
}

class WildEnergy(BossModule module) : Components.SpreadFromCastTargets(module, AID.WildEnergy, 6);
class ChainbladeCharge(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Share2, AID.ChainbladeCharge2, 6, 8.4f, PartyState.MaxPartySize, PartyState.MaxPartySize);

class ChainbladeBlowLines(BossModule module) : Components.GroupedAOEs(module,
[
    AID.ChainbladeBlow1,
    AID.ChainbladeBlow2,
    AID.ChainbladeBlow4,
    AID.ChainbladeBlow5,
    AID.ChainbladeBlow7,
    AID.ChainbladeBlow8,
    AID.ChainbladeBlow10,
    AID.ChainbladeBlow11
], new AOEShapeRect(40, 2));

class WyvernsRadianceCleave(BossModule module) : Components.GroupedAOEs(module,
[
    AID.WyvernsRadiance,
    AID.WyvernsRadiance1,
    AID.WyvernsRadiance16,
    AID.WyvernsRadiance17
], new AOEShapeRect(80, 14));

class GuardianResonanceRect(BossModule module) : Components.StandardAOEs(module, AID.GuardianResonance, new AOEShapeRect(40, 8));
class Rush(BossModule module) : Components.ChargeAOEs(module, AID.Rush, 6);
class Concentric1(BossModule module) : Components.StandardAOEs(module, AID.WyvernsRadiance6, 8);
class Concentric2(BossModule module) : Components.StandardAOEs(module, AID.WyvernsRadiance7, new AOEShapeDonut(8, 14));
class Concentric3(BossModule module) : Components.StandardAOEs(module, AID.WyvernsRadiance8, new AOEShapeDonut(14, 20));
class Concentric4(BossModule module) : Components.StandardAOEs(module, AID.WyvernsRadiance9, new AOEShapeDonut(20, 26));
class WyvernsOuroblade(BossModule module) : Components.GroupedAOEs(module, [AID.WyvernsOuroblade1, AID.WyvernsOuroblade3], new AOEShapeCone(40, 90.Degrees()));
class SteeltailThrust(BossModule module) : Components.GroupedAOEs(module, [AID.SteeltailThrust, AID.SteeltailThrust1], new AOEShapeRect(60, 3));

class ResonanceTowerSmall(BossModule module) : Components.CastTowers(module, AID.GuardianResonance2, 2, 1, 1);
class ResonanceTowerLarge(BossModule module) : Components.CastTowers(module, AID.GuardianResonance3, 4, 1, 1, AIHints.PredictedDamageType.Tankbuster)
{
    public override void Update()
    {
        base.Update();

        BitMask forbidden = default;
        foreach (var (slot, actor) in Module.Raid.WithSlot())
        {
            if (actor.Role != Role.Tank)
                forbidden.Set(slot);
        }

        for (var i = 0; i < Towers.Count; ++i)
        {
            var tower = Towers[i];
            tower.ForbiddenSoakers = forbidden;
            Towers[i] = tower;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (actor.Role == Role.Tank)
            return;

        foreach (var t in Towers)
            hints.AddForbiddenZone(ShapeContains.Circle(t.Position, Radius), t.Activation);
    }
}

class CrackedCrystalSmall(BossModule module) : Components.StandardAOEs(module, AID.WyvernsRadiance10, 6);
class CrackedCrystalLarge(BossModule module) : Components.StandardAOEs(module, AID.WyvernsRadiance11, 12);

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1043, NameID = 14237)]
public class GuardianArkveld(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
