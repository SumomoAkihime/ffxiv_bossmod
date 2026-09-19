using BossMod;
using BossMod.Components;
using System.Reflection;

static class CoreSeptemberTests
{
    public static void Run(Action<bool, string> check)
    {
        VerifyBoards(check);
        VerifyBoardAI(check);
        VerifyDashGate(check);
        VerifyTwister(check);
        VerifyViewer(check);
    }

    private static void VerifyBoards(Action<bool, string> check)
    {
        (uint oid, string component, uint aid, WPos center)[] boards =
        [
            (0x4CE6, "ManaManifestation", 49233, new(120, -420)),
            (0x4CE2, "SwingRound", 49228, new(120, -420)),
            (0x4CF6, "GroundingJoltSmall", 49266, new(120, 0)),
            (0x4CDB, "HeatLightning", 49186, new(120, -420)),
            (0x4D03, "GiganticRageCircle", 49359, new(120, -420)),
            (0x4CDD, "VoidThunderIIICross", 49207, new(120, 0)),
            (0x4CFE, "BanishCircle", 49337, new(120, 0))
        ];
        foreach (var (oid, name, aid, center) in boards)
        {
            var world = NewWorld();
            var boss = Create(world, 1, oid, center);
            using var module = BossModuleRegistry.CreateModuleForActor(world, boss, BossModuleInfo.Maturity.Contributed)!;
            check(module != null && module.Info?.GroupID == 1092, $"新棋盘 {oid:X} 默认成熟度注册成功");
            if (module == null)
                continue;
            module.StateMachine.Start(world.CurrentTime);
            check(module.Arena.Center == center && module.StateMachine.ActiveState != null, $"新棋盘 {oid:X} 场心与状态机正确");
            var aoe = (GenericAOEs)module.Components.Single(c => c.GetType().Name == name);
            var helper = Create(world, 2, 0x233C, center);
            var cast = new ActorCastInfo { Action = new(ActionType.Spell, aid), Location = new(center.X, 0, center.Z), TotalTime = 4.7f };
            helper.CastInfo = cast;
            aoe.OnCastStarted(helper, cast);
            aoe.Update();
            var shapes = aoe.ActiveAOEs(0, boss);
            check(shapes.Length == 1 && shapes[0].Origin == center && shapes[0].Shape.Check(center, center, default), $"{name} 实际读条后范围位于场心");
            aoe.OnCastFinished(helper, cast);
            helper.CastInfo = null;
            aoe.Update();
            check(aoe.ActiveAOEs(0, boss).Length == 0, $"{name} 读条结束清理显示");
        }
    }

    private static void VerifyBoardAI(Action<bool, string> check)
    {
        var world = NewWorld();
        var center = new WPos(120, -420);
        var player = Create(world, 1, 0, center, ActorType.Player);
        world.Execute(new PartyState.OpModify(0, new(1, player.InstanceID, false, "player")));
        var boss = Create(world, 2, 0x4D03, center);
        using var giant = BossModuleRegistry.CreateModuleForActor(world, boss, BossModuleInfo.Maturity.Contributed)!;
        giant.StateMachine.Start(world.CurrentTime);
        var bait = (GenericBaitProximity)giant.Components.Single(c => c.GetType().Name == "SmashingStampBait");
        var left = Create(world, 3, 0x4D05, center + new WDir(-10, 0));
        var right = Create(world, 4, 0x4D07, center + new WDir(10, 0));
        bait.OnEventIcon(player, 234, player.InstanceID);
        bait.Update();
        var hints = new AIHints();
        bait.AddAIHints(0, player, default, hints);
        check(hints.ForbiddenZones.Count == 1 && !hints.ForbiddenZones[0].shapeDistance.Contains(left.Position)
            && !hints.ForbiddenZones[0].shapeDistance.Contains(right.Position)
            && hints.ForbiddenZones[0].shapeDistance.Contains(center), "巨人未染色时两个软泥怪站位均可选，不形成互斥禁区");
        var element = new ActorStatus(2056, 0x499, world.FutureTime(10), boss.InstanceID);
        bait.OnStatusGain(boss, ref element);
        hints.Clear();
        bait.AddAIHints(0, player, default, hints);
        check(hints.ForbiddenZones.Count == 1 && hints.ForbiddenZones[0].shapeDistance.Contains(left.Position)
            && !hints.ForbiddenZones[0].shapeDistance.Contains(right.Position), "巨人雷属性仅保留火软泥怪站位");

        var world2 = NewWorld();
        center = new(120, 0);
        player = Create(world2, 1, 0, center, ActorType.Player);
        world2.Execute(new PartyState.OpModify(0, new(1, player.InstanceID, false, "player")));
        boss = Create(world2, 2, 0x4CFE, center);
        using var sphinx = BossModuleRegistry.CreateModuleForActor(world2, boss, BossModuleInfo.Maturity.Contributed)!;
        sphinx.StateMachine.Start(world2.CurrentTime);
        var assignment = (GenericAOEs)sphinx.Components.Single(c => c.GetType().Name == "Assignment");
        var prime = Create(world2, 3, 0x1EC106, center + new WDir(-12, 0), ActorType.EventObj);
        var composite = Create(world2, 4, 0x1EC108, center + new WDir(12, 0), ActorType.EventObj);
        var status = new ActorStatus(5150, 0, world2.FutureTime(8), boss.InstanceID);
        assignment.OnStatusGain(player, ref status);
        hints.Clear();
        assignment.AddAIHints(0, player, default, hints);
        check(hints.ForbiddenZones.Count == 1 && !hints.ForbiddenZones[0].shapeDistance.Contains(prime.Position)
            && hints.ForbiddenZones[0].shapeDistance.Contains(composite.Position), "斯芬克斯不先绘图也按素数状态生成安全区");
        assignment.OnStatusLose(player, ref status);
        hints.Clear();
        assignment.AddAIHints(0, player, default, hints);
        check(hints.ForbiddenZones.Count == 0 && assignment.ActiveAOEs(-1, player).Length == 0, "数字状态消失后立即释放移动限制，无效槽位无异常");
    }

