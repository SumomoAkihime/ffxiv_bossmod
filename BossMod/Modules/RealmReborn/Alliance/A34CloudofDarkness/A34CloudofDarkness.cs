namespace BossMod.RealmReborn.Alliance.A34CloudofDarkness;

class ZeroFormParticleBeam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ZeroFormParticleBeam, new AOEShapeRect(74, 12));

class FeintParticleBeam(BossModule module) : Components.StandardChasingAOEs(module, _chase, (uint)AID.FeintParticleBeam, (uint)AID.FeintParticleBeamChase, 3, 0.6f, 17)
{
    private static readonly AOEShapeCircle _first = new(8);
    private static readonly AOEShapeCircle _chase = new(3);

    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != ActionFirst)
            return;
        var pos = spell.TargetID == caster.InstanceID ? caster.Position : WorldState.Actors.Find(spell.TargetID)?.Position ?? spell.LocXZ;
        var target = Raid.WithoutSlot().Where(p => !Chasers.Any(c => c.Target == p)).MinBy(p => (p.Position - pos).LengthSq());
        if (target != null)
            Chasers.Add(new(Shape, target, pos, 0, MaxCasts, Module.CastFinishAt(spell), SecondsBetweenActivations));
        Update();
    }

    public override void Update()
    {
        base.Update();
        for (var i = Chasers.Count - 1; i >= 0; --i)
        {
            var c = Chasers[i];
            if (c.Target.IsDeadOrDestroyed || c.NumRemaining <= 0)
            {
                TargetsMask.Clear(Raid.FindSlot(c.Target.InstanceID));
                Chasers.RemoveAt(i);
            }
        }
        _aoes.Clear();
        foreach (var c in Chasers)
        {
            var pos = c.PredictedPosition();
            var off = pos - c.PrevPos;
            _aoes.Add(new(c.NumRemaining >= MaxCasts ? _first : c.Shape, pos, off.LengthSq() > 0 ? Angle.FromDirection(off) : default, c.NextActivation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        Update();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var c in Chasers)
            if (c.NumRemaining >= MaxCasts)
                hints.AddForbiddenZone(_first, c.PredictedPosition(), default, c.NextActivation);

        if (Chasers.All(c => c.Target != actor))
            return;

        // drag chase away from the raid
        var awayFromBoss = (Arena.Center - Module.PrimaryActor.Position).Normalized();
        if (awayFromBoss.LengthSq() > 0.01f)
            hints.AddForbiddenZone(new SDHalfPlane(Arena.Center, awayFromBoss));

        // and don't run the puddles through other players
        foreach (var p in Raid.WithoutSlot().Exclude(actor))
            hints.AddForbiddenZone(new SDCircle(p.Position, 4));
    }
}

class ParticleBeamTowers(BossModule module) : Components.GenericTowers(module, (uint)AID.ParticleBeam)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ParticleBeamTower)
            Towers.Add(new(actor.Position, 5, minSoakers: 1, maxSoakers: int.MaxValue, activation: WorldState.FutureTime(10)));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.ParticleBeamTower)
            Towers.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ParticleBeam or (uint)AID.ParticleBeamFail)
            Towers.RemoveAll(t => t.Position.AlmostEqual(spell.TargetXZ, 1));
    }
}

class HyperchargedClouds(BossModule module) : BossComponent(module)
{
    public const float HexRadius = 8.5f;
    private static readonly uint[] _hexOIDs = [(uint)OID.HyperchargedHexA, (uint)OID.HyperchargedHexB, (uint)OID.HyperchargedHexC];
    private readonly HashSet<ulong> _active = [];

    private static AllianceLetter HexAlliance(uint oid) => oid switch
    {
        (uint)OID.HyperchargedHexA => AllianceLetter.A,
        (uint)OID.HyperchargedHexB => AllianceLetter.B,
        (uint)OID.HyperchargedHexC => AllianceLetter.C,
        _ => AllianceLetter.None
    };

    private IEnumerable<Actor> ActiveHexes => WorldState.Actors.Where(a => _active.Contains(a.InstanceID));

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (!_hexOIDs.Contains(actor.OID))
            return;
        if (state == 0x00040008)
            _active.Add(actor.InstanceID);
        else if (state == 0x00010040)
            _active.Remove(actor.InstanceID);
    }

    public override void OnActorDestroyed(Actor actor) => _active.Remove(actor.InstanceID);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var mine = ActiveHexes.FirstOrDefault(h => HexAlliance(h.OID) == Raid.Alliance);
        if (mine != null)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(mine.Position, HexRadius, 5));

        foreach (var c in Module.Enemies((uint)OID.HyperchargedCloud).Where(c => c.IsTargetable && !c.IsDead))
            hints.SetPriority(c, 3);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var myAlliance = Raid.Alliance;
        foreach (var hex in ActiveHexes)
            DrawHex(hex.Position, HexAlliance(hex.OID) == myAlliance ? Colors.Safe : Colors.Danger);
    }

    private void DrawHex(WPos center, uint color)
    {
        Span<WPos> pts = stackalloc WPos[6];
        for (var i = 0; i < 6; ++i)
            pts[i] = center + HexRadius * (i * 60).Degrees().ToDirection();
        Arena.AddPolygon(pts, color, 2);
    }
}

class ParticleBeamEnrage(BossModule module) : Components.RaidwideCast(module, (uint)AID.ParticleBeamEnrage, "Kill Hypercharged Clouds!");
class Shadowlurkers(BossModule module) : Components.Adds(module, (uint)OID.Shadowlurker, 2);
class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.DarkCloud, (uint)OID.DarkStorm, (uint)OID.HyperchargedCloud], 1);

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3240)]
public class A34CloudofDarkness(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -400), new ArenaBoundsCircle(30));
