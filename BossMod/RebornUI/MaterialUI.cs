using BossMod.AI;
using BossMod.Autorotation;
using Dalamud.Interface;
using Dalamud.Plugin;
using RebornMaterial;
using System.IO;

namespace BossMod.RebornUI;

internal static class MaterialUI
{
    private static IDalamudPluginInterface? _dalamud;
    private static SettingsWindow? _settings;
    private static readonly List<FoldHost> _folds = [];
    private static bool _m3Attempted;

    public static void Start(IDalamudPluginInterface dalamud, WorldState ws, RotationDatabase rotationDB, RotationModuleManager rotation, AIManager ai, DirectoryInfo replayDir)
    {
        Stop();
        try
        {
            StartCore(dalamud, ws, rotationDB, rotation, ai, replayDir);
        }
        catch (Exception ex)
        {
            Service.Logger.Error(ex, "[RebornUI] Failed to start; using the classic interface instead");
            Stop();
        }
    }

    private static void StartCore(IDalamudPluginInterface dalamud, WorldState ws, RotationDatabase rotationDB, RotationModuleManager rotation, AIManager ai, DirectoryInfo replayDir)
    {
        M3.ElementBaseline = 0.75f;
        _m3Attempted = true;
        M3.Initialize(dalamud, Service.Config.Get<InterfaceConfig>());
        _dalamud = dalamud;

        _settings = new(Service.Config, ws, replayDir, rotationDB);

        var rotationHost = FoldHost.Wrap<UIRotationWindow>(Loc.Tr("Autorotation"), new(400f, 400f), new() { MinimumSize = new(260f, 140f), MaximumSize = new(5000f, 5000f) },
            () => RotationStatus(rotation), [Loc.Tr("Edit autorotation presets")]);
        if (rotationHost != null)
        {
            _folds.Add(rotationHost);
        }

        var aiHost = FoldHost.Wrap<AIManagementWindow>(Loc.Tr("Automovement"), new(560f, 340f), new() { MinimumSize = new(300f, 160f), MaximumSize = new(5000f, 5000f) },
            () => AIStatus(ai), null, new FoldHost.HostAction(FontAwesomeIcon.Cog, Loc.Tr("Open the automovement settings"), () => OpenSettings(nameof(AIConfig))));
        if (aiHost != null)
        {
            _folds.Add(aiHost);
        }
    }

    public static bool OpenSettings(string page = "")
    {
        if (_settings == null)
        {
            return false;
        }

        _settings.Open(SettingsWindow.PageForClassicTab(page));
        return true;
    }

    public static bool OpenEncounterSettings(BossModuleRegistry.Info info, WorldState ws)
    {
        if (_settings == null)
        {
            return false;
        }

        _ = new EncounterSettingsWindow(info, ws);
        return true;
    }

    public static void Stop()
    {
        var windows = _folds.Cast<RebornWindow>().Concat(Service.WindowSystem?.Windows.OfType<RebornWindow>() ?? []).Distinct().ToArray();
        foreach (var window in windows)
            Try(window.Dispose);
        _folds.Clear();
        _settings = null;
        if (_m3Attempted)
        {
            _m3Attempted = false;
            Try(static () => M3.Dispose());
        }
        _dalamud = null;
    }

    private static void Try(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Service.Logger.Error(ex, "[RebornUI] Cleanup failed");
        }
    }

    public static void BeginFrame()
    {
        if (_dalamud == null)
        {
            return;
        }

        try
        {
            M3.BeginFrame();
        }
        catch (Exception ex)
        {
            Stop();
            Service.Logger.Error(ex, "[RebornUI] Per-frame update failed; restored the classic interface");
        }
    }

    internal static string RotationStatusText(RotationModuleManager rotation)
        => rotation.IsForceDisabled ? Loc.Tr("Force-disabled")
            : rotation.Presets.Count > 0 ? Loc.Tr("Running {0}", rotation.PresetNames)
            : Loc.Tr("No preset active");

    private static (string, Vector4?) RotationStatus(RotationModuleManager rotation)
        => (RotationStatusText(rotation), rotation.IsForceDisabled ? M3.Scheme.Error
            : rotation.Presets.Count > 0 ? M3.Severity(M3Severity.Success) : null);

    private static (string, Vector4?) AIStatus(AIManager ai)
    {
        if (ai.Beh == null)
        {
            return (Loc.Tr("Off"), null);
        }

        var master = ai.WorldState.Party[ai.MasterSlot];
        return (master != null ? Loc.Tr("On, following {0} (slot {1})", master.Name, ai.MasterSlot + 1) : Loc.Tr("On"), M3.Severity(M3Severity.Success));
    }
}
