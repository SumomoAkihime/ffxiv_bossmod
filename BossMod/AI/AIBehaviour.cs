using BossMod.Autorotation;
using BossMod.Pathfinding;
using System.Threading;

namespace BossMod.AI;

public struct Targeting(AIHints.Enemy target, float preferredRange = 2.6f, Positional preferredPosition = Positional.Any, bool preferTanking = false)
{
    public AIHints.Enemy Target = target;
    public readonly float PreferredRange = preferredRange;
    public Positional PreferredPosition = preferredPosition;
    public readonly bool PreferTanking = preferTanking;
}

// constantly follow master
sealed class AIBehaviour(AIController ctrl, RotationModuleManager autorot) : IDisposable
{
    public WorldState WorldState => autorot.Bossmods.WorldState;
    public float ForceMovementIn = float.MaxValue; // TODO: reconsider
    private static readonly AIConfig _config = Service.Config.Get<AIConfig>();
    private readonly NavigationDecision.Context _naviCtx = new();
    private NavigationDecision _naviDecision;
    private bool _afkMode;
    private bool _followMaster; // if true, our navigation target is master rather than primary target - this happens e.g. in outdoor or in dungeons during gathering trash
    private WPos _masterPrevPos;
    private WPos _masterMovementStart;
    private DateTime _masterLastMoved;
    private DateTime _navStartTime; // if current time is < this, navigation won't start
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static readonly Random random = new();

    public void Dispose() { }

