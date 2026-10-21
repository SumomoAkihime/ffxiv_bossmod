namespace BossMod.Stormblood.Ultimate.UWU;

class P2Nails(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _nails = [];

    public Angle RelativeNorth { get; private set; }
    public bool Active => _nails.Any(n => n.IsTargetable);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_nails);
        foreach (var nail in _nails.Where(n => !n.IsDead))
            Arena.AddCircleUnfilled(nail.Position, 1, Colors.Object);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID != OID.InfernalNail)
            return;

        _nails.Add(actor);
        if (_nails.Count != 4)
            return;

        WDir offset = default;
        var count = 0;
        foreach (var nail in _nails)
        {
            if (!_nails.InRadiusExcluding(nail, 9).Any())
                continue;
            offset += nail.Position - Arena.Center;
            ++count;
        }

        if (count == 0)
            return;

        RelativeNorth = Angle.FromDirection(offset);
        if (Module.FindComponent<BossPositioning>() is { } positioning)
        {
            positioning.DesiredPosition = Arena.Center + RelativeNorth.ToDirection() * 4;
            positioning.DesiredRotation = RelativeNorth;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var nail in _nails)
        {
            if (hints.FindEnemy(nail) is not { } enemy)
                continue;

            if (nail.FindStatus(1544u, DateTime.MaxValue)?.Extra >= 2)
                enemy.Priority = 1;
            else if (nail.PendingHPRatio < 0.2f)
                enemy.Priority = AIHints.Enemy.PriorityForbidden;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.InfernalNail)
            _nails.Remove(actor);
    }
}