    private static void VerifyDashGate(Action<bool, string> check)
    {
        var world = NewWorld();
        var player = Create(world, 1, 0, default, ActorType.Player);
        var target = Create(world, 2, 0x1234, new(0, 5));
        var config = Service.Config.Get<ActionTweaksConfig>();
        var oldSafety = config.DashSafety;
        var oldExtra = config.DashSafetyExtra;
        try
        {
            config.DashSafety = config.DashSafetyExtra = false;
            var hints = new AIHints { ForbidDashes = true };
            var action = new ActionQueue.Entry { Target = target, TargetPos = new(0, 0, 5) };
            ActionDefinition.ConditionDelegate[] guards =
            [
                ActionDefinitions.DashToTargetCheck,
                ActionDefinitions.DashToPositionCheck,
                ActionDefinitions.DashFixedDistanceCheck(15),
                ActionDefinitions.DashFixedDistanceCheck(15, true),
                ActionDefinitions.BackdashCheck(10),
                ActionDefinitions.Instance[ActionID.MakeSpell(BossMod.Data.PhantomID.PhantomKick)]!.ForbidExecute!
            ];
            foreach (var guard in guards)
                check(guard(world, player, action, hints), "机制禁止位移覆盖目标、地点、前冲、后跳与辅助职业，关闭几何检查也生效");
            hints.Clear();
            check(!hints.ForbidDashes && guards.All(g => !g(world, player, action, hints)), "机制结束后清除禁位移，不残留限制");
        }
        finally { config.DashSafety = oldSafety; config.DashSafetyExtra = oldExtra; }
    }

    private static void VerifyTwister(Action<bool, string> check)
    {
        var world = NewWorld();
        var player = Create(world, 1, 0, default, ActorType.Player);
        world.Execute(new PartyState.OpModify(0, new(1, player.InstanceID, false, "player")));
        var boss = Create(world, 2, 0xffff2101, default);
        using var module = new EntryProbe(world, boss);
        var twister = new CastTwister(module, 1.5f, 0x1234, 42, 0.4, 0.65);
        var cast = new ActorCastInfo { Action = new(ActionType.Spell, 42), TotalTime = 4.7f };
        var start = world.CurrentTime;
        twister.OnCastStarted(boss, cast);
        world.Frame = new(start.AddSeconds(4.7), 0, 0, 0, 0, 1);
        twister.Update();
        check(twister.ActiveAOEs(0, player).Length == 0, "旋风不会在预测窗口之前显示");
        world.Frame = new(start.AddSeconds(4.9), 0, 0, 0, 0, 1);
        twister.Update();
        var aoes = twister.ActiveAOEs(0, player);
        check(aoes.Length == 1 && Math.Abs((aoes[0].Activation - start).TotalSeconds - 5.4) < 0.0001, "晚帧更新不会推迟旋风真实结算时刻，DD40保留提前0.25秒窗口");
        twister.OnActorCreated(Create(world, 3, 0x1234, default));
        check(twister.ActiveAOEs(0, player).Length == 1, "旋风实体生成后移除预测，仅保留真实范围");
    }

    private static void VerifyViewer(Action<bool, string> check)
    {
        using var viewer = new ModuleViewer(null, NewWorld());
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var initialized = typeof(ModuleViewer).GetField("_initialized", flags)!;
        check(!(bool)initialized.GetValue(viewer)!, "支持副本列表构造时不初始化全部分组");
        typeof(ModuleViewer).GetMethod("EnsureInitialized", flags)!.Invoke(viewer, null);
        check((bool)initialized.GetValue(viewer)!, "支持副本列表首次需要时初始化成功");
        var groups = (Array)typeof(ModuleViewer).GetField("_groups", flags)!.GetValue(viewer)!;
        var ensureModules = typeof(ModuleViewer).GetMethod("EnsureModules", flags)!;
        var displayed = 0;
        foreach (var cell in groups)
        {
            if (cell is not System.Collections.IEnumerable list)
                continue;
            foreach (var group in list)
            {
                var modules = (System.Collections.IList)ensureModules.Invoke(viewer, [group])!;
                displayed += modules.Count;
                check(ReferenceEquals(modules, ensureModules.Invoke(viewer, [group])), "副本分组重复展开复用缓存");
            }
        }
        check(displayed == BossModuleRegistry.RegisteredModules.Count, "全部副本按需展开无遗漏，名称及成熟度元数据可解析");
        var text = UICombo.EnumString(BossModuleInfo.Maturity.Contributed);
        check(!string.IsNullOrEmpty(text) && text == UICombo.EnumString((Enum)BossModuleInfo.Maturity.Contributed), "枚举缓存与动态类型路径返回相同成熟度说明");
    }

    private static WorldState NewWorld()
    {
        var world = new WorldState(10000000, "september-core");
        world.Frame = new(DateTime.UnixEpoch.AddHours(1), 0, 0, 0, 0, 1);
        return world;
    }

    private static Actor Create(WorldState world, ulong id, uint oid, WPos pos, ActorType type = ActorType.Enemy)
    {
        world.Execute(new ActorState.OpCreate(id, oid, (int)id, 0, "fixture", 0, type, Class.None, 100, new(pos.X, 0, pos.Z, 0), .5f, new(1000, 1000, 0, 10000, 10000), true, false, 0, 0, 0));
        return world.Actors.Find(id)!;
    }
}
