namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

public enum OID : uint
{
    Boss = 0x4C11, // R15.0, visual actor shared by both heads
    GreenHead = 0x4C12, // R15.0
    BlueHead = 0x4C13, // R15.0
    GreenHeadMechanic = 0x4C14, // R1.0
    BlueHeadMechanic = 0x4C15, // R1.0
    LightningOrb = 0x4C16, // R2.4
    IceOrb = 0x4C17, // R2.8
    ArcaneMatrix = 0x4B73,
    Helper = 0x233C
}

public enum AID : uint
{
    ThunderfrostTempest = 47735, // Boss->self, raidwide
    Buffet = 49726, // GreenHead/BlueHead->self, group assignment visual
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
    ArcaneBeacon = 49720, // ArcaneMatrix->self, 60x5 rect
    ArchaeofuryGreen = 47747, // Helper->player, range 6 circle
    ArchaeofuryBlue = 47748 // Helper->player, range 6 circle
}

public enum SID : uint
{
    EpicHero = 4192, // assigned to green head
    FatedHero = 4194, // assigned to blue head
    EpicVillain = 5400, // green head assignment controller
    FatedVillain = 5401, // blue head assignment controller
    EasterlyReprise = 5403, // wind from east, pushes west
    WesterlyReprise = 5404 // wind from west, pushes east
}

public enum TetherID : uint
{
    Buffet = 429
}

sealed class ThunderfrostTempest(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThunderfrostTempest);
sealed class PoisonBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PoisonBreath, 18f);
sealed class StormsBreath(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.StormsBreath, 14f);
sealed class TwoTerrors(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwoTerrors, new AOEShapeRect(40f, 5f));
sealed class ElementalClusters(BossModule module) : Components.SimpleAOEGroups(module,
    [(uint)AID.LightningCluster, (uint)AID.IceCluster], 15f);
sealed class ArcaneBeacon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArcaneBeacon, new AOEShapeRect(60f, 2.5f));
sealed class ArchaeofuryGreen(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ArchaeofuryGreen, 6f);
sealed class ArchaeofuryBlue(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ArchaeofuryBlue, 6f);

sealed class ElementalOrbSequence(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(15f);
    private readonly Dictionary<ulong, Actor> _orbs = [];
    private readonly HashSet<ulong> _firstWave = [];
    private readonly Dictionary<ulong, DateTime> _activations = [];
    private readonly List<AOEInstance> _active = [];
    private bool _firstWaveIdentified;
    private bool _firstWaveResolved;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _active.Clear();
        if (!_firstWaveIdentified)
            return [];

        foreach (var orb in _orbs.Values)
        {
            var current = _firstWaveResolved || _firstWave.Contains(orb.InstanceID);
            var activation = _activations.GetValueOrDefault(orb.InstanceID);
            _active.Add(new(Shape, orb.Position,
                activation: activation == default ? DateTime.MaxValue : activation,
                color: current ? Colors.Danger : Colors.AOE,
                risky: current,
                actorID: orb.InstanceID));
        }
        return CollectionsMarshal.AsSpan(_active);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is not ((uint)OID.LightningOrb) and not ((uint)OID.IceOrb))
            return;

        if (_orbs.Count == 0)
            Reset();
        _orbs[actor.InstanceID] = actor;
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (_orbs.Remove(actor.InstanceID))
            RemoveFromFirstWave(actor.InstanceID);
        _activations.Remove(actor.InstanceID);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        var orbOID = id switch
        {
            (uint)AID.LightningCluster => (uint)OID.LightningOrb,
            (uint)AID.IceCluster => (uint)OID.IceOrb,
            _ => 0u
        };
        if (orbOID != 0)
        {
            var activation = Module.CastFinishAt(spell, 2.4d);
            foreach (var orb in _orbs.Values)
            {
                if (orb.OID == orbOID && Shape.Check(spell.LocXZ, orb))
                {
                    _firstWave.Add(orb.InstanceID);
                    _activations[orb.InstanceID] = activation;
                    _firstWaveIdentified = true;
                }
            }
        }
        else if (id == (uint)AID.ThunderfrostTempest && _firstWaveIdentified)
        {
            var activation = Module.CastFinishAt(spell, 2.7d);
            foreach (var orb in _orbs.Values)
                if (!_firstWave.Contains(orb.InstanceID))
                    _activations[orb.InstanceID] = activation;
        }
        else if (id is (uint)AID.Shock or (uint)AID.HypothermalCombustion && _orbs.ContainsKey(caster.InstanceID))
        {
            _activations[caster.InstanceID] = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Shock or (uint)AID.HypothermalCombustion && _orbs.Remove(caster.InstanceID))
        {
            RemoveFromFirstWave(caster.InstanceID);
            _activations.Remove(caster.InstanceID);
            ++NumCasts;
        }
    }

    private void RemoveFromFirstWave(ulong instanceID)
    {
        if (_firstWave.Remove(instanceID) && _firstWave.Count == 0)
            _firstWaveResolved = true;
    }

    private void Reset()
    {
        _firstWave.Clear();
        _activations.Clear();
        _firstWaveIdentified = false;
        _firstWaveResolved = false;
    }
}

