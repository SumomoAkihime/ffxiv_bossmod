using BossMod.AI;
using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

internal sealed class DTRProvider : IDisposable
{
    private readonly RotationModuleManager _mgr;
    private readonly IDtrBarEntry? _autorotationEntry;
    private readonly IDtrBarEntry? _aiEntry;
    private readonly IDtrBarEntry? _statsEntry;
    private readonly AIConfig _aiConfig = Service.Config.Get<AIConfig>();
    private bool _wantOpenPopup;

    public DTRProvider(RotationModuleManager manager)
    {
        _mgr = manager;
        _autorotationEntry = SafeGetEntry("vbm-autorotation");
        _aiEntry = SafeGetEntry("vbm-ai");
        _statsEntry = SafeGetEntry("vbm-stats");

        _autorotationEntry?.OnClick = _ => _wantOpenPopup = true;
        if (_aiEntry == null)
            return;
        _aiEntry.Tooltip = "Left Click => Toggle Enabled, Right Click => Toggle DrawUI";

        _aiEntry.OnClick = ev =>
        {
            if (ev.ClickType == MouseClickType.Right)
                _aiConfig.DrawUI ^= true;
            else
                _aiConfig.Enabled ^= true;
            _aiConfig.Modified.Fire();
        };
    }

    private static IDtrBarEntry? SafeGetEntry(string title)
    {
        for (var i = 0; i < 8; ++i)
        {
            var candidate = i == 0 ? title : $"{title}-{i}";
            try
            {
                return Service.DtrBar.Get(candidate);
            }
            catch (ArgumentException)
            {
                // Duplicate title is common when previous plugin instance leaked DTR entries.
                // Try a suffixed key so plugin still loads and displays status.
            }
            catch (Exception ex)
            {
                Service.Logger.Warning(ex, $"DTR entry '{candidate}' unavailable, skipping.");
                return null;
            }
        }
        Service.Logger.Warning($"DTR entry '{title}' unavailable after retries, skipping.");
        return null;
    }

    public void Dispose()
    {
        _autorotationEntry?.Remove();
        _aiEntry?.Remove();
        _statsEntry?.Remove();
    }

    public void Update()
    {
        _autorotationEntry?.Shown = _mgr.Config.ShowDTR != AutorotationConfig.DtrStatus.None;
        var (icon, name) = _mgr.Presets.Count == 0 ? (BitmapFontIcon.SwordSheathed, "Idle") : _mgr.IsForceDisabled ? (BitmapFontIcon.SwordSheathed, "Disabled") : (BitmapFontIcon.SwordUnsheathed, string.Join(", ", _mgr.PresetNames));
        Payload prefix = _mgr.Config.ShowDTR == AutorotationConfig.DtrStatus.TextOnly ? new TextPayload("vbm: ") : new IconPayload(icon);
        _autorotationEntry?.Text = new SeString(prefix, new TextPayload(name));

        if (_aiEntry != null)
        {
            _aiEntry.Shown = _aiConfig.ShowDTR;
            _aiEntry.Text = "AI: " + (_aiConfig.Enabled ? "On" : "Off");
        }

        if (_statsEntry != null)
        {
            _statsEntry.Shown = _mgr.Config.ShowStatsDTR;
            _statsEntry.Text = _mgr.LastPathfindMs > 0
                ? $"Pathfind: {_mgr.LastRasterizeMs:f1}ms (r) {_mgr.LastPathfindMs:f1}ms (p)"
                : $"Pathfind: -";
        }

        if (_wantOpenPopup && _mgr.Player != null)
        {
            ImGui.OpenPopup("vbm_dtr_menu");
            _wantOpenPopup = false;
        }

        using var popup = ImRaii.Popup("vbm_dtr_menu");
        if (popup)
            if (UIRotationWindow.DrawRotationSelector(_mgr))
                ImGui.CloseCurrentPopup();
    }
}
