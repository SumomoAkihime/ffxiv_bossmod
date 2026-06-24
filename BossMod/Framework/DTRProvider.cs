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
    private readonly AIManager _ai;
    private readonly IDtrBarEntry? _autorotationEntry;
    private readonly IDtrBarEntry? _aiEntry;
    private readonly AIConfig _aiConfig = Service.Config.Get<AIConfig>();
    private bool _wantOpenPopup;

    public DTRProvider(RotationModuleManager manager, AIManager ai)
    {
        _mgr = manager;
        _ai = ai;
        _autorotationEntry = SafeGetEntry("vbm-autorotation");
        _aiEntry = SafeGetEntry("vbm-ai");

        _autorotationEntry?.OnClick = _ => _wantOpenPopup = true;
        if (_aiEntry == null)
            return;

        _aiEntry.Tooltip = "Left Click => Toggle Enabled, Right Click => Toggle DrawUI";
        _aiEntry.OnClick = ev =>
        {
            if (ev.ClickType == MouseClickType.Right)
            {
                _aiConfig.DrawUI ^= true;
            }
            else if (_ai.Beh == null)
            {
                _ai.SwitchToFollow(_aiConfig.FollowSlot);
            }
            else
            {
                _ai.SwitchToIdle();
            }
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
    }

    public void Update()
    {
        var show = RotationModuleManager.Config.ShowDTR != AutorotationConfig.DtrStatus.None;
        if (_autorotationEntry != null)
        {
            _autorotationEntry.Shown = show;
            if (show)
            {
                var (icon, name) = _mgr.Presets.Count == 0
                    ? (BitmapFontIcon.SwordSheathed, "Idle")
                    : _mgr.IsForceDisabled
                        ? (BitmapFontIcon.SwordSheathed, "Disabled")
                        : (BitmapFontIcon.SwordUnsheathed, _mgr.PresetNames);
                Payload prefix = RotationModuleManager.Config.ShowDTR == AutorotationConfig.DtrStatus.TextOnly ? new TextPayload("vbm: ") : new IconPayload(icon);
                _autorotationEntry.Text = new SeString(prefix, new TextPayload(name));
            }
        }

        if (_aiEntry != null)
        {
            _aiEntry.Shown = _aiConfig.ShowDTR;
            if (_aiConfig.ShowDTR)
                _aiEntry.Text = "AI: " + (_ai.Beh == null ? "Off" : "On");
        }

        if (_wantOpenPopup && _mgr.Player != null)
        {
            ImGui.OpenPopup("vbm_dtr_menu");
            _wantOpenPopup = false;
        }

        using var popup = ImRaii.Popup("vbm_dtr_menu");
        if (popup)
        {
            if (UIRotationWindow.DrawRotationSelector(_mgr))
                ImGui.CloseCurrentPopup();
        }
    }
}
