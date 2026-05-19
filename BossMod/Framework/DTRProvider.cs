using BossMod.AI;
using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace BossMod;

internal sealed class DTRProvider : IDisposable
{
    private readonly RotationModuleManager _mgr;
    private readonly AIManager _ai;
    private readonly IDtrBarEntry? _autorotationEntry;
    private readonly IDtrBarEntry? _aiEntry;
    private readonly IDtrBarEntry? _statsEntry;
    private readonly AIConfig _aiConfig = Service.Config.Get<AIConfig>();
    private bool _wantOpenPopup;

    public unsafe DTRProvider(RotationModuleManager manager, AIManager ai)
    {
        _mgr = manager;
        _ai = ai;
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
        try
        {
            return Service.DtrBar.Get(title);
        }
        catch (Exception ex)
        {
            Service.Logger.Warning(ex, $"DTR entry '{title}' unavailable, skipping.");
            return null;
        }
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
            _aiEntry.Text = "AI: " + (_ai.Behaviour == null ? "Off" : "On");
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
