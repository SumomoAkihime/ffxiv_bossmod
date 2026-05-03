namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

class DivideAndConquerBait(BossModule module) : Components.GenericBaitAway(module, AID.DivideAndConquerBait)
{
    private static readonly AOEShapeRect Shape = new(60, 2.5f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DivideAndConquer && WorldState.Actors.Find(targetID) is { } target)
            CurrentBaits.Add(new(actor, target, Shape, WorldState.FutureTime(3.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            if (CurrentBaits.Count > 0)
                CurrentBaits.RemoveAt(0);
        }
    }
}

class DivideAndConquerAOE(BossModule module) : Components.StandardAOEs(module, AID.DivideAndConquerAOE, new AOEShapeRect(60, 2.5f));
