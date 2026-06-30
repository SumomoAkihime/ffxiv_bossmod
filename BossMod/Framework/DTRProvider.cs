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
    private readonly IDtrBarEntry _autorotationEntry = Service.DtrBar.Get("vbm-autorotation");
    private readonly IDtrBarEntry _aiEntry = Service.DtrBar.Get("vbm-ai");
    private static readonly AIConfig _aiConfig = Service.Config.Get<AIConfig>();
    private bool _wantOpenPopup;

    public DTRProvider(RotationModuleManager manager, AIManager ai)
    {
        _mgr = manager;
        _ai = ai;

        _autorotationEntry.OnClick = _ => _wantOpenPopup = true;
        _aiEntry.Tooltip = Loc.Tr("Left Click => Toggle Enabled");

        _aiEntry.OnClick = _ =>
        {
            if (_ai.ToggleConfig())
                _aiConfig.Modified.Fire();
        };
    }

    public void Dispose()
    {
        _autorotationEntry.Remove();
        _aiEntry.Remove();
    }

    public void Update()
    {
        var show = _mgr.Config.ShowDTR != AutorotationConfig.DtrStatus.None;
        _autorotationEntry.Shown = show;
        if (show)
        {
            var (icon, name) = _mgr.Presets.Count == 0 ? (BitmapFontIcon.SwordSheathed, Loc.Tr("Idle")) : _mgr.IsForceDisabled ? (BitmapFontIcon.SwordSheathed, Loc.Tr("Disabled")) : (BitmapFontIcon.SwordUnsheathed, _mgr.PresetNames);
            Payload prefix = _mgr.Config.ShowDTR == AutorotationConfig.DtrStatus.TextOnly ? new TextPayload("vbm: ") : new IconPayload(icon);
            _autorotationEntry.Text = new SeString(prefix, new TextPayload(name));
        }

        var show2 = _aiConfig.ShowDTR;
        _aiEntry.Shown = show2;
        var beh = _ai.Beh;
        if (show2)
        {
            _aiEntry.Text = Loc.Tr("AI: {0}", beh == null ? Loc.Tr("Off") : Loc.Tr("On"));
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
            {
                ImGui.CloseCurrentPopup();
            }
        }
    }
}
