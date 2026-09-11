using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using BossMod;
using BossMod.Autorotation;
using BossMod.Autorotation.MiscAI;

static class IPCTests
{
    public static void Run(RotationModuleManager manager, BossModuleManager bossmods, AIHints hints, object ai, Action<bool, string> check, string? autoDutyDirectory)
    {
        var endpoints = new Dictionary<string, Delegate>();
        var registrations = new Dictionary<string, Type>();
        var pluginInterface = typeof(Service).GetProperty("PluginInterface")!;
        pluginInterface.SetValue(null, IpcProxy.Create(pluginInterface.PropertyType, (method, args) =>
        {
            if (method.Name != "GetIpcProvider")
                throw new NotSupportedException(method.Name);
            var name = (string)args![0]!;
            registrations[name] = method.ReturnType;
            return IpcProxy.Create(method.ReturnType, (m, a) =>
            {
                if (m.Name is "RegisterFunc" or "RegisterAction") endpoints[name] = (Delegate)a![0]!;
                else if (m.Name is "UnregisterFunc" or "UnregisterAction") endpoints.Remove(name);
                else if (m.Name != "SendMessage") throw new NotSupportedException(m.Name);
                return null;
            });
        }));
        Type Internal(string name) => typeof(AIHints).Assembly.GetType(name, true)!;
        var obstacles = RuntimeHelpers.GetUninitializedObject(Internal("BossMod.Pathfinding.ObstacleMapManager"));
        using var provider = (IDisposable)Activator.CreateInstance(Internal("BossMod.IPCProvider"), bossmods, hints, manager, null, null, ai, obstacles)!;
        object? Invoke(string name, params object?[] args) => endpoints["BossMod." + name].DynamicInvoke(args);
        bool Result(string name, params object?[] args) => (bool)Invoke(name, args)!;
        check(registrations["BossMod.Configuration"].GenericTypeArguments[0] == typeof(IReadOnlyList<string>), "配置 IPC 使用原版只读列表签名");
        check(Invoke("Configuration", Array.Empty<string>(), false) is List<string> { Count: > 0 }, "配置 IPC 接受数组调用方");
        var moduleName = typeof(EntryProbe).ToString();
        check(Result("Configuration.DisableModule", moduleName, true), "IPC 禁止指定模块");
        check(!Result("Configuration.DisableModule", moduleName, true), "IPC 重复禁用返回未变化");
        var boss = bossmods.WorldState.Actors.Find(20)!;
        check(BossModuleRegistry.CreateModuleForActor(bossmods.WorldState, boss, BossModuleInfo.Maturity.WIP) == null, "禁用实际阻止模块创建");
        check(Result("Configuration.DisableModule", moduleName, false), "IPC 恢复指定模块");
        using (var restored = BossModuleRegistry.CreateModuleForActor(bossmods.WorldState, boss, BossModuleInfo.Maturity.WIP))
            check(restored is EntryProbe, "恢复实际允许模块创建");
        if (autoDutyDirectory != null)
        {
            foreach (var file in new[] { "AutoDuty.json", "AutoDuty_Passive.json" })
            {
                var source = File.ReadAllText(Path.Combine(autoDutyDirectory, file), System.Text.Encoding.UTF8);
                using var original = JsonDocument.Parse(source);
                var name = original.RootElement.GetProperty("Name").GetString()!;
                check(Result("Presets.Create", source, true), "导入官方 AutoDuty 预设 " + name);
                var actual = manager.Database.Presets.FindPresetByName(name)!;
                var modules = original.RootElement.GetProperty("Modules");
                check(actual.Modules.Count == modules.EnumerateObject().Count(), "官方预设模块完整 " + name);
                foreach (var mod in modules.EnumerateObject())
                {
                    var imported = actual.Modules.Find(m => m.Type.FullName == mod.Name);
                    check(imported != null && imported.SerializedSettings.Count == mod.Value.GetArrayLength(), "官方预设策略完整 " + mod.Name);
                }
                check(Result("Presets.SetActive", name) && manager.Presets.Count == 1, "AutoDuty 显式选择独占预设");
                foreach (var value in new[] { "None", "Pathfind" })
                    check(Result("Presets.AddTransientStrategy", name, typeof(NormalMovement).FullName, "Destination", value), "AutoDuty 路径/战斗移动切换 " + value);
                check(Result("Presets.ClearTransientPresetStrategies", name), "清理 AutoDuty 临时策略");
                manager.Clear();
                check(Result("Presets.Delete", name), "清理官方测试预设");
            }
        }
        var preset = new Preset("AutoDuty 离线夹具");
        var index = preset.AddModule(typeof(NormalMovement), NormalMovement.Definition(), (m, a) => new NormalMovement(m, a));
        preset.Modules[index].SerializedSettings.Add(new(0, (int)NormalMovement.Track.Destination, new StrategyValueTrack { Option = (int)NormalMovement.DestinationStrategy.Explicit, Target = StrategyTarget.PointAbsolute, Offset1 = 10 }));
        var json = JsonSerializer.Serialize(preset, Serialization.BuildSerializationOptions());
        check(Result("Presets.Create", json, true), "IPC 创建外部预设");
        check(Invoke("Presets.Get", preset.Name) is string, "IPC 查询预设");
        check(Result("Presets.SetActive", preset.Name), "IPC 显式选择预设");
        hints.Clear(); manager.Update(0, false, false);
        check((string?)Invoke("Presets.GetActive") == preset.Name && hints.ForcedMovement is { X: > 0 }, "IPC 选择后实际移动");
        check(Result("AI.IsNavigating") && Invoke("AI.NaviTargetPos") is System.Numerics.Vector3 { X: 10, Z: 0 }, "外部查询原版移动目标");
        var second = new Preset("第二预设"); manager.Database.Presets.UserPresets.Add(second);
        check(Result("Presets.Activate", second.Name) && manager.Presets.Count == 2, "IPC 追加预设不覆盖已有选择");
        check(!Result("Presets.Activate", second.Name), "IPC 重复激活返回未变化");
        check(Result("Presets.Deactivate", second.Name) && manager.Presets.Count == 1 && manager.Presets[0].Name == preset.Name, "IPC 仅停用指定预设");
        check(!Result("Presets.Deactivate", second.Name) && !Result("Presets.Activate", "不存在的预设"), "IPC 非活动或未知预设返回失败");
        manager.Database.Presets.UserPresets.Remove(second);
        check(Result("Presets.AddTransientStrategy", preset.Name, typeof(NormalMovement).FullName, "Destination", "None"), "IPC 临时关闭移动");
        hints.Clear(); manager.Update(0, false, false);
        check(manager.MovementModuleActive && hints.ForcedMovement == null, "IPC 临时停止不会回退");
        check(!Result("AI.IsNavigating") && Invoke("AI.NaviTargetPos") == null, "停止时外部导航状态一致");
        check(Result("Presets.ClearTransientStrategy", preset.Name, typeof(NormalMovement).FullName, "Destination"), "IPC 清理临时覆盖");
        hints.Clear(); manager.Update(0, false, false);
        check(hints.ForcedMovement is { X: > 0 }, "IPC 恢复原有移动策略");
        Invoke("AI.PauseMovement", true); hints.Clear(); manager.Update(0, false, false);
        check(hints.ForcedMovement == null, "外部暂停约束原版移动");
        Invoke("AI.PauseMovement", false);
        check(Result("Presets.SetForceDisabled") && Result("Presets.GetForceDisabled"), "IPC 总停止");
        hints.Clear(); manager.Update(0, false, false);
        ai.GetType().GetMethod("Update")!.Invoke(ai, null);
        check(Result("Presets.GetForceDisabled"), "旧 AI 不解除外部总停止");
        check(Result("Presets.ClearActive") && !Result("Presets.GetForceDisabled"), "IPC 清理停止状态");
        check(!Result("Presets.SetActive", "不存在的预设"), "不存在的预设返回失败");
        check(Result("Presets.SetActiveList", new List<string> { preset.Name }), "IPC 多预设入口保留");
        check(((List<string>)Invoke("Presets.GetActiveList")!).SequenceEqual(new[] { preset.Name }), "IPC 多预设查询");
        check(!Result("Presets.SetActiveList", new List<string> { "不存在的预设" }) && manager.Presets[0].Name == preset.Name, "IPC 无效列表不破坏原选择");
        manager.Clear();
        check(Result("Presets.Delete", preset.Name), "IPC 删除测试预设");
        Console.WriteLine($"IPC：实际注册 {registrations.Count} 个接口；外部预设、临时策略、移动暂停及启停流程通过。");
    }
}

public class IpcProxy : DispatchProxy
{
    public Func<MethodInfo, object?[]?, object?> Handler = null!;
    public static object Create(Type type, Func<MethodInfo, object?[]?, object?> handler)
    {
        var result = DispatchProxy.Create(type, typeof(IpcProxy));
        ((IpcProxy)result).Handler = handler;
        return result;
    }
    protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!, args);
}
