using Dalamud.Game.ClientState.Conditions;

namespace BossMod.AI;

// utility for simulating user actions based on AI decisions:
// - navigation
// - using actions safely (without spamming, not in cutscenes, etc)
sealed class AIController(WorldState ws, ActionManagerEx amex, MovementOverride movement)
{
    public WPos? NaviTargetPos;
    public float? NaviTargetVertical;
    public bool AllowInterruptingCastByMovement;
    public float NaviLeewaySeconds = float.MaxValue;
    public bool ForceCancelCast;

    private readonly CastSettleHold _castHold = new();
    private readonly ActionManagerEx _amex = amex;
    private readonly MovementOverride _movement = movement;

    public bool IsVerticalAllowed => Service.Condition[ConditionFlag.InFlight];
    public Angle CameraFacing => (Camera.Instance?.CameraAzimuth ?? 0f).Radians() + 180f.Degrees();
    public Angle CameraAltitude => (Camera.Instance?.CameraAltitude ?? 0f).Radians();

    public void Clear()
    {
        NaviTargetPos = null;
        NaviTargetVertical = null;
        AllowInterruptingCastByMovement = false;
        NaviLeewaySeconds = float.MaxValue;
        ForceCancelCast = false;
        _castHold.Reset();
    }

    public void SetFocusTarget(Actor? actor)
    {
        if (Service.TargetManager.FocusTarget?.EntityId != actor?.InstanceID)
        {
            Service.TargetManager.FocusTarget = actor != null ? Service.ObjectTable.SearchById((uint)actor.InstanceID) : null;
        }
    }

    public void Update(Actor? player, AIHints hints, DateTime now)
    {
        if (player == null || player.IsDead || ws.Party.Members[PartyState.PlayerSlot].InCutscene)
        {
            return;
        }

        Vector3? desiredPosition = null;

        // TODO this checks whether movement keys are pressed, we need a better solution
        var moveRequested = _movement.IsMoveRequested();
        var castInProgress = player.CastInfo != null && !player.CastInfo.EventHappened;
        var castRemaining = _amex.CastTimeRemaining;
        var mustMoveNow = AllowInterruptingCastByMovement || (castRemaining > 0f ? NaviLeewaySeconds <= castRemaining - 0.5f : NaviLeewaySeconds <= 0f);
        var forbidMovement = moveRequested || _amex.MoveMightInterruptCast && !mustMoveNow;
        if (NaviTargetPos != null && !forbidMovement && (NaviTargetPos.Value - player.Position).LengthSq() > 0.001f)
        {
            desiredPosition = NaviTargetPos.Value.ToVec3(NaviTargetVertical != null && IsVerticalAllowed ? NaviTargetVertical.Value : player.PosRot.Y);
        }
        else
        {
            hints.ForceCancelCast |= ForceCancelCast && castInProgress;
        }

        var moving = hints.ForcedMovement == null && desiredPosition != null;
        if (_castHold.Update(now, moving, moving && _amex.MoveMightInterruptCast, out _))
        {
            hints.HoldCasts = true;
            hints.MaxCastTime = 0f;
        }

        if (hints.ForcedMovement == null && desiredPosition != null)
        {
            hints.ForcedMovement = desiredPosition.Value - player.PosRot.XYZ();
        }
    }
}