sealed class BuffetAssignments(BossModule module) : BossComponent(module)
{
    private readonly Dictionary<ulong, uint> _assignments = [];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_assignments.TryGetValue(actor.InstanceID, out var assignedOID))
            hints.Add(assignedOID == (uint)OID.BlueHead ? "攻击蓝头" : "攻击绿头", false);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_assignments.TryGetValue(pc.InstanceID, out var assignedOID))
        {
            var assigned = Module.Enemies(assignedOID).FirstOrDefault(actor => !actor.IsDestroyed);
            if (assigned != null)
                Arena.Actor(assigned, Colors.Safe);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_assignments.TryGetValue(actor.InstanceID, out var assignedOID))
            return;

        foreach (var enemy in hints.PotentialTargets)
            if (enemy.Actor.OID is (uint)OID.GreenHead or (uint)OID.BlueHead && enemy.Actor.OID != assignedOID)
                enemy.Priority = AIHints.Enemy.PriorityForbidden;
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.Buffet)
            return;

        var target = WorldState.Actors.Find(tether.Target);
        if (target?.OID == (uint)OID.BlueHeadMechanic)
            _assignments[source.InstanceID] = (uint)OID.BlueHead;
        else if (target?.OID == (uint)OID.GreenHeadMechanic)
            _assignments[source.InstanceID] = (uint)OID.GreenHead;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.EpicHero)
            _assignments[actor.InstanceID] = (uint)OID.GreenHead;
        else if (status.ID == (uint)SID.FatedHero)
            _assignments[actor.InstanceID] = (uint)OID.BlueHead;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.EpicVillain or (uint)SID.FatedVillain)
            _assignments.Clear();
    }
}

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
    private readonly Dictionary<ulong, (uint Status, DateTime Activation)> _assignments = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (!_assignments.TryGetValue(actor.InstanceID, out var assignment))
            return [];

        var direction = (assignment.Status == (uint)SID.EasterlyReprise ? -90f : 90f).Degrees();
        _source[0] = new(
            Arena.Center,
            20f,
            assignment.Activation,
            direction: direction,
            kind: Kind.DirForward);
        return _source;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.EasterlyReprise or (uint)SID.WesterlyReprise)
            _assignments[actor.InstanceID] = (status.ID, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (_assignments.GetValueOrDefault(actor.InstanceID).Status == status.ID)
            _assignments.Remove(actor.InstanceID);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HissingRepriseEast or (uint)AID.HissingRepriseWest)
        {
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
            .ActivateOnEnter<BuffetAssignments>()
            .ActivateOnEnter<ThunderfrostTempest>()
            .ActivateOnEnter<PoisonBreath>()
            .ActivateOnEnter<StormsBreath>()
            .ActivateOnEnter<TwoTerrors>()
            .ActivateOnEnter<HissingReprise>()
            .ActivateOnEnter<ElementalClusters>()
            .ActivateOnEnter<ElementalOrbSequence>()
            .ActivateOnEnter<BlazeSequence>()
            .ActivateOnEnter<ArcaneBeacon>()
            .ActivateOnEnter<ArchaeofuryGreen>()
            .ActivateOnEnter<ArchaeofuryBlue>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(TwoHeadedAevisStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
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
