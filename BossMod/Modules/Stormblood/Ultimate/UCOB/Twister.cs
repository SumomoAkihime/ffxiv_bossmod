namespace BossMod.Stormblood.Ultimate.UCOB;

abstract class TwisterBase(BossModule module) : Components.CastTwister(module, 1.25f, (uint)OID.VoidzoneTwister, (uint)AID.Twister, 0.3d, 0.65d) // TODO: verify radius
{
    private readonly bool _forceJump = Service.Config.Get<UCOBConfig>().TwisterForceJump;
    private DateTime _spawnAt;
    public bool HasPredictedPositions => PredictedPositions.Count > 0;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == WatchedAction)
            _spawnAt = Module.CastFinishAt(spell, SpawnDelay);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var twister in ActiveTwisters)
            hints.AddForbiddenZone(new SDCircle(twister.Position, 1.25f));
        foreach (var position in PredictedPositions)
            hints.AddForbiddenZone(new SDCircle(position, 1.25f));

        // Stay still until the server snapshots positions; slidecasting before this can make the twister undodgeable.
        if (_spawnAt > WorldState.CurrentTime && !HasPredictedPositions)
        {
            hints.MaxCastTime = 0f;
            hints.ForceCancelCast = true;
            hints.ForcedMovement = new(0f);
        }
        if (PredictedPositions.Any(p => actor.Position.InCircle(p, 1.25f)))
            hints.GoalZonesEnabled = false;
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
