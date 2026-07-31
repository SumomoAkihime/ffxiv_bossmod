namespace BossMod.Dawntrail.Foray.MagicTowerNormal.MTN4Catalogue;

public enum OID : uint
{
    Boss = 0x4B5F, // R7.5
    HolyLance = 0x4B62,
    PropheticPhenomenon = 0x4B63,
    CataloguePhantom = 0x4B6F,
    Helper = 0x233C
}

public enum AID : uint
{
    Flare = 48415, // Boss->self, raidwide visual
    RomeosBallad = 48385, // Helper->self, range 15 circle
    Aim = 48387, // Helper->self, range 11 circle
    OmniElements = 48394, // Boss->self, raidwide
    ElementaryChemistry = 48905, // Helper->self, 15x15 rect
    ShockwaveVisual = 48405, // HolyLance->self, knockback 9
    Iainuki = 48389, // CataloguePhantom->self, range 30 60-degree cone
    WindSlash = 48391, // CataloguePhantom->self, range 30 60-degree cone
    AllConsumingFlames = 48420, // Helper->player, range 6 circle
    Starfall = 48413, // PropheticPhenomenon->self, range 10 circle
    Cleansing = 48414 // PropheticPhenomenon->self, range 3-15 donut
}

public enum IconID : uint
{
    AllConsumingFlames = 466
}

sealed class Flare(BossModule module) : Components.RaidwideCast(module, (uint)AID.Flare);
sealed class RomeosBallad(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RomeosBallad, 15f);
sealed class Aim(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Aim, 11f);
sealed class OmniElements(BossModule module) : Components.RaidwideCast(module, (uint)AID.OmniElements);
sealed class ElementaryChemistry(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElementaryChemistry, new AOEShapeRect(15f, 7.5f));
sealed class Iainuki(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Iainuki, new AOEShapeCone(30f, 30f.Degrees()));
sealed class WindSlash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindSlash, new AOEShapeCone(30f, 30f.Degrees()));
sealed class AllConsumingFlames(BossModule module) : Components.SpreadFromIcon(module,
    (uint)IconID.AllConsumingFlames, (uint)AID.AllConsumingFlames, 6f, 5.1d);
sealed class Starfall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Starfall, 10f);
sealed class Cleansing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Cleansing, new AOEShapeDonut(3f, 15f));

sealed class Shockwave(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _sources = new(3);
    private readonly Knockback[] _nearest = new Knockback[1];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_sources.Count == 0)
            return [];

        var nearest = _sources[0];
        var nearestDistance = (actor.Position - nearest.Origin).LengthSq();
        for (var i = 1; i < _sources.Count; ++i)
        {
            var distance = (actor.Position - _sources[i].Origin).LengthSq();
            if (distance < nearestDistance)
            {
                nearest = _sources[i];
                nearestDistance = distance;
            }
        }
        _nearest[0] = nearest;
        return _nearest;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShockwaveVisual)
            _sources.Add(new(caster.Position, 9f, Module.CastFinishAt(spell), actorID: caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShockwaveVisual)
        {
            _sources.RemoveAll(s => s.ActorID == caster.InstanceID);
            ++NumCasts;
        }
    }
}

sealed class MTN4CatalogueStates : StateMachineBuilder
{
    public MTN4CatalogueStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Flare>()
            .ActivateOnEnter<RomeosBallad>()
            .ActivateOnEnter<Aim>()
            .ActivateOnEnter<OmniElements>()
            .ActivateOnEnter<ElementaryChemistry>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<Iainuki>()
            .ActivateOnEnter<WindSlash>()
            .ActivateOnEnter<AllConsumingFlames>()
            .ActivateOnEnter<Starfall>()
            .ActivateOnEnter<Cleansing>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(MTN4CatalogueStates),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Boss,
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093,
    NameID = 14717,
    SortOrder = 4)]
public sealed class MTN4Catalogue(WorldState ws, Actor primary) : BossModule(ws, primary, new(0f, -628f), new ArenaBoundsCircle(30f));