    public async Task Execute(Actor player, Actor master)
    {
        if (await _semaphore.WaitAsync(0).ConfigureAwait(false))
        {
            try
            {
                ForceMovementIn = float.MaxValue;
                if (player.IsDead)
                {
                    return;
                }

                // keep master in focus
                if (_config.FocusTargetMaster)
                {
                    FocusMaster(master);
                }

                _afkMode = _config.AutoAFK && !master.InCombat && (WorldState.CurrentTime - _masterLastMoved).TotalSeconds > _config.AFKModeTimer;
                var gazeImminent = autorot.Hints.ForbiddenDirections.Count != 0 && autorot.Hints.ForbiddenDirections.Ref(0).activation <= WorldState.FutureTime(0.5d);
                var pyreticImminent = autorot.Hints.ImminentSpecialMode.mode == AIHints.SpecialMode.Pyretic && autorot.Hints.ImminentSpecialMode.activation <= WorldState.FutureTime(1d);
                var misdirectionMode = autorot.Hints.ImminentSpecialMode.mode == AIHints.SpecialMode.Misdirection && autorot.Hints.ImminentSpecialMode.activation <= WorldState.CurrentTime;
                var hadNavi = _naviDecision.Destination != null;
                Targeting target = default;

                _followMaster = master != player;

                // note: if there are pending knockbacks, don't update navigation decision to avoid fucking up positioning
                if (player.PendingKnockbacks.Count == 0)
                {
                    _naviDecision = await BuildNavigationDecision(player, master, target).ConfigureAwait(false);

                    // there is a difference between having a small positive leeway and having a negative one for pathfinding, prefer to keep positive
                    _naviDecision.LeewaySeconds = Math.Max(0, _naviDecision.LeewaySeconds - 0.1f);
                }

                var masterIsMoving = TrackMasterMovement(master);
                var moveWithMaster = masterIsMoving && _followMaster;
                ForceMovementIn = moveWithMaster || gazeImminent || pyreticImminent ? default : _naviDecision.LeewaySeconds;

                if (_config.MoveDelay != 0d && !hadNavi && _naviDecision.Destination != null)
                {
                    _navStartTime = WorldState.FutureTime(_config.MoveDelay);
                }

                UpdateMovement(player, master, gazeImminent || pyreticImminent, misdirectionMode ? autorot.Hints.MisdirectionThreshold : default, null);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    private async Task<NavigationDecision> BuildNavigationDecision(Actor player, Actor master, Targeting targeting)
    {
        if (_config.ForbidMovement || _config.ForbidAIMovementMounted && player.MountId != default
            || autorot.Hints.ImminentSpecialMode.mode is AIHints.SpecialMode.NoMovement or AIHints.SpecialMode.Pyretic && autorot.Hints.ImminentSpecialMode.activation <= WorldState.FutureTime(1d))
        {
            return new() { LeewaySeconds = float.MaxValue };
        }

        if (autorot.Hints.ImminentSpecialMode.mode == AIHints.SpecialMode.Freezing && autorot.Hints.ImminentSpecialMode.activation <= WorldState.FutureTime(2.1d))
        {
            var randomO1 = random.NextSingle() * 2f - 1f;
            var randomO2 = random.NextSingle() * 2f - 1f;
            var pos = player.Position;
            autorot.Hints.ForcedMovement = new WPos(pos.X * randomO1, pos.Z * randomO2).ToVec3();
            return new() { LeewaySeconds = float.MaxValue };
        }

        Actor? forceDestination = null;
        var interactTarget = autorot.Hints.InteractWithTarget;
        if (interactTarget != null)
        {
            forceDestination = interactTarget;
        }
        else if (_followMaster)
        {
            forceDestination = master;
        }

        _followMaster = interactTarget == null && (_config.FollowDuringCombat || !master.InCombat || (_masterPrevPos - _masterMovementStart).LengthSq() > 100f) && (_config.FollowDuringActiveBossModule || autorot.Bossmods.ActiveModule?.StateMachine.ActiveState == null) && (_config.FollowOutOfCombat || master.InCombat);
        if (forceDestination != null && forceDestination != master && autorot.Hints.PathfindMapBounds.Contains(forceDestination.Position - autorot.Hints.PathfindMapCenter))
        {
            autorot.Hints.GoalZones.Add(AIHints.GoalProximity(forceDestination, 3.5f, 100f));
        }
        if (_followMaster)
        {
            if (master != player)
                autorot.Hints.GoalZones.Add(AIHints.GoalSingleTarget(master, Positional.Any, _config.MaxDistanceToSlot));
            return await Task.Run(() => NavigationDecision.Build(_naviCtx, WorldState.CurrentTime, autorot.Hints, player, autorot.Bossmods.WorldState.Client.MoveSpeed, forbiddenZoneCushion: _config.PreferredDistance)).ConfigureAwait(false);
        }

        // TODO: remove this once all rotation modules are fixed
        if (autorot.Hints.GoalZones.Count == 0 && targeting.Target != null)
        {
            autorot.Hints.GoalZones.Add(AIHints.GoalSingleTarget(targeting.Target.Actor, targeting.PreferredPosition, targeting.PreferredRange));
        }

        return await Task.Run(() => NavigationDecision.Build(_naviCtx, WorldState.CurrentTime, autorot.Hints, player, autorot.Bossmods.WorldState.Client.MoveSpeed, _config.PreferredDistance)).ConfigureAwait(false);
    }

    private void FocusMaster(Actor master)
    {
        var masterChanged = Service.TargetManager.FocusTarget?.EntityId != master.InstanceID;
        if (masterChanged)
        {
            ctrl.SetFocusTarget(master);
            _masterPrevPos = _masterMovementStart = master.Position;
            _masterLastMoved = WorldState.CurrentTime.AddSeconds(-1d);
        }
    }

    private bool TrackMasterMovement(Actor master)
    {
        // keep track of master movement
        // idea is that if master is moving forward (e.g. running in outdoor or pulling trashpacks in dungeon), we want to closely follow and not stop to cast
        var masterIsMoving = true;
        if (master.Position != _masterPrevPos)
        {
            _masterLastMoved = WorldState.CurrentTime;
            _masterPrevPos = master.Position;
        }
        else if ((WorldState.CurrentTime - _masterLastMoved).TotalSeconds > 0.5d)
        {
            // master has stopped, consider previous movement finished
            _masterMovementStart = _masterPrevPos;
            masterIsMoving = false;
        }
        // else: don't consider master to have stopped moving unless he's standing still for some small time

        return masterIsMoving;
    }

    private void UpdateMovement(Actor player, Actor master, bool gazeOrPyreticImminent, Angle misdirectionAngle, ActionQueue? queueForSprint)
    {
        if (gazeOrPyreticImminent)
        {
            // gaze or pyretic imminent, drop any movement - we should have moved to safe zone already...
            ctrl.NaviTargetPos = null;
            ctrl.NaviTargetVertical = null;
            ctrl.ForceCancelCast = true;
        }
        else if (misdirectionAngle != default && _naviDecision.Destination is WPos destination)
        {
            ctrl.AllowInterruptingCastByMovement = true;
            var dir = destination - player.Position;
            var distSq = dir.LengthSq();
            var threshold = 45f.Degrees();
            var forceddir = WorldState.Client.ForcedMovementDirection;
            var allowMovement = forceddir.AlmostEqual(Angle.FromDirection(dir), threshold.Rad);
            if (allowMovement)
            {
                allowMovement = CalculateUnobstructedPathLength(forceddir) >= Math.Min(3f, distSq);
            }

            ctrl.NaviTargetPos = allowMovement && distSq >= 0.01f ? destination : null;

            float CalculateUnobstructedPathLength(Angle dir)
            {
                var start = _naviCtx.Map.WorldToGrid(player.Position);
                var startx = start.x;
                var starty = start.y;
                if (!_naviCtx.Map.InBounds(startx, starty))
                {
                    return 0f;
                }

                var end = _naviCtx.Map.WorldToGrid(player.Position + 100f * dir.ToDirection());
                var startG = _naviCtx.Map.PixelMaxG[_naviCtx.Map.GridToIndex(startx, starty)];
                var pixels = _naviCtx.Map.EnumeratePixelsInLine(startx, starty, end.x, end.y);
                var len = pixels.Length;
                for (var i = 0; i < len; ++i)
                {
                    var p = pixels[i];
                    var px = p.x;
                    var py = p.y;
                    if (!_naviCtx.Map.InBounds(px, py) || _naviCtx.Map.PixelMaxG[_naviCtx.Map.GridToIndex(px, py)] < startG)
                    {
                        var dest = _naviCtx.Map.GridToWorld(px, py, 0.5f, 0.5f);
                        return (dest - player.Position).LengthSq();
                    }
                }
                return float.MaxValue;
            }

            // debug
            //void drawLine(WPos from, WPos to, uint color) => Camera.Instance!.DrawWorldLine(new(from.X, player.PosRot.Y, from.Z), new(to.X, player.PosRot.Y, to.Z), color);
            //var toDest = _naviDecision.Destination.Value - player.Position;
            //drawLine(player.Position, _naviDecision.Destination.Value, Colors.Safe);
            //drawLine(_naviDecision.Destination.Value, _naviDecision.Destination.Value + toDest.Normalized().OrthoL(), Colors.Safe);
            //drawLine(player.Position, ctrl.NaviTargetPos.Value, Colors.Danger);
        }
        else
        {
            var toDest = _naviDecision.Destination != null ? _naviDecision.Destination.Value - player.Position : default;
            var distSq = toDest.LengthSq();
            ctrl.NaviTargetPos = WorldState.CurrentTime >= _navStartTime ? _naviDecision.Destination : null;
            ctrl.NaviTargetVertical = master != player ? master.PosRot.Y : null;
            ctrl.AllowInterruptingCastByMovement = player.CastInfo != null && _naviDecision.LeewaySeconds <= player.CastInfo.RemainingTime - 0.5d;
            ctrl.ForceCancelCast = false;

            //var cameraFacing = _ctrl.CameraFacing;
            //var dot = cameraFacing.Dot(_ctrl.TargetRot.Value);
            //if (dot < -0.707107f)
            //    _ctrl.TargetRot = -_ctrl.TargetRot.Value;
            //else if (dot < 0.707107f)
            //    _ctrl.TargetRot = cameraFacing.OrthoL().Dot(_ctrl.TargetRot.Value) > 0 ? _ctrl.TargetRot.Value.OrthoR() : _ctrl.TargetRot.Value.OrthoL();

            // sprint, if not in combat and far enough away from destination
            if (player.InCombat ? _naviDecision.LeewaySeconds <= 0f && distSq > 25f : player != master && distSq > 400f)
            {
                queueForSprint?.Push(ActionDefinitions.IDSprint, player, ActionQueue.Priority.Minimal + 100f);
            }
        }
    }
}
