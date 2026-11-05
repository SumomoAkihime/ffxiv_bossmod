namespace BossMod.Stormblood.Ultimate.UCOB;

abstract class TwisterBase(BossModule module, uint aid = (uint)AID.Twister, double spawnDelay = 0.3d, double predictBeforeSpawn = 0.6d) : Components.CastTwister(module, 1.25f, (uint)OID.VoidzoneTwister, aid, spawnDelay, predictBeforeSpawn, predictRadius: 2.25f) // TODO: verify radius
{
    private readonly bool _forceJump = Service.Config.Get<UCOBConfig>().TwisterForceJump;
    private const double PreSnapshotFreeze = 1.5d;
    public bool HasPredictedPositions => PredictedPositions.Count > 0;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _spawnAt = Module.CastFinishAt(spell, SpawnDelay);
            _predictAt = _spawnAt.AddSeconds(-(PredictionTime + WorldState.Network.LastRTT));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var twister in ActiveTwisters)
        {
            hints.AddForbiddenZone(new SDCircle(twister.Position, 1.25f));
            hints.Voidzones.Add((twister.Position, 1.25f));
        }
        foreach (var position in PredictedPositions)
        {
            hints.AddForbiddenZone(new SDCircle(position, PredictionRadius));
            hints.Voidzones.Add((position, PredictionRadius));
        }

        // Stay still until the server snapshots positions; slidecasting before this can make the twister undodgeable.
        var predictionPeriod = (PredictionAt - WorldState.CurrentTime).TotalSeconds is > 0d and < PreSnapshotFreeze;
        if (predictionPeriod && !HasPredictedPositions)
        {
            hints.MaxCastTime = 0f;
            if (Service.Config.Get<BossModuleConfig>().AllowAutomaticActions)
                hints.ForceCancelCast = true;
            hints.ForcedMovement = new(0f);
            hints.ForbidDashes = true;
        }
        if (predictionPeriod || HasPredictedPositions)
        {
            hints.MaxCastTime = 0;
            if (Service.Config.Get<BossModuleConfig>().AllowAutomaticActions)
                hints.ForceCancelCast = true;
        }
        if (PredictedPositions.Any(p => actor.Position.InCircle(p, PredictionRadius)))
        {
            hints.GoalZonesEnabled = false;
            hints.ForbidDashes = true;
        }
        if (_forceJump && PredictedPositions.Count > 0 && PredictedActivation > WorldState.CurrentTime && PredictedActivation < WorldState.FutureTime(0.5d))
        {
            hints.WantJump = true;
        }
    }
}

sealed class Twister(BossModule module) : TwisterBase(module);

sealed class P1Twister(BossModule module) : TwisterBase(module)
{
    public override bool KeepOnPhaseChange => true;
}

class P1TwisterFireball(BossModule module) : Components.CastCounter(module, (uint)AID.Twister)
{
    bool Enabled = true;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        Enabled |= spell.Action.ID == WatchedAction;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Enabled)
            return;

        var dropPosition = Module.PrimaryActor.Position + Module.PrimaryActor.DirectionTo(Arena.Center) * (Module.PrimaryActor.HitboxRadius + 3);

        hints.AddForbiddenZone(new SDCircle(dropPosition, 2.5f), DateTime.MaxValue);
    }
}
