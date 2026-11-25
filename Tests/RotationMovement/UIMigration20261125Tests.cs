using System.Collections;
using System.Reflection;
using System.Text.Json;
using BossMod;
using BossMod.Autorotation;
using BossMod.RebornUI;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

static class UIMigration20261125Tests
{
    const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
    static Type Internal(string name) => typeof(InterfaceConfig).Assembly.GetType("BossMod.RebornUI." + name, true)!;
    static object? Field(object value, string name) => value.GetType().GetField(name, All)!.GetValue(value);

    public static void Run(RotationModuleManager manager, Action<bool, string> check)
    {
        Configuration(check);
        RotationState(manager, check);
        WindowOwnership(check);
        SettingsAndLocalization(manager.WorldState, check);
        InitializationFailure(manager, check);
    }

    static void Configuration(Action<bool, string> check)
    {
        var config = new InterfaceConfig();
        check(config.TextScale == 1 && config.ElementScale == 1 && config.PaddingScale == 1 && config.MotionScale == 1, "界面配置的字号、控件、间距与动画采用上游默认值");
        check(!config.CompactSettings && config.InlineDescriptions && !config.IconSidebar, "界面默认布局、说明与侧栏设置正确");
        config.TextScale = 1.25f;
        config.ElementScale = 0.9f;
        config.MotionScale = 0;
        config.IconSidebar = true;
        var options = Serialization.BuildSerializationOptions();
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
            config.Serialize(writer, options);
        using var data = JsonDocument.Parse(stream.ToArray());
        var restored = new InterfaceConfig();
        restored.Deserialize(data.RootElement, options);
        check(restored.TextScale == 1.25f && restored.ElementScale == 0.9f && restored.MotionScale == 0 && restored.IconSidebar, "新增界面设置经本地ConfigNode序列化完整往返");
        check(!data.RootElement.TryGetProperty("UiAccentColor", out _), "M3接口属性复用已有配置字段，避免生成重复序列化键");
        using var old = JsonDocument.Parse("{\"HideDefaultPresets\":true,\"ShowUI\":true}");
        var autorotation = new AutorotationConfig();
        autorotation.Deserialize(old.RootElement, options);
        check(autorotation.HideDefaultPreset && autorotation.ShowUI, "旧预设隐藏配置继续通过本地迁移入口读取");
    }

    static void RotationState(RotationModuleManager manager, Action<bool, string> check)
    {
        var saved = manager.Presets.ToArray();
        var status = Internal("MaterialUI").GetMethod("RotationStatusText", All)!;
        string Read() => (string)status.Invoke(null, [manager])!;
        try
        {
            manager.Clear();
            check(Read() == Loc.Tr("No preset active"), "新界面正确显示无预设状态");
            var first = new Preset("职业预设");
            var second = new Preset("移动预设");
            manager.Activate(first);
            manager.Activate(second);
            check(Read() == Loc.Tr("Running {0}", "职业预设, 移动预设"), "新界面完整显示多个已选预设及选择顺序");
            manager.Toggle(first);
            check(manager.Presets.SequenceEqual([second]) && Read().Contains("移动预设"), "切换一个预设保留其他已选预设");
            manager.SetForceDisabled();
            check(Read() == Loc.Tr("Force-disabled") && manager.IsForceDisabled, "新界面正确显示总停止状态");
            check(manager.Presets.Count == 1, "状态读取保持总停止预设集合");
        }
        finally
        {
            manager.Clear();
            foreach (var preset in saved)
                manager.Activate(preset);
        }
    }

    static void WindowOwnership(Action<bool, string> check)
    {
        var system = Service.WindowSystem!;
        using var inner = new ProbeWindow();
        var wrap = Internal("FoldHost").GetMethod("Wrap", All)!.MakeGenericMethod(typeof(ProbeWindow));
        var actions = Array.CreateInstance(wrap.GetParameters()[5].ParameterType.GetElementType()!, 0);
        var host = (UIWindow)wrap.Invoke(null, ["UI test", new System.Numerics.Vector2(400, 300), new WindowSizeConstraints(), null, null, actions])!;
        try
        {
            check(!system.Windows.Contains(inner) && system.Windows.Contains(host), "折叠宿主接管窗口注册且保留原窗口实例");
            host.OnOpen();
            host.PreOpenCheck();
            check(inner.OpenCount == 1 && inner.PreCount == 1 && host.IsOpen, "折叠宿主转发生命周期与打开状态");
            host.GetType().GetMethod("OnCloseRequested", All)!.Invoke(host, null);
            host.OnClose();
            check(!inner.IsOpen && inner.CloseCount == 1, "关闭宿主只向原窗口发送一次关闭回调");
            Internal("MaterialUI").GetMethod("Stop", All)!.Invoke(null, null);
            check(system.Windows.Contains(inner) && !system.Windows.Contains(host), "清理部分界面资源后恢复原窗口注册");
            host.Dispose();
            check(system.Windows.Count(window => ReferenceEquals(window, inner)) == 1, "重复清理保持原窗口唯一注册");
        }
        finally { host.Dispose(); }
    }

