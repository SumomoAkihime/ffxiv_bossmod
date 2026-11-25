namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE201ABeastUnleashed;

public enum OID : uint
{
    AtlasCarbuncle = 0x4C4F, // R9.067, x1
    AtlasCarbuncleHelper = 0x233C, // R0.500, x20, Helper type
    AtlasCarbuncle1 = 0x4D88, // R1.000, x1
    TopazStone = 0x4C50, // R1.000, x12
    TopazOutline1 = 0x1EC045, // R0.500, x1, EventObj type
    TopazOutline2 = 0x1EC046 // R0.500, x2, EventObj type
}

public enum AID : uint
{
    AutoAttack = 50852, // AtlasCarbuncle->player, no cast, single-target
    SonicHowl = 48298, // AtlasCarbuncle->self, 5.0s cast, ???
    SonicHowl1 = 49505, // AtlasCarbuncleHelper->self, no cast, ???
    TailToClaw = 48295, // AtlasCarbuncle->self, 6.0s cast, range 40 180.000-degree cone
    TailToClaw1 = 48297, // AtlasCarbuncle->self, no cast, range 45 ?-degree cone

    SpinebreakingStampedeCast = 48291, // AtlasCarbuncle->location, 8.0s cast, ???
    SpinebreakingStampedeMiddleVisual = 48289, // Helper->self, 2.5s cast, range 40 width 60 rect
    SpinebreakingStampedeMiddle = 49507, // Helper->self, no cast, ???
    SpinebreakingStampedeCircleVisual = 48288, // Helper->self, 2.5s cast, range 60 circle
    SpinebreakingStampedeCircle = 49506, // Helper->self, no cast, ???
    SpinebreakingStampedeTeleport = 48299, // AtlasCarbuncle->location, no cast, single-target
    SpinebreakingStampedeTeleport1 = 48292, // AtlasCarbuncle->location, no cast, ???

    DeathWall = 49104, // AtlasCarbuncle1->self, no cast, ???
    ClawToTail = 48294, // AtlasCarbuncle->self, 6.0s cast, range 40 180.000-degree cone
    ClawToTail1 = 48296, // AtlasCarbuncle->self, no cast, range 45 ?-degree cone
    TopazStones = 48280, // AtlasCarbuncle->self, 3.0s cast, single-target
    TopazRay1 = 48281, // TopazStone->self, 3.0s cast, range 4 circle
    TopazRay2 = 48282, // TopazStone->self, 3.0s cast, range 4 circle
    UnknownAbility1 = 50461, // AtlasCarbuncle->self, no cast, single-target
    WeaponskillRubyGlow = 48284, // AtlasCarbuncle->self, 3.0s cast, ???
    AbilityRubyGlow = 50637, // AtlasCarbuncleHelper->self, no cast, ???
    ReflectiveCoat = 50418, // AtlasCarbuncle->self, 3.0s cast, single-target
    RubyReflection = 48287, // AtlasCarbuncleHelper->self, no cast, range 40 width 40 rect
    RubyReflection1 = 48286, // AtlasCarbuncleHelper->self, no cast, range 40 width 40 rect
    RubyReflection2 = 48285, // Helper->self, no cast, range 20 width 20 rect
}

sealed class SonicHowl(BossModule module) : Components.RaidwideCast(module, (uint)AID.SonicHowl);
sealed class RubyGlow(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.WeaponskillRubyGlow, (uint)AID.AbilityRubyGlow, 0.7d); // ported from upstream

