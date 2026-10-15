using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using BossMod;
using BossMod.AI;
using BossMod.Autorotation;
using BossMod.Autorotation.MiscAI;

static class UpstreamCastIPC20261015Tests
{
    const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    static Type Internal(string name) => typeof(AIHints).Assembly.GetType(name, true)!;

    public static void Run(RotationModuleManager manager, Actor player, object ai, Action<bool, string> check)
    {
        TestConfiguration(check);
        TestConcurrentConfiguration(check);
        TestMovement(manager, player, check);
        TestCastBlocking(manager, check);
        TestIPC(manager, ai, check);
        TestCastHistory(check);
    }

    static void TestConfiguration(Action<bool, string> check)
    {
        var root = new ConfigRoot(); root.Initialize();
        var node = root.Get<ActionTweaksConfig>();
        var original = node.AnimationLockDelayMax;
        var changed = 0;
        using var subscription = root.Modified.Subscribe(() => ++changed);
        string[] Args(string value) => [nameof(ActionTweaksConfig), nameof(ActionTweaksConfig.AnimationLockDelayMax), value];
        var directory = Path.Combine(Path.GetTempPath(), "BossMod-Transient-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        int Saved(string path)
        {
            using var json = JsonDocument.Parse(File.ReadAllText(path, System.Text.Encoding.UTF8));
            return json.RootElement.GetProperty("Payload").GetProperty(typeof(ActionTweaksConfig).FullName!).GetProperty(nameof(ActionTweaksConfig.AnimationLockDelayMax)).GetInt32();
        }
        try
        {
            check(root.SetTransient(Args("30")).Count == 0 && node.AnimationLockDelayMax == 30, "临时配置立即生效");
            check(root.SetTransient(Args("40")).Count == 0 && node.AnimationLockDelayMax == 40, "重复临时覆盖更新当前值");
            var file = new FileInfo(Path.Combine(directory, "snapshot.json")); root.SaveToFile(file);
            check(Saved(file.FullName) == original, "其他配置触发保存时不写入临时值");
            check(root.ClearTransient() == 1 && node.AnimationLockDelayMax == original && root.ClearTransient() == 0, "重复清理恢复第一次覆盖前的原值");
            check(changed == 3, "覆盖及恢复通知配置监听器");

            root.SetTransient(Args("30"));
            check(root.SetTransient(Args("invalid")).Count > 0 && node.AnimationLockDelayMax == 30, "无效数值不破坏已有覆盖");
            check(root.SetTransient([nameof(ActionTweaksConfig)]).Count > 0 && root.SetTransient(["missing", "missing", "1"]).Count > 0, "缺参和未知字段返回错误");
            check(root.SetTransient([nameof(ActionTweaksConfig), "Delay", "true"]).Count > 0, "缩写歧义返回错误");
            check(root.ClearTransient() == 1 && node.AnimationLockDelayMax == original, "错误输入后仍恢复用户原值");

            root.SetTransient(Args("30")); node.AnimationLockDelayMax = 35; node.Modified.Fire();
            root.SaveToFile(file);
            check(root.ClearTransient() == 0 && node.AnimationLockDelayMax == 35 && Saved(file.FullName) == 35, "用户界面修改优先于旧临时覆盖");
            root.SetTransient(Args("40"));
            check(root.ConsoleCommand(Args("45"), false).Count == 0 && root.ClearTransient() == 0 && node.AnimationLockDelayMax == 45, "旧 Configuration(save=false) 保留显式新值并移除临时覆盖");
            root.SetTransient(Args("30")); node.AnimationLockDelayMax = 37;
            root.SetTransient(Args("40")); root.ClearTransient();
            check(node.AnimationLockDelayMax == 37, "尚未通知的直接用户编辑也成为下一次覆盖的原值");

            var keep = node.PreventMovingWhileCasting;
            root.SetTransient([nameof(ActionTweaksConfig), nameof(ActionTweaksConfig.PreventMovingWhileCasting), (!keep).ToString()]);
            var keepEnum = node.MoveEscapeHatch;
            root.SetTransient([nameof(ActionTweaksConfig), nameof(ActionTweaksConfig.MoveEscapeHatch), "Ctrl"]);
            check(node.PreventMovingWhileCasting == !keep && node.MoveEscapeHatch == ActionTweaksConfig.ModifierKey.Ctrl, "布尔与枚举临时配置兼容现有转换");
            check(root.ClearTransient() == 2 && node.PreventMovingWhileCasting == keep && node.MoveEscapeHatch == keepEnum, "多个字段独立恢复");

            node.AnimationLockDelayMax = original;
            var saves = Enumerable.Range(0, 24).Select(i => Task.Run(() =>
            {
                root.SetTransient(Args(i % 2 == 0 ? "30" : "40"));
                root.SaveToFile(new FileInfo(Path.Combine(directory, i + ".json")));
            })).ToArray();
            Task.WaitAll(saves);
            check(Enumerable.Range(0, 24).All(i => Saved(Path.Combine(directory, i + ".json")) == original), "并发覆盖和后台保存均只持久化用户原值");
            check(root.ClearTransient() == 1 && node.AnimationLockDelayMax == original, "并发覆盖后正常清理");
        }
        finally { Directory.Delete(directory, true); }
    }

    // Only the three public cast-timer fields are backed by native-shaped memory; no game hooks or functions are invoked.
    sealed unsafe class LiveCast : IDisposable
    {
        public readonly ActionManagerEx Amex = (ActionManagerEx)RuntimeHelpers.GetUninitializedObject(typeof(ActionManagerEx));
        readonly Type nativeType;
        readonly IntPtr memory;
        public LiveCast()
        {
            var pointer = typeof(ActionManagerEx).GetField("_inst", All)!;
            nativeType = pointer.FieldType.GetElementType()!;
            memory = Marshal.AllocHGlobal(Marshal.SizeOf(nativeType));
            new Span<byte>((void*)memory, Marshal.SizeOf(nativeType)).Clear();
            pointer.SetValue(Amex, Pointer.Box((void*)memory, pointer.FieldType));
            Set(0, false);
        }
        public void Set(float remaining, bool interrupts)
        {
            Marshal.WriteInt32(memory, Marshal.OffsetOf(nativeType, "CastSpellId").ToInt32(), remaining > 0 ? 119 : 0);
            Marshal.WriteInt32(memory, Marshal.OffsetOf(nativeType, "CastTimeTotal").ToInt32(), BitConverter.SingleToInt32Bits(remaining));
            Marshal.WriteInt32(memory, Marshal.OffsetOf(nativeType, "CastTimeElapsed").ToInt32(), 0);
            Amex.MoveMightInterruptCast = interrupts;
        }
        public void Dispose() => Marshal.FreeHGlobal(memory);
    }

    static void TestConcurrentConfiguration(Action<bool, string> check)
    {
        var root = new ConfigRoot(); root.Initialize();
        var node = root.Get<ActionTweaksConfig>();
        string[] Args(string value) => [nameof(ActionTweaksConfig), nameof(ActionTweaksConfig.AnimationLockDelayMax), value];
        root.SetTransient(Args("30"));
        var ledger = (System.Collections.IDictionary)typeof(ConfigRoot).GetField("_transient", All)!.GetValue(root)!;
        var key = (ITuple)ledger.Keys.Cast<object>().Single();
        var field = key[1];
        var userEdit = typeof(ConfigRoot).GetMethod("ApplyUserEdit", All)!;
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        using var clearStarted = new ManualResetEventSlim();
        var edit = Task.Run(() => userEdit.Invoke(root, [node, field, (Func<bool>)(() =>
        {
            node.AnimationLockDelayMax = 37;
            entered.Set();
            if (!release.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("UI edit gate timeout");
            return true;
        })]));
        Task<int>? clear = null;
        try
        {
            check(entered.Wait(TimeSpan.FromSeconds(5)), "并发配置：实际UI写入进入共同门闩");
            clear = Task.Run(() => { clearStarted.Set(); return root.ClearTransient(); });
            check(clearStarted.Wait(TimeSpan.FromSeconds(5)) && !clear.Wait(50), "并发配置：清理不能插入UI修改的读写窗口");
        }
        finally { release.Set(); edit.GetAwaiter().GetResult(); clear?.GetAwaiter().GetResult(); }
        check(node.AnimationLockDelayMax == 37 && clear!.Result == 0, "并发配置：清理保留用户新值并移除临时账本");
        root.SetTransient(Args("40"));
        userEdit.Invoke(root, [node, field, (Func<bool>)(() => { node.AnimationLockDelayMax = 40; return true; })]);
        check(root.ClearTransient() == 0 && node.AnimationLockDelayMax == 40, "并发配置：显式确认临时同值也转为永久值");

        var directory = Path.Combine(Path.GetTempPath(), "BossMod-SameFile-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "config.json");
        var saves = new System.Collections.Concurrent.ConcurrentQueue<Task>();
        using var subscription = root.Modified.Subscribe(() => saves.Enqueue(Task.Run(() => root.SaveToFile(new(path)))));
        for (var round = 0; round < 4; ++round)
        {
            for (var value = 26; value <= 45; ++value)
            {
                root.ConsoleCommand(Args(value.ToString()), true);
                Thread.Yield();
            }
            Task.WaitAll(saves.ToArray());
            using var json = JsonDocument.Parse(File.ReadAllText(path, System.Text.Encoding.UTF8));
            var saved = json.RootElement.GetProperty("Payload").GetProperty(typeof(ActionTweaksConfig).FullName!).GetProperty(nameof(ActionTweaksConfig.AnimationLockDelayMax)).GetInt32();
            check(saved == 45, "并发配置：同一实际配置文件竞争保存仍保留最新永久值 " + round);
        }
    }

    static void TestMovement(RotationModuleManager manager, Actor player, Action<bool, string> check)
    {
        using var live = new LiveCast();
        var previous = ActionManagerEx.Instance;
        var previousMovement = NormalMovement.Instance;
        var previousCast = player.CastInfo;
        var previousPosition = player.PosRot;
        var previousFrame = manager.WorldState.Frame;
        var aiConfig = Service.Config._nodes[Internal("BossMod.AI.AIConfig")];
        var forbidField = aiConfig.GetType().GetField("ForbidMovement")!;
        var previousForbid = forbidField.GetValue(aiConfig);
        var hints = manager.Hints;
        var now = DateTime.UnixEpoch.AddDays(1);
        var movement = new NormalMovement(manager, player);
        var values = new StrategyValues(NormalMovement.Definition().Configs);
        var destination = (StrategyValueTrack)values.Values[(int)NormalMovement.Track.Destination];
        destination.Option = (int)NormalMovement.DestinationStrategy.Explicit;
        destination.Target = StrategyTarget.PointAbsolute; destination.Offset1 = 10; destination.Offset2 = 0;
        var cast = (StrategyValueTrack)values.Values[(int)NormalMovement.Track.Cast];
        void Tick(double seconds)
        {
            manager.WorldState.Frame = new(now.AddSeconds(seconds), 0, 0, 0, 0, 1);
            hints.Clear(); Actor? target = null; movement.Execute(values, ref target, 0, false);
        }
        try
        {
            ActionManagerEx.Instance = live.Amex; player.CastInfo = null; player.PosRot = default;
            forbidField.SetValue(aiConfig, false);
            live.Set(2, true); cast.Option = (int)NormalMovement.CastStrategy.Greedy; Tick(0);
            check(hints.ForcedMovement == null && !hints.HoldCasts, "世界快照尚无施法时仍保护客户端新读条");
            cast.Option = (int)NormalMovement.CastStrategy.Leeway; Tick(.1);
            check(hints.ForcedMovement is { X: > 0 } && hints.ForceCancelCast && hints.HoldCasts && hints.MaxCastTime == 0, "无躲避余量抢占实时读条并禁止立即重读");
            live.Set(0, false); Tick(.3);
            check(hints.ForcedMovement is { X: > 0 } && hints.HoldCasts, "打断后继续移动时延长停步保护");
            player.PosRot = new(10, 0, 0, 0); Tick(.5);
            check(hints.ForcedMovement == null && hints.HoldCasts && hints.MaxCastTime == 0, "到达目标后继续保护停步窗口");
            Tick(.81);
            check(!hints.HoldCasts, "连续停步超过 0.5 秒解除保护");
            player.PosRot = default; player.CastInfo = new() { TotalTime = 2 }; cast.Option = (int)NormalMovement.CastStrategy.Greedy; Tick(1);
            check(hints.ForcedMovement == null, "客户端未跟踪的交互仍回退到世界施法信息");
            player.CastInfo = null; live.Set(.4f, false); Tick(1.1);
            check(hints.ForcedMovement is { X: > 0 } && !hints.HoldCasts, "成功结算后的滑步窗口不触发停步保护");

            var mover = (MovementOverride)RuntimeHelpers.GetUninitializedObject(typeof(MovementOverride));
            var controller = Activator.CreateInstance(Internal("BossMod.AI.AIController"), manager.WorldState, live.Amex, mover)!;
            void Set(string field, object? value) => controller.GetType().GetField(field, All)!.SetValue(controller, value);
            void ControllerTick(double seconds)
            {
                hints.Clear(); controller.GetType().GetMethod("Update")!.Invoke(controller, [player, hints, now.AddSeconds(seconds)]);
            }
            live.Set(2, true); Set("NaviTargetPos", (WPos?)new WPos(10, 0)); Set("NaviLeewaySeconds", 3f); ControllerTick(2);
            check(hints.ForcedMovement == null, "基础 AI 以客户端实时读条和导航余量决定停步");
            Set("NaviLeewaySeconds", 1f); ControllerTick(2.1);
            check(hints.ForcedMovement is { X: > 0 } && hints.HoldCasts, "基础 AI 急迫移动抢占并发出停步保护");
            live.Set(0, true); controller.GetType().GetMethod("Clear")!.Invoke(controller, null);
            Set("NaviTargetPos", (WPos?)new WPos(10, 0)); Set("NaviLeewaySeconds", 1f); ControllerTick(3);
            check(hints.ForcedMovement == null && !hints.HoldCasts, "即将施法时普通移动等待且 Clear 取消旧保护");
            Set("AllowInterruptingCastByMovement", true); ControllerTick(3.05);
            check(hints.ForcedMovement is { X: > 0 }, "显式移动覆盖保留强制朝向机制抢占能力");
            Set("AllowInterruptingCastByMovement", false);
            Set("NaviLeewaySeconds", 0f); ControllerTick(3.1);
            check(hints.ForcedMovement is { X: > 0 } && hints.HoldCasts, "即将施法时零余量允许紧急移动");
            hints.Clear(); hints.ForceCancelCast = true; Set("NaviTargetPos", null);
            controller.GetType().GetMethod("Update")!.Invoke(controller, [player, hints, now.AddSeconds(4)]);
            check(hints.ForceCancelCast, "基础 AI 不擦除机制及其他模块的取消施法请求");
            live.Set(2, true); mover.UserMove = new(1, 0); Set("NaviTargetPos", (WPos?)new WPos(10, 0)); ControllerTick(5);
            check(hints.ForcedMovement == null, "人工输入时基础 AI 不注入第二份移动");
        }
        finally
        {
            ActionManagerEx.Instance = previous; NormalMovement.Instance = previousMovement; player.CastInfo = previousCast; player.PosRot = previousPosition;
            manager.WorldState.Frame = previousFrame; forbidField.SetValue(aiConfig, previousForbid); hints.Clear();
        }
    }

    static void TestCastBlocking(RotationModuleManager manager, Action<bool, string> check)
    {
        using var live = new LiveCast();
        var config = ActionManagerEx.Config;
        var previous = config.PreventMovingWhileCasting;
        var hints = manager.Hints;
        typeof(ActionManagerEx).GetField("_ws", All)!.SetValue(live.Amex, manager.WorldState);
        typeof(ActionManagerEx).GetField("_hints", All)!.SetValue(live.Amex, hints);
        bool Blocked() => (bool)typeof(ActionManagerEx).GetMethod("ShouldBlockMovementForCast", All)!.Invoke(live.Amex, null)!;
        try
        {
            config.PreventMovingWhileCasting = true; live.Set(2, true); hints.Clear();
            check(Blocked(), "普通读条仍使用滑步输入保护");
            hints.ForcedMovement = new(1, 0, 0);
            check(Blocked(), "未明确抢占的移动不会绕过读条保护");
            hints.HoldCasts = true;
            check(!Blocked(), "急迫躲避的移动和停步保护联合信号绕过自动移动阻塞");
            hints.ForcedMovement = null;
            check(Blocked(), "已停步时仍保护真实读条");

            var rsr = (RotationSolverRebornModule)RuntimeHelpers.GetUninitializedObject(typeof(RotationSolverRebornModule));
            var subscriber = typeof(RotationSolverRebornModule).GetField("_autorotationActive", All)!;
            bool hasFunction = false, active = false, throws = false;
            subscriber.SetValue(rsr, IpcProxy.Create(subscriber.FieldType, (method, _) =>
            {
                if (method.Name == "get_HasFunction") return hasFunction;
                if (method.Name == "InvokeFunc") return throws ? throw new InvalidOperationException("IPC unavailable") : active;
                throw new NotSupportedException(method.Name);
            }));
            typeof(ActionManagerEx).GetField("_rsr", All)!.SetValue(live.Amex, rsr);
            check(Blocked(), "RSR 无接口时保留本地施法移动保护");
            hasFunction = true; active = true;
            check(!Blocked(), "RSR 循环激活时由其负责滑步输入保护");
            active = false; check(Blocked(), "RSR 循环关闭时恢复本地保护");
            active = true; throws = true;
            check(Blocked(), "RSR IPC 异常不会绕过本地施法保护");
        }
        finally { config.PreventMovingWhileCasting = previous; hints.Clear(); }
    }

    static void TestIPC(RotationModuleManager manager, object ai, Action<bool, string> check)
    {
        var endpoints = new Dictionary<string, Delegate>();
        var registrations = new Dictionary<string, Type>();
        var serviceProperty = typeof(Service).GetProperty("PluginInterface")!;
        var originalService = serviceProperty.GetValue(null);
        var config = Service.Config.Get<ActionTweaksConfig>();
        var originalValue = config.AnimationLockDelayMax;
        var originalHasSubscribers = Service.Config.Modified.HaveSubscribers();
        var movement = (MovementOverride)RuntimeHelpers.GetUninitializedObject(typeof(MovementOverride));
        IDisposable? provider = null;
        serviceProperty.SetValue(null, IpcProxy.Create(serviceProperty.PropertyType, (method, args) =>
        {
            if (method.Name != "GetIpcProvider") throw new NotSupportedException(method.Name);
            var name = (string)args![0]!; registrations[name] = method.ReturnType;
            return IpcProxy.Create(method.ReturnType, (m, a) =>
            {
                if (m.Name is "RegisterFunc" or "RegisterAction") endpoints[name] = (Delegate)a![0]!;
                else if (m.Name is "UnregisterFunc" or "UnregisterAction") endpoints.Remove(name);
                else if (m.Name != "SendMessage") throw new NotSupportedException(m.Name);
                return null;
            });
        }));
        try
        {
            var obstacles = RuntimeHelpers.GetUninitializedObject(Internal("BossMod.Pathfinding.ObstacleMapManager"));
            provider = (IDisposable)Activator.CreateInstance(Internal("BossMod.IPCProvider"), manager.Bossmods, manager.Hints, manager, null, movement, ai, obstacles)!;
            object? Invoke(string name, params object?[] args) => endpoints["BossMod." + name].DynamicInvoke(args);
            check(registrations["BossMod.Configuration"].GenericTypeArguments[0] == typeof(IReadOnlyList<string>)
                && registrations["BossMod.Configuration.SetTransient"].GenericTypeArguments[0] == typeof(IReadOnlyList<string>), "新旧配置 IPC 都接受只读列表调用方");
            check(Invoke("Configuration.SetTransient", (object)new[] { nameof(ActionTweaksConfig), nameof(ActionTweaksConfig.AnimationLockDelayMax), "40" }) is List<string> { Count: 0 }
                && config.AnimationLockDelayMax == 40, "新临时配置 IPC 可由数组调用");
            check((int)Invoke("Configuration.ClearTransient")! == 1 && config.AnimationLockDelayMax == originalValue, "IPC 清理返回恢复字段数");
            manager.Hints.Clear(); manager.Hints.HoldCasts = true;
            check((bool)Invoke("Movement.IsMoving")!, "外部循环在停步保护期仍得到移动中信号");
            manager.Hints.Clear(); check(!(bool)Invoke("Movement.IsMoving")!, "提示清理后移动状态不残留");
            movement.UserMove = new(1, 0); check((bool)Invoke("Movement.IsMoveRequested")!, "人工移动 IPC 使用真实输入状态");
            Invoke("Configuration.SetTransient", new List<string> { nameof(ActionTweaksConfig), nameof(ActionTweaksConfig.AnimationLockDelayMax), "40" });
            var setter = endpoints["BossMod.Configuration.SetTransient"];
            using var entered = new ManualResetEventSlim();
            using var release = new ManualResetEventSlim();
            var blocked = new BlockingArguments([nameof(ActionTweaksConfig), nameof(ActionTweaksConfig.AnimationLockDelayMax), "41"], entered, release);
            var late = Task.Run(() => setter.DynamicInvoke(new object?[] { blocked }));
            try
            {
                check(entered.Wait(TimeSpan.FromSeconds(5)), "IPC 卸载：调用已进入闭包但仍在物化外部参数");
                provider.Dispose();
            }
            finally { release.Set(); }
            var lateResult = (List<string>)late.GetAwaiter().GetResult()!;
            check(lateResult.Count > 0 && Service.Config.ClearTransient() == 0 && config.AnimationLockDelayMax == originalValue,
                "IPC 卸载：已经进入的延迟调用不能重新写入临时配置");
            check(config.AnimationLockDelayMax == originalValue && endpoints.Count == 0, "IPC 提供器卸载清理接口并恢复临时配置");
            provider.Dispose(); check(config.AnimationLockDelayMax == originalValue, "重复卸载幂等");
            check(Service.Config.Modified.HaveSubscribers() == originalHasSubscribers, "卸载不留下新的配置监听器");
        }
        finally
        {
            provider?.Dispose(); config.AnimationLockDelayMax = originalValue;
            serviceProperty.SetValue(null, originalService); manager.Hints.Clear();
        }
    }

    sealed class BlockingArguments(string[] values, ManualResetEventSlim entered, ManualResetEventSlim release) : IReadOnlyList<string>
    {
        public int Count => values.Length;
        public string this[int index] => values[index];
        public IEnumerator<string> GetEnumerator()
        {
            entered.Set();
            if (!release.Wait(TimeSpan.FromSeconds(5))) throw new TimeoutException("IPC arguments timeout");
            return ((IEnumerable<string>)values).GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    static void TestCastHistory(Action<bool, string> check)
    {
        var tracker = new CastTracker(); var action = new ActionID(ActionType.Spell, 119);
        tracker.OnCastStart(action, 1, default, 2); tracker.OnEffect(2);
        check(tracker.History[0].Outcome == CastTracker.Outcome.Casting, "无关动作效果不结束当前施法记录");
        tracker.OnEffect(1);
        check(tracker.History[0].Outcome == CastTracker.Outcome.Completed, "匹配动作序列按真实效果包结算");
        tracker.OnCastStart(action, 2, default, 2); tracker.Update(new(.1f, 0), 0, true, false);
        check(tracker.History[^1] is { Outcome: CastTracker.Outcome.Interrupted, AutoMoveFrames: 1, RemainingAtEnd: 2 }, "打断帧的自动移动计入施法中断原因");
        tracker.OnCastStart(action, 3, default, .1f); tracker.Update(default, 0, false, false);
        check(tracker.History[^1].Outcome == CastTracker.Outcome.NoEffect, "自然读条结束不误记移动打断");
        tracker.Clear(); check(tracker.History.Count == 0, "清理施法历史");
        for (uint i = 0; i < 25; ++i) { tracker.OnCastStart(action, i, default, 1); tracker.OnEffect(i); }
        check(tracker.History.Count == CastTracker.MaxEntries && tracker.History[0].Sequence == 5, "施法历史按固定上限丢弃最旧记录");
    }
}