    static void SettingsAndLocalization(WorldState world, Action<bool, string> check)
    {
        var active = typeof(Loc).GetProperty(nameof(Loc.Active))!;
        var strings = typeof(Loc).GetField("_strings", All)!;
        var oldActive = Loc.Active;
        var oldStrings = strings.GetValue(null);
        var renderer = Internal("SettingsRenderer");
        var views = (IDictionary)renderer.GetField("_views", All)!.GetValue(null)!;
        var enumLabels = (IDictionary)renderer.GetField("_enumLabels", All)!.GetValue(null)!;
        using var resource = typeof(InterfaceConfig).Assembly.GetManifestResourceStream("BossMod.Localization.zh-CN.json")!;
        var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(resource)!;
        try
        {
            active.SetValue(null, true);
            strings.SetValue(null, translations);
            views.Clear();
            enumLabels.Clear();
            var viewMethod = renderer.GetMethod("View", All)!;
            foreach (var node in Service.Config.Nodes)
            {
                var view = viewMethod.Invoke(null, [node.GetType()])!;
                var fields = (Array)Field(view, "Fields")!;
                var expected = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).Count(field => field.IsDefined(typeof(PropertyDisplayAttribute)));
                check(fields.Length == expected, "新设置视图覆盖本地配置的全部可见字段：" + node.GetType().Name);
                foreach (var field in fields)
                {
                    var meta = Field(field!, "Meta")!;
                    var source = node.GetType().GetField((string)Field(meta, "Name")!)!;
                    check((bool)Field(meta, "Radio")! == source.IsDefined(typeof(PropertyRadioAttribute)), "单选元数据与原字段特性一致：" + source.Name);
                }
            }
            var interfaceView = viewMethod.Invoke(null, [typeof(InterfaceConfig)])!;
            check(((IEnumerable)Field(interfaceView, "Fields")!).Cast<object>().Any(field => (string)Field(field, "Label")! == "主题色"), "界面设置标签使用中文资源");
            var options = (string[])Internal("SettingsLayout").GetField("_maturityOptions", All)!.GetValue(null)!;
            check(options.Length == Enum.GetValues<BossModuleInfo.Maturity>().Length, "完成度选项保持本地枚举顺序与数量");
            var type = Internal("SettingsWindow");
            using var window = (UIWindow)Activator.CreateInstance(type, All, null, [Service.Config, world, null, null], null)!;
            var page = type.GetMethod("PageForClassicTab", All)!;
            foreach (var (old, current) in new[] { ("About", "home"), ("Supported fights", "fights"), ("Autorotation presets", "presets"), ("Slash commands", "commands"), ("InterfaceConfig", "InterfaceConfig") })
                check((string)page.Invoke(null, [old])! == current, "旧页面入口映射保持可用：" + old);
            type.GetField("_searchText", All)!.SetValue(window, "主题色");
            type.GetMethod("UpdateSearch", All)!.Invoke(window, null);
            check(((ICollection)type.GetField("_searchResults", All)!.GetValue(window)!).Count > 0, "中文关键词可以搜索到界面配置");
        }
        finally
        {
            active.SetValue(null, oldActive);
            strings.SetValue(null, oldStrings);
            views.Clear();
            enumLabels.Clear();
        }
    }

    static void InitializationFailure(RotationModuleManager manager, Action<bool, string> check)
    {
        var logger = typeof(Service).GetProperty(nameof(Service.Logger))!;
        var saved = logger.GetValue(null);
        var errors = 0;
        logger.SetValue(null, IpcProxy.Create(logger.PropertyType, (method, _) =>
        {
            if (method.Name == "Error")
                ++errors;
            return null;
        }));
        var before = Service.WindowSystem!.Windows.ToArray();
        try
        {
            Internal("MaterialUI").GetMethod("Start", All)!.Invoke(null, [null, manager.WorldState, manager.Database, manager, null, new DirectoryInfo("temp")]);
            check(errors > 0 && Service.WindowSystem.Windows.SequenceEqual(before), "初始化失败记录错误并清理已注册界面资源");
            check(!(bool)Internal("MaterialUI").GetMethod("OpenSettings", All)!.Invoke(null, [""])!, "初始化失败后由既有设置入口承接");
            Internal("MaterialUI").GetMethod("Stop", All)!.Invoke(null, null);
            check(!RebornMaterial.M3.IsInitialized, "失败后的重复清理释放M3状态");
        }
        finally { logger.SetValue(null, saved); }
    }

    sealed class ProbeWindow() : UIWindow("ui-migration-probe", false, new(400, 300))
    {
        public int OpenCount;
        public int CloseCount;
        public int PreCount;
        public override void Draw() { }
        public override void OnOpen() => ++OpenCount;
        public override void OnClose() => ++CloseCount;
        public override void PreOpenCheck() { ++PreCount; IsOpen = true; }
    }
}
