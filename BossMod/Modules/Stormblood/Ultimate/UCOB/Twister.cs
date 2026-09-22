namespace BossMod.Stormblood.Ultimate.UCOB;

abstract class TwisterBase(BossModule module) : Components.CastTwister(module, 1.25f, (uint)OID.VoidzoneTwister, (uint)AID.Twister, 0.3d, 0.8d) // TODO: verify radius
{
    private readonly bool _forceJump = Service.Config.Get<UCOBConfig>().TwisterForceJump;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (PredictionAt < WorldState.FutureTime(0.5d) || PredictedPositions.Count > 0 && ActiveTwisters.Length == 0)
        {
            hints.MaxCastTime = 0f;
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
