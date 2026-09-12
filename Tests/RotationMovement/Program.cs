using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using BossMod;
using BossMod.Autorotation;
using BossMod.Autorotation.MiscAI;
using BossMod.Components;
using BossMod.Pathfinding;
using Dalamud.Bindings.ImGui;

class Program
{
    const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    static int checks;
    static void Check(bool value, string message) { ++checks; if (!value) throw new Exception(message); }
    static Type Internal(string name) => typeof(AIHints).Assembly.GetType(name, true)!;
    static object? Call(object obj, string name, params object?[] args) => obj.GetType().GetMethod(name, All)!.Invoke(obj, args);
    static void Set(object obj, string name, object? value) => obj.GetType().GetField(name, All)!.SetValue(obj, value);
    static T Get<T>(object obj, string name) => (T)obj.GetType().GetField(name, All)!.GetValue(obj)!;
    static async Task<int> Main(string[] args)
    {
        var dalamud = Environment.GetEnvironmentVariable("DALAMUD_HOME") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher/addon/Hooks/dev");
        AssemblyLoadContext.Default.Resolving += (_, name) => File.Exists(Path.Combine(dalamud, name.Name + ".dll")) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(dalamud, name.Name + ".dll")) : null;
        if (args.Length is not (2 or 4) || args[0] != "--sqpack" || args.Length == 4 && args[2] != "--autoduty") { Console.Error.WriteLine("用法：--sqpack <游戏 game/sqpack 目录>；仅离线读取游戏数据。"); return 2; }
        try { await Run(args[1], args.Length == 4 ? args[3] : null); Console.WriteLine($"通过：{checks} 项循环/移动、异步切换及组件断言。"); return 0; }
        catch (Exception ex) { Console.Error.WriteLine(ex.GetBaseException()); return 1; }
    }
    static async Task Run(string sqpack, string? autoDutyDirectory)
    {
        using var data = new Lumina.GameData(sqpack, new Lumina.LuminaOptions { DefaultExcelLanguage = Lumina.Data.Language.ChineseSimplified });
        Service.LuminaGameData = data;
        Service.Config.Initialize();
        Service.WindowSystem = new("offline");
        var imgui = ImGui.CreateContext();
        try
        {
            var world = new WorldState(10000000, "offline");
            Actor Create(ulong id, int index, uint oid, ActorType type, WPos p)
            {
                world.Execute(new ActorState.OpCreate(id, oid, index, 0, "fixture", 0, type, type == ActorType.Player ? Class.PLD : Class.None, 100, new(p.X, 0, p.Z, 0), .5f, new(1000, 1000, 0, 10000, 10000), true, type == ActorType.Player, 0, 0, 0));
                return world.Actors.Find(id)!;
            }
            var player = Create(1, 0, 0, ActorType.Player, default);
            world.Execute(new PartyState.OpModify(0, new(1, 1, false, "player")));
            var master = Create(2, 2, 0, ActorType.Player, new(12, 0));
            world.Execute(new PartyState.OpModify(1, new(2, 2, false, "master")));
            var boss = Create(20, 4, 0xffff2101, ActorType.Enemy, default);
            var info = BossModuleRegistry.Info.Build(typeof(EntryProbe))!;
            typeof(BossModuleRegistry).GetMethod("Register", All)!.Invoke(null, [info]);
            using var module = new EntryProbe(world, boss);
            using var bossmods = new BossModuleManager(world);
            var hints = new AIHints();
            var temp = Path.Combine(Path.GetTempPath(), "BossModRotationMovement");
            var db = new RotationDatabase(new DirectoryInfo(temp), new FileInfo(Path.Combine(temp, "empty-defaults.json")));
            using var manager = new RotationModuleManager(db, bossmods, hints);
            var config = Service.Config._nodes[Internal("BossMod.AI.AIConfig")];
            Set(config, "FocusTargetMaster", false);
            var output = new Preset("职业输出");
            output.AddModule(typeof(OutputProbe), new("probe", "", "", "", RotationModuleQuality.Good, new(~0ul), 100), (m, p) => new OutputProbe(m, p));
            Preset MovePreset(string name, NormalMovement.DestinationStrategy strategy)
            {
                var p = new Preset(name);
                var index = p.AddModule(typeof(NormalMovement), NormalMovement.Definition(), (m, a) => new NormalMovement(m, a));
                p.Modules[index].SerializedSettings.Add(new(0, (int)NormalMovement.Track.Destination, new StrategyValueTrack { Option = (int)strategy, Target = StrategyTarget.PointAbsolute, Offset1 = 10, Offset2 = 0 }));
                return p;
            }
            void Tick() { hints.Clear(); manager.Update(0, false, false); }
            var movement = MovePreset("任意名称", NormalMovement.DestinationStrategy.Explicit);
            manager.Activate(output); manager.Activate(movement); Tick();
            Check(manager.MovementModuleActive && hints.ForcedMovement is { X: > 0 } && hints.ActionsToExecute.Entries.Count == 1, "循环与移动同时执行");
            movement.Name = "重命名后"; Tick();
            Check(manager.MovementModuleActive && hints.ForcedMovement is { X: > 0 }, "预设改名不改变移动");
            manager.Deactivate(movement); Tick();
            Check(!manager.MovementModuleActive && hints.ForcedMovement == null && hints.ActionsToExecute.Entries.Count == 1, "只关闭移动");
            manager.Activate(movement); manager.Deactivate(output); Tick();
            Check(manager.MovementModuleActive && hints.ForcedMovement is { X: > 0 } && hints.ActionsToExecute.Entries.Count == 0, "只关闭职业循环");
            manager.Activate(output);
            var stop = MovePreset("停止", NormalMovement.DestinationStrategy.None);
            manager.Deactivate(movement); manager.Activate(stop); manager.Activate(movement); Tick();
            Check(manager.MovementModuleActive && hints.ForcedMovement == null && hints.ActionsToExecute.Entries.Count == 1, "第一个移动模块明确停止，后续模块不覆盖");
            var controller = RuntimeHelpers.GetUninitializedObject(Internal("BossMod.AI.AIController"));
            var ai = RuntimeHelpers.GetUninitializedObject(Internal("BossMod.AI.AIManager"));
            Set(ai, "Autorot", manager); Set(ai, "Controller", controller);
            Set(controller, "NaviTargetPos", (WPos?)new WPos(30, 0));
            Call(ai, "Update");
            Check(Get<WPos?>(controller, "NaviTargetPos") == null && hints.ForcedMovement == null, "旧 AI 不接管原版停止结果");
            manager.SetForceDisabled(); Tick(); Call(ai, "Update");
            Check(manager.IsForceDisabled && !manager.MovementModuleActive && hints.ActionsToExecute.Entries.Count == 0 && hints.ForcedMovement == null, "总停止不被 AI 清除");
            manager.Clear(); manager.Activate(output); manager.Activate(movement);
            Set(config, "ForbidMovement", true); Tick();
            Check(hints.ForcedMovement == null && hints.ActionsToExecute.Entries.Count == 1, "禁止移动不停止职业输出");
            Set(config, "ForbidMovement", false);
            foreach (var mode in new[] { AIHints.SpecialMode.Pyretic, AIHints.SpecialMode.PyreticMove, AIHints.SpecialMode.NoMovement })
            {
                hints.Clear(); hints.ImminentSpecialMode = (mode, world.CurrentTime, DateTime.MaxValue); manager.Update(0, false, false); Call(ai, "Update");
                Check(hints.ForcedMovement == null && manager.MovementModuleActive, "特殊状态不回退旧路径 " + mode);
            }
            player.PendingKnockbacks.Add(default); Tick(); Check(hints.ForcedMovement == null, "击退待结算不移动"); player.PendingKnockbacks.Clear();
            // Legacy AI wrappers have native game services in their constructors. The fixture uses only their navigation fields.
            var behaviourType = Internal("BossMod.AI.AIBehaviour");
            object Behaviour() => Activator.CreateInstance(behaviourType, controller, manager)!;
            var follow = Behaviour(); master.InCombat = true; hints.Clear(); Call(follow, "AddFollowHints", player, master);
            Check(hints.GoalZones.Count == 1 && hints.GoalZones[0](master.Position) > hints.GoalZones[0](player.Position), "手动跟随向原版移动提供目标");
            Set(follow, "_masterMovementStart", master.Position); Set(config, "FollowDuringCombat", false); hints.Clear(); Call(follow, "AddFollowHints", player, master); Check(hints.GoalZones.Count == 0, "跟随开关保留"); Set(config, "FollowDuringCombat", true); Call(follow, "Dispose");
            foreach (var action in new[] { "Suspend", "Dispose", "Pause" })
            {
                hints.Clear(); hints.GoalZones.Add(p => p.X >= 3 ? 1 : 0);
                using var block = new BlockingShape(); hints.ForbiddenZones.Add((block, world.CurrentTime, 0));
                var behaviour = Behaviour();
                var task = (Task)Call(behaviour, "Execute", player, player)!;
                try
                {
                    Check(block.Entered.Wait(5000), "旧寻路已开始 " + action);
                    if (action == "Pause")
                    {
                        manager.Clear(); manager.Update(0, false, false);
                        Set(ai, "Beh", behaviour); Set(config, "ForbidMovement", true); Call(ai, "Update");
                    }
                    else
                        Call(behaviour, action);
                    var current = new WPos(99, 99); Set(controller, "NaviTargetPos", (WPos?)current);
                    block.Release.Set(); await task.WaitAsync(TimeSpan.FromSeconds(10));
                    Check(Get<WPos?>(controller, "NaviTargetPos") == current, "旧异步结果不能覆盖新状态 " + action);
                }
                finally { block.Release.Set(); await task.WaitAsync(TimeSpan.FromSeconds(10)); Call(behaviour, "Dispose"); Set(config, "ForbidMovement", false); Set(ai, "Beh", null); }
            }
            hints.Clear(); hints.GoalZones.Add(p => p.X >= 3 ? 1 : 0);
            using (var block = new BlockingShape())
            {
                hints.ForbiddenZones.Add((block, world.CurrentTime, 0));
                var normal = new NormalMovement(manager, player);
                var values = new StrategyValues(NormalMovement.Definition().Configs);
                var destination = (StrategyValueTrack)values.Values[(int)NormalMovement.Track.Destination];
                destination.Option = (int)NormalMovement.DestinationStrategy.Pathfind;
                Actor? target = boss; normal.Execute(values, ref target, 0, false);
                var task = Get<Task<NavigationDecision>>(normal, "_decisionTask");
                try
                {
                    Check(block.Entered.Wait(5000), "原版异步寻路已开始");
                    destination.Option = (int)NormalMovement.DestinationStrategy.None; normal.Execute(values, ref target, 0, false);
                    block.Release.Set(); await task.WaitAsync(TimeSpan.FromSeconds(10)); hints.Clear();
                    destination.Option = (int)NormalMovement.DestinationStrategy.Pathfind; normal.Execute(values, ref target, 0, false);
                    Check(hints.ForcedMovement == null, "重新启用不采用停用前路径");
                    await Get<Task<NavigationDecision>>(normal, "_decisionTask").WaitAsync(TimeSpan.FromSeconds(10));
                }
                finally { block.Release.Set(); await task.WaitAsync(TimeSpan.FromSeconds(10)); }
            }
            bossmods.ActiveModule = module; module.StateMachine.Start(world.CurrentTime); player.PosRot = new(30, 0, 0, 0);
            var entry = new NormalMovement(manager, player); var entryValues = new StrategyValues(NormalMovement.Definition().Configs);
            ((StrategyValueTrack)entryValues.Values[(int)NormalMovement.Track.Destination]).Option = (int)NormalMovement.DestinationStrategy.Pathfind;
            Actor? noTarget = null; hints.Clear(); entry.Execute(entryValues, ref noTarget, 0, false); Check(hints.ForcedMovement is { X: < 0 }, "保留副本入场");
            bossmods.ActiveModule = null; player.PosRot = new(0, 0, -5, 0);
            var gaze = new GazeProbe(module); var text = new BossComponent.TextHints(); hints.Clear(); gaze.AddHints(0, player, text); gaze.AddAIHints(0, player, default, hints);
            Check(text.Count > 0 && hints.ForbiddenDirections.Count == 1, "视线提示默认开启");
            gaze.EnableHints = false; text = new(); hints.Clear(); gaze.AddHints(0, player, text); gaze.AddAIHints(0, player, default, hints);
            Check(text.Count == 0 && hints.ForbiddenDirections.Count == 0, "文字与 AI 视线提示可关闭");
            bool Visible(WPos p, GenericGaze.Eye eye) => (bool)typeof(GenericGaze).GetMethod("EyeInDrawRange", All)!.Invoke(gaze, [p, eye])!;
            var eye = new GenericGaze.Eye(default, range: 10);
            Check(Visible(new(0, 5), eye) && Visible(new(0, 12), eye) && !Visible(new(0, 12.1f), eye), "视线绘图范围与20%余量方向正确");
            gaze.DrawEyeRange = false; Check(Visible(new(0, 1000), eye), "可关闭绘图距离限制");
            foreach (var (name, enabled) in new[] { ("P2SanctityOfTheWard1Gaze", true), ("P5DeathOfTheHeavensGaze", false) })
            {
                var g = (GenericGaze)Activator.CreateInstance(Internal("BossMod.Endwalker.Ultimate.DSW2." + name), module)!;
                Check(g.EnableHints == enabled && !g.DrawEyeRange, "绝龙诗保留开关及提前绘图 " + name);
            }
            var actors = Enumerable.Range(0, 8).Select(i => new Actor((ulong)(100 + i), 0, i * 2, 0, "fixture", 0, ActorType.Player, Class.PLD, 100, new(MathF.Sin(i * MathF.PI / 4) * 10, 0, MathF.Cos(i * MathF.PI / 4) * 10, 0))).ToArray();
            foreach (var ccw in new[] { false, true }) foreach (var start in actors)
            {
                var ordered = actors.ToList().ClockOrder(start, default, ccw);
                var compat = actors.Where(_ => true).ClockOrder(start, default, ccw);
                Check(ordered[0] == start && ordered.Distinct().Count() == 8 && ordered.SequenceEqual(compat), "排序包装保持顺序");
                var indexed = actors.Select((a, i) => (i, a)).ToArray().ClockOrder(start, default, ccw);
                Check(indexed.Select(x => x.Item2).SequenceEqual(ordered), "带槽位排序一致");
            }
            Check(Array.Empty<Actor>().ClockOrder(player, default).Length == 0, "空排序安全");
            var bad = new GroupAssignmentLightParties(); Check(bad.Resolve(world.Party, new PartyRolesConfig()).Count == 0, "无效分组返回空列表");
            IPCTests.Run(manager, bossmods, hints, ai, Check, autoDutyDirectory);
            MechanicSafetyTests.Run(module, player, Check);
            RadarDrawingTests.Run(Check);
            TankFacingTests.Run(manager, player, boss, Check);
            BoardUpstreamTests.Run(Check);
            UCOBUpstreamTests.Run(Check);
            ModuleManagementTests.Run(bossmods, boss, Check);
        PrePullMigrationTests.Run(Check);
            Console.WriteLine("循环/移动开关、预设改名与顺序、跟随、旧任务失效、视线及排序检查完成。");
        }
        finally { ImGui.DestroyContext(imgui); }
    }
}
sealed class BlockingShape : ShapeDistance, IDisposable
{
    public readonly ManualResetEventSlim Entered = new();
    public readonly ManualResetEventSlim Release = new();
    public override float Distance(in WPos p) { Entered.Set(); if (!Release.Wait(10000)) throw new TimeoutException("等待测试放行"); return 10; }
    public void Dispose() { Release.Set(); Entered.Dispose(); Release.Dispose(); }
}
sealed class OutputProbe(RotationModuleManager m, Actor p) : RotationModule(m, p)
{
    public override void Execute(StrategyValues s, ref Actor? target, float delay, bool moving) => Hints.ActionsToExecute.Push(new ActionID(ActionType.Spell, 9), Player, 1000);
}
[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = 0xffff2101, Expansion = BossModuleInfo.Expansion.Global, Category = BossModuleInfo.Category.Dungeon)]
public sealed class EntryProbe(WorldState w, Actor a) : BossModule(w, a, default, new ArenaBoundsCircle(20));
public sealed class EntryProbeStates : StateMachineBuilder { public EntryProbeStates(EntryProbe m) : base(m) { TrivialPhase(); } }
sealed class GazeProbe(BossModule m) : GenericGaze(m)
{
    private readonly Eye[] _eyes = [new(default, range: 10)];
    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor) => _eyes;
}
