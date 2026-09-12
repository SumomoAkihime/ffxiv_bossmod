using BossMod;
using BossMod.Autorotation;
using BossMod.Autorotation.MiscAI;

static class TankFacingTests
{
    public static void Run(RotationModuleManager manager, Actor player, Actor boss, Action<bool, string> check)
    {
        var hints = manager.Hints;
        var savedPosition = player.PosRot;
        var savedBossPosition = boss.PosRot;
        var savedTarget = boss.TargetID;
        var savedModule = manager.Bossmods.ActiveModule;
        try
        {
            manager.Bossmods.ActiveModule = null;
            player.PosRot = new(5, 0, 0, 0);
            boss.PosRot = default;
            var movement = new NormalMovement(manager, player);
            var strategy = new StrategyValues(NormalMovement.Definition().Configs);
            ((StrategyValueTrack)strategy.Values[(int)NormalMovement.Track.Destination]).Option = (int)NormalMovement.DestinationStrategy.None;

            Func<WPos, float>[] Goals(bool tanking, bool movable)
            {
                hints.Clear();
                boss.TargetID = tanking ? player.InstanceID : 0;
                var enemy = new AIHints.Enemy(boss, 1, true) { CanMove = movable, DesiredRotation = default, DesiredPosition = new(0, 10) };
                hints.Enemies[boss.CharacterSpawnIndex] = enemy;
                hints.PotentialTargets.Add(enemy);
                Actor? target = boss;
                movement.Execute(strategy, ref target, 0, false);
                return hints.GoalZones.ToArray();
            }

            var baseline = Goals(false, false).Length;
            var facing = Goals(true, false);
            check(facing.Length == baseline + 1, "只有当前承伤者收到首领朝向目标");
            check(facing[^1](new(0, 5)) == 0.5f && facing[^1](new(0, 15)) == 0,
                "朝向引导保持当前径向距离，不再奖励整条向外延伸的直线");
            check(facing[^1](new(5, 0)) == 0, "站在首领侧面时仍需调整朝向");
            var movable = Goals(true, true);
            check(movable.Length == baseline + 2 && movable[^1](new(0, 5)) == 0.5f,
                "可移动首领同时保留拉怪和朝向目标");
            check(hints.ForcedMovement == null, "关闭移动时位置偏好不产生强制移动");
        }
        finally
        {
            player.PosRot = savedPosition;
            boss.PosRot = savedBossPosition;
            boss.TargetID = savedTarget;
            manager.Bossmods.ActiveModule = savedModule;
            hints.Clear();
        }
    }
}
