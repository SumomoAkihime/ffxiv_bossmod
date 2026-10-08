namespace BossMod.RealmReborn.Alliance.A33Cerberus;

static class Belly
{
    public const float HeightThreshold = -100f;

    public static bool InBelly(Actor actor) => actor.PosRot.Y < HeightThreshold;
    public static bool IsMini(Actor actor) => actor.FindStatus((uint)SID.Minimum) != null;

    public static bool WantsBelly(PartyState party, Actor actor)
        => actor.Role is Role.Melee or Role.Ranged || party.Alliance == AllianceLetter.B; // A = all get in the belly, B = belly is where you should go, C = could you please get in the belly
}

class BellyArena(BossModule module) : BossComponent(module)
{
    public override void Update()
    {
        var pc = Raid.Player();
        if (pc == null)
            return;
        if (Belly.InBelly(pc))
        {
            Arena.Center = A33Cerberus.BellyCenter;
            Arena.Bounds = A33Cerberus.BellyBounds;
        }
        else
        {
            Arena.Center = A33Cerberus.OutsideCenter;
            Arena.Bounds = A33Cerberus.OutsideBounds;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Belly.InBelly(actor))
            return;

        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.StomachWall or (uint)OID.Unknown => 2,
                _ => AIHints.Enemy.PriorityForbidden
            };
        }
    }
}

class TailBlow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailBlow, new AOEShapeCone(19.8f, 45.Degrees()))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Belly.InBelly(actor) ? [] : base.ActiveAOEs(slot, actor);
}

class SulphurousBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SulphurousBreath, new AOEShapeRect(35.8f, 3))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Belly.InBelly(actor) ? [] : base.ActiveAOEs(slot, actor);
}

class SulphurousBreathHelper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SulphurousBreathHelper, new AOEShapeRect(40.5f, 3))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Belly.InBelly(actor) ? [] : base.ActiveAOEs(slot, actor);
}

class HoundOutOfHell(BossModule module) : Components.ChargeAOEs(module, (uint)AID.HoundOutOfHell, 7)
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Belly.InBelly(actor) ? [] : base.ActiveAOEs(slot, actor);
}

class LightningBoltCharge(BossModule module) : Components.ChargeAOEs(module, (uint)AID.LightningBoltCharge, 2)
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Belly.InBelly(actor) ? [] : base.ActiveAOEs(slot, actor);
}

class HexEye(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HexEye, new AOEShapeCircle(4.8f));
class Ululation(BossModule module) : Components.RaidwideCast(module, (uint)AID.Ululation, "Stay near allies!")
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Belly.InBelly(actor))
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class Wolfsbane(BossModule module) : Components.Adds(module, (uint)OID.Wolfsbane, 1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Belly.InBelly(actor))
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class GastricJuiceAdd(BossModule module) : Components.Adds(module, (uint)OID.GastricJuice)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Belly.InBelly(actor))
            return;
        foreach (var a in ActiveActors)
            hints.SetPriority(a, AIHints.Enemy.PriorityForbidden);
    }
}

class Electrons(BossModule module) : Components.Adds(module, (uint)OID.Electron);

class StomachAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.StomachWall, (uint)OID.Unknown], 2)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Belly.InBelly(actor)) // the belly adds spawn with gastric juice
        {
            foreach (var a in ActiveActors)
                hints.SetPriority(a, AIHints.Enemy.PriorityForbidden);
            return;
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

static class BellyAOEs
{
    public static void AddHints(ReadOnlySpan<Components.GenericAOEs.AOEInstance> aoes, Actor actor, bool inverted, BossComponent.TextHints hints, string enter, string leave)
    {
        var inside = false;
        foreach (ref readonly var aoe in aoes)
            inside |= aoe.Check(actor.Position);
        if (aoes.Length > 0 && inside != inverted)
            hints.Add(inverted ? enter : leave);
    }

    public static void AddAIHints(ReadOnlySpan<Components.GenericAOEs.AOEInstance> aoes, bool inverted, AIHints hints)
    {
        var shapes = new List<ShapeDistance>();
        var activation = DateTime.MaxValue;
        foreach (ref readonly var aoe in aoes)
        {
            shapes.Add(aoe.Shape.Distance(aoe.Origin, aoe.Rotation));
            if (aoe.Activation < activation)
                activation = aoe.Activation;
        }
        if (shapes.Count > 0)
            hints.AddForbiddenZone(inverted ? new SDInvertedUnion([.. shapes]) : new SDUnion([.. shapes]), activation);
    }
}

class Mini(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mini, new AOEShapeCircle(9))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Belly.InBelly(actor))
            return [];
        var inverted = Belly.WantsBelly(Raid, actor) && !Belly.IsMini(actor);
        foreach (ref var aoe in CollectionsMarshal.AsSpan(Casters))
        {
            aoe.Risky = !inverted;
            aoe.Color = inverted ? Colors.SafeFromAOE : default;
        }
        return CollectionsMarshal.AsSpan(Casters);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
        => BellyAOEs.AddHints(ActiveAOEs(slot, actor), actor, Belly.WantsBelly(Raid, actor) && !Belly.IsMini(actor), hints, "Get hit by Mini!", "GTFO from Mini!");

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        => BellyAOEs.AddAIHints(ActiveAOEs(slot, actor), Belly.WantsBelly(Raid, actor) && !Belly.IsMini(actor), hints);
}

class Slabber(BossModule module) : Components.VoidzoneAtCastTarget(module, 8, (uint)AID.Slabber, m => m.Enemies((uint)OID.SlabberVoidzone).Where(a => a.EventState != 7), 0)
{
    private readonly List<AOEInstance> _visible = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _visible.Clear();
        if (Belly.InBelly(actor))
            return [];
        var inverted = Belly.IsMini(actor);
        foreach (var aoe in base.ActiveAOEs(slot, actor))
            _visible.Add(aoe with { Risky = !inverted, Color = inverted ? Colors.SafeFromAOE : default });
        return CollectionsMarshal.AsSpan(_visible);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
        => BellyAOEs.AddHints(ActiveAOEs(slot, actor), actor, Belly.IsMini(actor), hints, "Go to Slabber!", "GTFO from Slabber!");

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        => BellyAOEs.AddAIHints(ActiveAOEs(slot, actor), Belly.IsMini(actor), hints);
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3234)]
public class A33Cerberus(WorldState ws, Actor primary) : BossModule(ws, primary, OutsideCenter, OutsideBounds)
{
    public static readonly WPos OutsideCenter = new(0, -198);
    public static readonly ArenaBoundsRect OutsideBounds = new(20, 40);
    public static readonly WPos BellyCenter = new(1, -200);
    // this isn't accurate but it includes all the stomach walls and is *most* of the arena. cba shaping the actual shape
    public static readonly ArenaBoundsCircle BellyBounds = new(13);
}
