namespace BossMod.Stormblood.Ultimate.UCOB;

abstract class TwisterBase(BossModule module, uint aid = (uint)AID.Twister, double spawnDelay = 0.3d, double predictBeforeSpawn = 0.65d) : Components.CastTwister(module, 1.25f, (uint)OID.VoidzoneTwister, aid, spawnDelay, predictBeforeSpawn) // TODO: verify radius
{
    private readonly bool _forceJump = Service.Config.Get<UCOBConfig>().TwisterForceJump;
    private const double PreSnapshotFreeze = 1.65d;
    public bool HasPredictedPositions => PredictedPositions.Count > 0;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var twister in ActiveTwisters)
        {
            hints.AddForbiddenZone(new SDCircle(twister.Position, 1.25f));
            hints.Voidzones.Add((twister.Position, 1.25f));
        }
        foreach (var position in PredictedPositions)
        {
            hints.AddForbiddenZone(new SDCircle(position, 1.25f));
            hints.Voidzones.Add((position, 1.25f));
        }

        // Stay still until the server snapshots positions; slidecasting before this can make the twister undodgeable.
        if ((PredictionAt - WorldState.CurrentTime).TotalSeconds is > 0d and < PreSnapshotFreeze && !HasPredictedPositions)
        {
            hints.MaxCastTime = 0f;
            hints.ForceCancelCast = true;
            hints.ForcedMovement = new(0f);
            hints.ForbidDashes = true;
        }
        if (PredictedPositions.Any(p => actor.Position.InCircle(p, 1.25f)))
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
