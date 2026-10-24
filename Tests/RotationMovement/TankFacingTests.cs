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

            player.PosRot = new(0, 0, 10, 0);
            boss.PosRot = default;
            boss.TargetID = player.InstanceID;
            hints.Clear();
            var first = new AIHints.Enemy(boss, 1, true) { CanMove = false };
            var secondActor = new Actor(0xffff2201, 0, 8, 0, "secondary", 0, ActorType.Enemy, Class.None, 100, new(10, 0, 0, 0)) { TargetID = player.InstanceID };
            var second = new AIHints.Enemy(secondActor, 1, true) { CanMove = false };
            hints.PotentialTargets.Add(first);
            hints.PotentialTargets.Add(second);
            hints.GoalZones.Add(AIHints.GoalSingleTarget(boss, 3));
            Actor? noPrimaryTarget = null;
            movement.Execute(strategy, ref noPrimaryTarget, 0, false);
            check(hints.GoalZones.Count == 3, "无主目标时仍处理所有以玩家为目标的敌人");
            var nearScore = hints.GoalZones.Sum(g => g(new(0, 1)));
            var farScore = hints.GoalZones.Sum(g => g(new(0, 10)));
            check(nearScore > farScore, "原地读条敌人的朝向奖励不压过近战接敌目标");
            check(hints.ForcedMovement == null, "多敌人目标评分仍遵守 None 移动策略");
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
