using static BossMod.PartyRolesConfig;

namespace BossMod.Stormblood.Ultimate.UCOB;

class Heavensfall(BossModule module) : Components.GenericKnockback(module, (uint)AID.Heavensfall)
{
    public DateTime Activation;

    private readonly List<Knockback> _portedKnockbackView = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        _portedKnockbackView.Clear();
        _portedKnockbackView.AddRange([new Knockback(Module.Center, 11, Activation, ignoreImmunes: true)]);
        return CollectionsMarshal.AsSpan(_portedKnockbackView);
    }
}

class P2Heavensfall(BossModule module) : Heavensfall(module)
{
    public override void AddAIHints(int slot, Actor actor, Assignment assignment, AIHints hints)
    {
        hints.AddForbiddenZone(new SDPrecisePosition(new WPos(0, 9), new(0, 1), 0.5f, actor.Position, 0.1f), Activation);
    }
}

class P2HeavensfallPillar(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeRect _shape = new(5, 5, 5);

    private readonly List<AOEInstance> _portedAOEView = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _portedAOEView.Clear();
        if (_aoe is { } aoe)
            _portedAOEView.Add(aoe);
        return CollectionsMarshal.AsSpan(_portedAOEView);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID != OID.EventHelper)
            return;
        switch (state)
        {
            case 0x00040008: // appear
                _aoe = new(_shape, actor.Position, actor.Rotation);
                break;
            // 0x00100020: ? 0.5s after appear
            // 0x00400080: ? 4.0s after appear
            // 0x01000200: ? 5.8s after appear
            // 0x04000800: ? 7.5s after appear
            // 0x10002000: ? 9.4s after appear
            case 0x40008000: // disappear (11.1s after appear)
                _aoe = null;
                break;
        }
    }
}

class P2ThermionicBurst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThermionicBurst, new AOEShapeCone(24.5f, 11.25f.Degrees()));

abstract class MeteorStream(BossModule module) : Components.StrictUniformStackSpread(module, 0, 4, alwaysShowSpreads: true)
{
    public int NumCasts;
}

class P2MeteorStream : MeteorStream
{
    public P2MeteorStream(BossModule module) : base(module)
    {
        AddSpreads(Raid.WithoutSlot(true), WorldState.FutureTime(5.6f));
    }

    public override (WPos, float)? GetSpreadDestination(int order, Actor target)
    {
        var assignment = AssignmentsPerSlot.BoundSafeAt(Raid.FindSlot(target.InstanceID), Assignment.Unassigned);
        if (assignment == Assignment.Unassigned)
            return null;
        var (dist, angle) = assignment switch
        {
            Assignment.MT => (9, -11.25f.Degrees()),
            Assignment.OT => (9, 11.25f.Degrees()),
            Assignment.H1 => (18, -11.25f.Degrees()),
            Assignment.H2 => (18, 11.25f.Degrees()),
            Assignment.M1 => (9, -56.25f.Degrees()),
            Assignment.M2 => (9, 56.25f.Degrees()),
            Assignment.R1 => (18, -56.25f.Degrees()),
            Assignment.R2 => (18, 56.25f.Degrees()),
            _ => default
        };
        return (Arena.Center + angle.ToDirection() * dist, 2);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MeteorStream)
        {
            ++NumCasts;
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);

            // update activation time for second set
            if (NumCasts == 4)
                for (var i = 0; i < Spreads.Count; i++)
                    Spreads.Ref(i).Activation = WorldState.FutureTime(3.1f);
        }
    }
}

class P2HeavensfallDalamudDive(BossModule module) : Components.GenericBaitAway(module, (uint)AID.DalamudDive, true, true)
{
    private readonly Actor? _target = module.WorldState.Actors.Find(module.PrimaryActor.TargetID);

    private static readonly AOEShapeCircle _shape = new(5);

    public void Show()
    {
        if (_target != null)
            CurrentBaits.Add(new(_target, _target, _shape));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action.ID == WatchedAction)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, Assignment assignment, AIHints hints)
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var len = baits.Length;
        var isTarget = false;
        for (var i = 0; i < len; ++i)
        {
            if (baits[i].Target == actor)
            {
                isTarget = true;
                break;
            }
        }
        if (!isTarget)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }

        // preposition close to nael
        if (actor.Role is Role.Melee or Role.Tank)
        {
            for (var i = 0; i < len; ++i)
            {
                var t = baits[i].Target;
                if (t != actor)
                {
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(t.Position, 6f, 1f));
                }
            }
        }

        if (NumCasts > 0 && Module.Enemies((uint)OID.NaelDeusDarnus).FirstOrDefault() is { IsTargetable: false } nael)
        {
            hints.GoalZones.Add(AIHints.GoalSingleTarget(nael.Position, nael.HitboxRadius));
        }
    }
}
