namespace BossMod.Stormblood.Alliance.A33ThunderGod;

class Colosseum : BossComponent
{
    public Colosseum(BossModule module) : base(module) => SetArena(false);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Colosseum)
            SetArena(true);
        else if (spell.Action.ID == (uint)AID.BalanceAsunder2)
            SetArena(false);
    }

    private void SetArena(bool upper)
    {
        Arena.Bounds = upper ? A33ThunderGod.UpperBounds : A33ThunderGod.DefaultBounds;
        Arena.Center = upper ? A33ThunderGod.ArenaCenter : A33ThunderGod.DefaultBounds.Center;
    }
}

class HallowedBolt(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _circle = new(15f);
    private static readonly AOEShapeDonut _donut = new(15f, 30f);
    private readonly List<AOEInstance> _casts = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.HallowedBolt1 => _circle,
            (uint)AID.HallowedBolt2 => _donut,
            _ => null
        };
        if (shape != null)
        {
            _casts.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            _casts.Add(new(shape, caster.Position, activation: Module.CastFinishAt(spell), actorID: caster.InstanceID));
            Refresh();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.HallowedBolt1 or (uint)AID.HallowedBolt2)
            RemoveCaster(caster);
    }

    public override void OnActorDestroyed(Actor actor) => RemoveCaster(actor);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Colosseum or (uint)AID.BalanceAsunder2)
        {
            _casts.Clear();
            _aoes.Clear();
        }
    }

    private void RemoveCaster(Actor caster)
    {
        if (_casts.RemoveAll(aoe => aoe.ActorID == caster.InstanceID) != 0)
            Refresh();
    }

    private void Refresh()
    {
        _casts.Sort((a, b) => a.Activation.CompareTo(b.Activation));
        _aoes.Clear();
        // Both circle -> donut and donut -> circle occur; show the first unfinished cast at each origin.
        foreach (var aoe in _casts)
            if (!_aoes.Any(active => active.Origin.AlmostEqual(aoe.Origin, 1f)))
                _aoes.Add(aoe);
    }
}

class TGHolySword(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _platformCircle = new(15f);
    private static readonly AOEShapeCircle _platformCircleLarge = new(16f);
    private static readonly AOEShapeCircle _centerCircle = new(26f);
    private static readonly AOEShapeDonut _centerDonut = new(20f, 35f);
    private static readonly WPos[] _platforms =
    [
        new(-612.5f, -578.4f), new(-587.5f, -578.4f), new(-575f, -600f),
        new(-587.5f, -621.5f), new(-612.5f, -621.5f), new(-625f, -600f)
    ];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TGHolySword1:
                AddPlatforms(_platformCircle, spell, [1, 3, 5]);
                break;
            case AID.TGHolySword2:
                AddPlatforms(_platformCircle, spell, [0, 2, 4]);
                break;
            case AID.TGHolySword4:
                AddPlatformSequence(spell);
                break;
            case AID.TGHolySword5:
                _aoes.Add(new(_centerCircle, Module.Center, activation: Module.CastFinishAt(spell, 0.2d)));
                break;
            case AID.TGHolySword6:
                _aoes.Add(new(_centerDonut, Module.Center, activation: Module.CastFinishAt(spell, 0.4d)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HallowedBoltAOE2 or (uint)AID.DivineRuination1 or (uint)AID.NorthswainsStrike or (uint)AID.JudgmentBlade)
        {
            var index = _aoes.FindIndex(aoe => aoe.Origin.AlmostEqual(caster.Position, 1f));
            if (index >= 0)
                _aoes.RemoveAt(index);
        }
    }

    private void AddPlatforms(AOEShape shape, ActorCastInfo spell, int[] indices)
    {
        var activation = Module.CastFinishAt(spell, 1d);
        foreach (var index in indices)
            _aoes.Add(new(shape, _platforms[index], activation: activation));
    }

    private void AddPlatformSequence(ActorCastInfo spell)
    {
        for (var i = 0; i < 3; ++i)
        {
            var direction = spell.Rotation + (30f - 60f * i).Degrees();
            var index = Enumerable.Range(0, _platforms.Length).MinBy(j => MathF.Abs((Angle.FromDirection(_platforms[j] - Module.Center) - direction).Normalized().Rad));
            _aoes.Add(new(_platformCircleLarge, _platforms[index], activation: Module.CastFinishAt(spell, 1d + 3.1d * i)));
        }
    }
}

class Duskblade(BossModule module) : BossComponent(module)
{
    private const float Radius = 3f;
    private bool _active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Duskblade)
            _active = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Duskblade)
            _active = false;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_active)
            return;

        foreach (var tower in Module.Enemies((uint)OID.DuskbladeTower))
        {
            var numInside = Module.Raid.WithoutSlot().InRadius(tower.Position, Radius).Count();
            Arena.ZoneCircleOutline(tower.Position, Radius, numInside >= 3 ? Colors.Safe : Colors.Danger, 2f);
        }
    }
}

class Shadowblade(BossModule module) : Components.BaitAwayIcon(module, 5f, (uint)IconID.Icon170, (uint)AID.Shadowblade2, 8.1d);
class CrushWeaponTarget(BossModule module) : Components.BaitAwayIcon(module, 6f, (uint)IconID.Nox, (uint)AID.CrushWeapon3, 8.1d);
class CrushWeaponAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrushWeapon2, 6f);
class HallowedBoltStack(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Icon62, (uint)AID.HallowedBolt5, 6f, 5.1d, 8, 8);

class CrushArmor(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _helpers = module.Enemies((uint)OID.Helper);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var source in TetherSources())
        {
            var target = WorldState.Actors.Find(source.Tether.Target);
            if (target != null)
            {
                Arena.AddLine(source.Position, target.Position, Colors.Danger);
                Arena.ZoneCircleOutline(target.Position, 1f, Colors.Danger);
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => TetherSources().Any(source => source.Tether.Target == player.InstanceID) ? PlayerPriority.Danger : PlayerPriority.Normal;

    private IEnumerable<Actor> TetherSources()
    {
        if (Module.PrimaryActor.Tether.ID == (uint)TetherID.Tether84)
            yield return Module.PrimaryActor;
        foreach (var helper in _helpers.Tethered(TetherID.Tether84))
            yield return helper;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7899)] //7917
public class A33ThunderGod(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    public static readonly WPos ArenaCenter = new(-600f, -600f);
    // z3r3_a3_boss2.mdl: upper floor surface radius 35, placed 16 units above the lower arena.
    public static readonly ArenaBoundsCircle UpperBounds = new(35f);
    public static readonly ArenaBoundsCustom DefaultBounds = new([new Circle(new(-612.5f, -578.4f), 10), new Circle(new(-587.5f, -578.4f), 10), new Circle(new(-575, -600), 10), new Circle(new(-587.5f, -621.5f), 10), new Circle(new(-612.5f, -621.5f), 10), new Circle(new(-625, -600), 10), new Donut(new(-600, -600), 20, 27)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.EphemeralKnight));
    }
}