sealed class TailToClaw(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCone cone = new(45f, 90f.Degrees());
    private readonly RubyReflection rubyaoe = module.FindComponent<RubyReflection>()!;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ClawToTail or (uint)AID.TailToClaw)
        {
            var loc = spell.LocXZ;
            var rot = spell.Rotation;
            var act = Module.CastFinishAt(spell);

            AddAOE();
            AddAOE(180f.Degrees(), 3.1d);

            void AddAOE(Angle offset = default, double delay = default)
            {
                var pos = delay != default ? loc - 5f * rot.ToDirection() : loc;
                var rot2 = rot + offset;
                _aoes.Add(new(cone, pos, rot2, delay != default ? act.AddSeconds(delay) : act, shapeDistance: cone.Distance(pos, rot2)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count is var count && count != 0 && spell.Action.ID is (uint)AID.TailToClaw or (uint)AID.TailToClaw1 or (uint)AID.ClawToTail or (uint)AID.ClawToTail1)
        {
            _aoes.RemoveAt(0);
            if (count == 2)
            {
                ref var aoe2 = ref _aoes.Ref(0);
                var rot = aoe2.Rotation;
                aoe2.Origin -= 5f * rot.ToDirection();
                aoe2.ShapeDistance = cone.Distance(aoe2.Origin, rot);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (rubyaoe.AOEs.Count != 0)
        {
            return [];
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }
}

sealed class TopazRay(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TopazRay1, (uint)AID.TopazRay2], 4f)
{
    public readonly List<Actor> Actors = new(10);
    private RubyReflection? rubyreflection;
    private bool intercept;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.TopazStone && id == 0x2489)
        {
            Actors.Add(actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TopazRay1 or (uint)AID.TopazRay2)
        {
            Actors.Clear();
            intercept = false;
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID is var oid && oid == (uint)OID.TopazOutline1 && state == 0x00010002u || oid == (uint)OID.TopazOutline2 && state is 0x00100020u or 0x01000200u)
        {
            intercept = true;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (!intercept)
        {
            base.OnCastStarted(caster, spell);
        }
        else if (spell.Action.ID is (uint)AID.TopazRay1 or (uint)AID.TopazRay2)
        {
            rubyreflection ??= Module.FindComponent<RubyReflection>()!;
            var center = Arena.Center;
            var origin = spell.LocXZ;
            AOEShape shape;
            var aoes = CollectionsMarshal.AsSpan(rubyreflection.AOEs);
            var count = aoes.Length;
            var act = Module.CastFinishAt(spell);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Shape is AOEShapeRect && Intersect.CircleAARectEdge(origin - aoe.Origin, 4f, 10f, 10f)
                    || aoe.Shape is AOEShapeCustom custom && custom.Polygon.PolygonCircleIntersection(center - origin, 4f) == PolygonShapeRelation.Intersecting)
                {
                    shape = new AOEShapeCustom(center, [new Square(CellCenter(CellIndex(origin)), 5f)], shapes2: [new Circle(origin, 4f)], operand: OperandType.Intersection);
                    Casters.Add(new(shape, center, default, act, actorID: caster.InstanceID, shapeDistance: shape.Distance(center, default)));
                    return;
                }
            }
            shape = Shape;
            Casters.Add(new(shape, spell.LocXZ, default, act, actorID: caster.InstanceID, shapeDistance: shape.Distance(origin, default)));
        }
    }

    public int CellIndex(WPos pos)
    {
        var off = pos - Arena.Center;
        return (CoordinateIndex(off.Z) << 2) | CoordinateIndex(off.X);
    }

    private int CoordinateIndex(float coord) => coord switch
    {
        < -10f => 0,
        < 0f => 1,
        < 10f => 2,
        _ => 3
    };

    public WPos CellCenter(int index)
    {
        var x = -15f + 10f * (index & 3);
        var z = -15f + 10f * (index >> 2);
        return Arena.Center + new WDir(x, z);
    }
}

sealed class RubyReflection : Components.GenericAOEs
{
    public RubyReflection(BossModule module) : base(module)
    {
        var center = Arena.Center;
        Reflection1Zero =
        [
            new(center, [new Square(new(223f, 337f), 5f), new Square(new(233f, 337f), 5f), new Square(new(233f, 347f), 5f), new Square(new(233f, 357f), 5f)]),
            new(center, [new Square(new(223f, 347f), 5f), new Square(new(223f, 357f), 5f), new Square(new(223f, 367f), 5f), new Square(new(233f, 367f), 5f)]),
            new(center, [new Square(new(243f, 337f), 5f), new Square(new(253f, 337f), 5f), new Square(new(253f, 347f), 5f), new Square(new(253f, 357f), 5f)]),
            new(center, [new Square(new(243f, 347f), 5f), new Square(new(243f, 357f), 5f), new Square(new(243f, 367f), 5f), new Square(new(253f, 367f), 5f)])
        ];
        Reflection2Zero =
        [
            new(center, [new Square(new(223f, 337f), 5f), new Square(new(223f, 347f), 5f), new Square(new(233f, 347f), 5f), new Square(new(243f, 347f), 5f)]),
            new(center, [new Square(new(233f, 337f), 5f), new Square(new(243f, 337f), 5f), new Square(new(253f, 337f), 5f), new Square(new(253f, 347f), 5f)]),
            new(center, [new Square(new(223f, 357f), 5f), new Square(new(223f, 367f), 5f), new Square(new(233f, 367f), 5f), new Square(new(243f, 367f), 5f)]),
            new(center, [new Square(new(233f, 357f), 5f), new Square(new(243f, 357f), 5f), new Square(new(253f, 357f), 5f), new Square(new(253f, 367f), 5f)]),
        ];
        TopazComponent = module.FindComponent<TopazRay>()!;
    }
    private readonly TopazRay TopazComponent;
    public readonly List<AOEInstance> AOEs = [];
    private readonly AOEShapeRect rect = new(10f, 10f, 10f);
    private readonly WPos[] Quadrants = [new(228f, 342f), new(248f, 342f), new(228f, 362f), new(248f, 362f)];
    private readonly AOEShapeCustom[] Reflection1Zero;
    private readonly AOEShapeCustom[] Reflection2Zero;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return CollectionsMarshal.AsSpan(AOEs);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.TopazOutline1)
        {
            if (state == 0x00010002u)
            {
                var act = WorldState.FutureTime(11.8d);
                var quadCount = Quadrants.Length;
                var topaz = CollectionsMarshal.AsSpan(TopazComponent.Actors);
                var topazCount = topaz.Length;
                for (var i = 0; i < topazCount; ++i)
                {
                    var t = topaz[i];
                    var pos = t.Position;
                    var rot = t.Rotation;
                    for (var j = 0; j < quadCount; ++j)
                    {
                        var quad = Quadrants[j];
                        var p = pos + (rot + 180f.Degrees()).ToDirection() * 3f;
                        if (quad.InSquare(pos, 10f) && !quad.InSquare(p, 10f))
                        {
                            AOEs.Add(new(rect, quad, activation: act, shapeDistance: rect.Distance(quad, default)));
                        }
                    }
                }
            }
        }
        else if (actor.OID == (uint)OID.TopazOutline2)
        {
            if (state is 0x00100020u or 0x01000200u)
            {
                var act = WorldState.FutureTime(14.8d);
                var shapes = state == 0x00100020u ? Reflection2Zero : Reflection1Zero;
                var rubyRot = actor.Rotation;
                var center = Arena.Center;
                var shapeCount = shapes.Length;
                var topaz = CollectionsMarshal.AsSpan(TopazComponent.Actors);
                var topazCount = topaz.Length;
                for (var i = 0; i < topazCount; ++i)
                {
                    var t = topaz[i];
                    var pos = t.Position;
                    var rot = t.Rotation;
                    for (var j = 0; j < shapeCount; ++j)
                    {
                        var shape = shapes[j];
                        var poly = shape.Polygon.Transform(default, rubyRot.ToDirection());
                        poly.InitPolygonIndex();
                        var p = pos + (rot + 180f.Degrees()).ToDirection() * 3f;
                        if (poly.Contains(pos - center) && !poly.Contains(p - center))
                        {
                            var aoe = new AOEShapeCustom(center, [], skipPolygonInit: true);
                            aoe.ReplacePolygon(poly, center);
                            AOEs.Add(new(aoe, center, activation: act, shapeDistance: aoe.Distance(center, default)));
                        }
                    }
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RubyReflection or (uint)AID.RubyReflection1 or (uint)AID.RubyReflection2)
        {
            AOEs.Clear();
        }
    }
}

sealed class StarvingDread(BossModule module) : Components.GenericKnockback(module)
{
    private Angle? _firstDirection;
    private DateTime _firstActivation;
    private Knockback? _second;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        Knockback? first = null;
        if (_firstDirection is { } direction)
        {
            var kind = direction.ToDirection().OrthoL().Dot(actor.Position - Arena.Center) >= 0f ? Kind.DirLeft : Kind.DirRight;
            first = new(Arena.Center, 15f, _firstActivation, direction: direction, kind: kind);
        }

        if (first != null && _second != null)
            return new Knockback[2] { first.Value, _second.Value };
        if (first != null)
            return new Knockback[1] { first.Value };
        return _second != null ? new Knockback[1] { _second.Value } : [];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // Snapshot each stage independently: immunity can expire between the two hits.
        var first = _firstDirection is { } direction && !IsImmune(slot, _firstActivation) ? (WDir?)direction.ToDirection().OrthoL() : null;
        var second = _second is { } knockback && !IsImmune(slot, knockback.Activation) ? (WPos?)knockback.Origin : null;
        if (first == null && second == null)
            return;

        // Before the first hit, prepare for BOTH landings. After it, keep the second
        // landing constraint active immediately so uptime cannot pull us out of safety.
        hints.AddForbiddenZone(new UnsafeKnockbackPositions(Arena.Center, Arena.Bounds.Radius - 1f, first, second,
            LandingDangers(slot, actor, _firstActivation), LandingDangers(slot, actor, _second?.Activation ?? default)),
            first != null ? _firstActivation : WorldState.CurrentTime);
        hints.GoalZonesEnabled = false;
    }

    private ShapeDistance[] LandingDangers(int slot, Actor actor, DateTime activation)
    {
        if (activation == default)
            return [];
        List<ShapeDistance> dangers = [];
        foreach (var component in Module.Components)
            if (component is Components.GenericAOEs aoes)
                foreach (var aoe in aoes.ActiveAOEs(slot, actor))
                    if (aoe.Activation >= activation.AddSeconds(-0.5d) && aoe.Activation <= activation.AddSeconds(1d))
                        dangers.Add(aoe.Shape.Distance(aoe.Origin, aoe.Rotation));
        return [.. dangers];
    }

    private sealed class UnsafeKnockbackPositions(WPos center, float halfWidth, WDir? firstNormal, WPos? secondOrigin,
        ShapeDistance[] firstDangers, ShapeDistance[] secondDangers) : ShapeDistance
    {
        public override float Distance(in WPos p) => Contains(p) ? 0f : 1f;

        public override bool Contains(in WPos p)
        {
            var landing = p;
            if (firstNormal is { } normal)
            {
                // Select the side from the candidate position, not the player's current side.
                landing += ((landing - center).Dot(normal) >= 0f ? normal : -normal) * 15f;
                if (!landing.InSquare(center, halfWidth))
                    return true;
            }
            if (firstNormal != null && InDanger(landing, firstDangers))
                return true;
            if (secondOrigin is { } origin)
            {
                var offset = landing - origin;
                if (offset.LengthSq() <= Epsilon)
                    return true; // the exact knockback origin has no reliable direction
                landing += offset.Normalized() * 30f;
                if (!landing.InSquare(center, halfWidth))
                    return true;
            }
            return secondOrigin != null && InDanger(landing, secondDangers);
        }

        private static bool InDanger(WPos position, ShapeDistance[] dangers)
        {
            foreach (var danger in dangers)
                if (danger.Distance(position) <= 0.5f)
                    return true;
            return false;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SpinebreakingStampedeCast)
        {
            var movement = spell.LocXZ - caster.Position;
            if (movement.LengthSq() > 1f)
            {
                _firstDirection = Angle.FromDirection(movement);
                _firstActivation = Module.CastFinishAt(spell, 0.75d);
            }
        }
        else if (spell.Action.ID == (uint)AID.SpinebreakingStampedeCircleVisual)
            _second = new(caster.Position, 30f, Module.CastFinishAt(spell, 6.1d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // The actual first hit follows the boss's cast end by about 0.65s.
        // Keep the preparation constraint until it resolves; each hit has duplicate helper events.
        if (spell.Action.ID == (uint)AID.SpinebreakingStampedeMiddle && _firstDirection != null)
        {
            _firstDirection = null;
            ++NumCasts;
        }
        else if (spell.Action.ID == (uint)AID.SpinebreakingStampedeCircle && _second != null)
        {
            _second = null;
            ++NumCasts;
        }
    }
}

sealed class ABeastUnleashedStates : StateMachineBuilder
{
    public ABeastUnleashedStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SonicHowl>()
            .ActivateOnEnter<RubyGlow>()
            .ActivateOnEnter<TopazRay>()
            .ActivateOnEnter<RubyReflection>()
            .ActivateOnEnter<TailToClaw>()
            .ActivateOnEnter<StarvingDread>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.AtlasCarbuncle, Contributors = "gynorhino", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1093u, NameID = 56u)]
public sealed class ABeastUnleashed(WorldState ws, Actor primary) : BossModule(ws, primary, new(238f, 352f), new ArenaBoundsSquare(20f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player() is Actor player && player.Position.InSquare(Arena.Center, 20f);
}
