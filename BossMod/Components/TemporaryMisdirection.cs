namespace BossMod.Components;

// generic temporary misdirection component
[SkipLocalsInit]
public abstract class TemporaryMisdirection(BossModule module, uint aid, string hint = "Applies temporary misdirection") : CastHint(module, aid, hint)
{
    private BitMask mask;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is 1422u or 2936u or 3694u or 3909u)
        {
            mask.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is 1422u or 2936u or 3694u or 3909u)
        {
            mask.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (mask[slot])
        {
            hints.AddSpecialMode(AIHints.SpecialMode.Misdirection, default);
        }
    }
}

[SkipLocalsInit]
public abstract class Spinning(BossModule module, uint aid, bool createForbiddenZones = true, uint statusID = 2973u, string hint = "Applies spinning") : CastHint(module, aid, hint)
{
    internal BitMask mask;
    private readonly uint _statusID = statusID;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == _statusID)
            mask.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == _statusID)
            mask.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (createForbiddenZones && mask[slot])
        {
            hints.AddForbiddenZone(new SDRect(actor.Position, actor.Rotation, 5.5f, 7.5f, 7.5f), WorldState.FutureTime(2d));
            hints.AddForbiddenZone(new SDCone(actor.Position, 100f, actor.Rotation + 180f.Degrees(), 45f.Degrees()), DateTime.MaxValue);
        }
    }
}
