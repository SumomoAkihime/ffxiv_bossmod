namespace BossMod.Stormblood.Ultimate.UWU;

// TODO: kill priorities
class P2Nails(BossModule module) : BossComponent(module)
{
    readonly List<Actor> _nails = [];

    public Angle RelativeNorth { get; private set; }

    public bool Active => _nails.Any(n => n.IsTargetable);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_nails, Colors.Enemy);
        foreach (var n in _nails.Where(n => !n.IsDead))
            Arena.AddCircleUnfilled(n.Position, n.HitboxRadius, Colors.Object);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.InfernalNail)
        {
            _nails.Add(actor);

            if (_nails.Count == 4)
            {
                var pos = Module.FindComponent<BossPositioning>();
                var nearby = _nails.Where(n => _nails.InRadiusExcluding(n, 9).Any()).ToList();
                if (pos == null || nearby.Count == 0)
                    return;
                RelativeNorth = Angle.FromDirection(new WPos(nearby.Average(n => n.Position.X), nearby.Average(n => n.Position.Z)) - Arena.Center);
                pos.DesiredPosition = Arena.Center + RelativeNorth.ToDirection() * 4;
                pos.DesiredRotation = RelativeNorth;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var n in _nails)
        {
            if (hints.FindEnemy(n) is { } e)
            {
                e.ForbidDOTs = true;
                e.Priority = n.PendingHPRatio > 0.5f || n.FindStatus(1544)?.Extra >= 2 ? 1 : AIHints.Enemy.PriorityForbidden;
            }
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.InfernalNail)
            _nails.Remove(actor);
    }
}
