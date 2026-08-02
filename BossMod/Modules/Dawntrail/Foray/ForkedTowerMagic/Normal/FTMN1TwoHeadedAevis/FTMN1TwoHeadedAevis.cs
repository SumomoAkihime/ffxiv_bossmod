namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

public enum OID : uint
{
    Boss = 0x4C11, // R15.0, visual actor shared by both heads
    GreenHead = 0x4C12, // R15.0
    BlueHead = 0x4C13, // R15.0
    LightningOrb = 0x4C16, // R2.4
    IceOrb = 0x4C17, // R2.8
    ArcaneMatrix = 0x4B73,
    Helper = 0x233C
}

public enum AID : uint
{
    ThunderfrostTempest = 47735, // Boss->self, raidwide
    PoisonBreath = 47617, // Helper->self, range 18 circle
    StormsBreath = 48243, // Helper->self, knockback 14
    TwoTerrors = 50658, // Helper->self, 40x10 rect
    HissingRepriseVisual = 49722, // GreenHead/BlueHead->self, 3.0s cast
    HissingRepriseEast = 49724, // Helper->players, directional knockback 20
    HissingRepriseWest = 49725, // Helper->players, directional knockback 20
    LightningCluster = 50697, // Helper->self, range 15 circle
    IceCluster = 50698, // Helper->self, range 15 circle
    Shock = 47706, // LightningOrb->self, range 15 circle
    HypothermalCombustion = 47707, // IceOrb->self, range 15 circle
    Blazeloop = 47660, // Helper->self, range 5-60 donut
    Blaze1 = 50703, // Helper->self, range 5 circle
    Blaze2 = 50704, // Helper->self, range 5 circle
    Blaze3 = 50705, // Helper->self, range 5 circle
    ArcaneBeacon = 49720 // ArcaneMatrix->self, 60x5 rect
}

sealed class ThunderfrostTempest(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThunderfrostTempest);
sealed class PoisonBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PoisonBreath, 18f);
sealed class StormsBreath(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.StormsBreath, 14f);
sealed class TwoTerrors(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwoTerrors, new AOEShapeRect(40f, 5f));
sealed class ElementalClusters(BossModule module) : Components.SimpleAOEGroups(module,
    [(uint)AID.LightningCluster, (uint)AID.IceCluster, (uint)AID.Shock, (uint)AID.HypothermalCombustion], 15f);
sealed class ArcaneBeacon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArcaneBeacon, new AOEShapeRect(60f, 2.5f));

sealed class BlazeSequence(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Circle = new(5f);
    private static readonly AOEShapeDonut Donut = new(5f, 60f);
    private readonly List<(AOEInstance AOE, bool Donut)> _aoes = [];
    private readonly AOEInstance[] _active = new AOEInstance[2];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Math.Min(2, _aoes.Count);
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i].AOE;
            aoe.Color = i == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = i == 0;
            _active[i] = aoe;
        }
        return _active.AsSpan(0, count);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.Blaze1 or (uint)AID.Blaze2 or (uint)AID.Blaze3)
        {
            var activation = Module.CastFinishAt(spell);
            _aoes.Add((new(Circle, spell.LocXZ, activation: activation), false));
            _aoes.Add((new(Donut, spell.LocXZ, activation: activation.AddSeconds(2.6d)), true));
            Sort();
        }
        else if (id == (uint)AID.Blazeloop)
        {
            var index = Find(spell.LocXZ, true);
            if (index >= 0)
            {
                var preview = _aoes[index];
                preview.AOE.Activation = Module.CastFinishAt(spell);
                _aoes[index] = preview;
                Sort();
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var donut = spell.Action.ID == (uint)AID.Blazeloop;
        if (!donut && spell.Action.ID is not ((uint)AID.Blaze1) and not ((uint)AID.Blaze2) and not ((uint)AID.Blaze3))
            return;

        var index = Find(caster.Position, donut);
        if (index >= 0)
            _aoes.RemoveAt(index);
        ++NumCasts;
    }

    private int Find(WPos position, bool donut)
    {
        var index = _aoes.FindIndex(preview => preview.Donut == donut && (preview.AOE.Origin - position).LengthSq() < 1f);
        return index >= 0 ? index : _aoes.FindIndex(preview => preview.Donut == donut);
    }

    private void Sort() => _aoes.Sort((left, right) => left.AOE.Activation.CompareTo(right.AOE.Activation));
}

sealed class HissingReprise(BossModule module) : Components.GenericKnockback(module)
{
    private readonly Knockback[] _source = new Knockback[1];
    private readonly Dictionary<ulong, bool> _assignments = [];
    private DateTime _activation;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_activation == default)
            return [];

        if (!_assignments.TryGetValue(actor.InstanceID, out var east))
        {
            east = actor.Position.X > Arena.Center.X;
            _assignments[actor.InstanceID] = east;
        }
        _source[0] = new(
            Arena.Center + new WDir(east ? 20f : -20f, 0f),
            20f,
            _activation,
            direction: (east ? -90f : 90f).Degrees(),
            kind: Kind.DirForward);
        return _source;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HissingRepriseVisual && _activation == default)
        {
            _assignments.Clear();
            _activation = Module.CastFinishAt(spell, 13.8d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HissingRepriseEast or (uint)AID.HissingRepriseWest)
        {
            _activation = default;
            _assignments.Clear();
            ++NumCasts;
        }
    }
}

sealed class TwoHeadedAevisStates : StateMachineBuilder
{
    public TwoHeadedAevisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThunderfrostTempest>()
            .ActivateOnEnter<PoisonBreath>()
            .ActivateOnEnter<StormsBreath>()
            .ActivateOnEnter<TwoTerrors>()
            .ActivateOnEnter<HissingReprise>()
            .ActivateOnEnter<ElementalClusters>()
            .ActivateOnEnter<BlazeSequence>()
            .ActivateOnEnter<ArcaneBeacon>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(TwoHeadedAevisStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.GreenHead,
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093,
    NameID = 14490,
    SortOrder = 1)]
public sealed class TwoHeadedAevis(WorldState ws, Actor primary) : BossModule(ws, primary, new(-900f, 700f), new ArenaBoundsSquare(20f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.GreenHead));
        Arena.Actors(Enemies((uint)OID.BlueHead));
    }
}
