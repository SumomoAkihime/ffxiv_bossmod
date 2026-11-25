using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using RebornMaterial;

namespace BossMod.RebornUI;

internal sealed partial class SettingsWindow
{
    private static readonly (string, string)[] _availableAICommands =
    [
        ( "on", Loc.Tr("Enables the AI.") ),
        ( "off", Loc.Tr("Disables the AI.") ),
        ( "toggle", Loc.Tr("Toggles the AI on/off.") ),
        ( "targetmaster", Loc.Tr("Toggles the focus on target leader.") ),
        ( "follow slotX", Loc.Tr("Follows the specified slot, eg. Slot1.") ),
        ( "follow name", Loc.Tr("Follows the specified party member by name.") ),
        ( "ui", Loc.Tr("Toggles the AI menu.") ),
        ( "forbidactions", Loc.Tr("Toggles the forbidding of actions. (only for autorotation)") ),
        ( "forbidactions on/off", Loc.Tr("Sets forbid actions to on or off. (only for autorotation)") ),
        ( "forbidmovement", Loc.Tr("Toggles the forbidding of movement.") ),
        ( "forbidmovement on/off", Loc.Tr("Sets forbid movement to on or off.") ),
        ( "idlewhilemounted", Loc.Tr("Toggles the idling while mounted.") ),
        ( "idlewhilemounted on/off", Loc.Tr("Sets idle while mounted to on or off.") ),
        ( "followcombat", Loc.Tr("Toggles following during combat.") ),
        ( "followcombat on/off", Loc.Tr("Sets following during combat to on or off.") ),
        ( "followmodule", Loc.Tr("Toggles following during active boss module.") ),
        ( "followmodule on/off", Loc.Tr("Sets following during active boss module to on or off.") ),
        ( "followoutofcombat", Loc.Tr("Toggles following during out of combat.") ),
        ( "followoutofcombat on/off", Loc.Tr("Sets following target out of combat to on or off.") ),
        ( "followtarget", Loc.Tr("Toggles following targets during combat.") ),
        ( "followtarget on/off", Loc.Tr("Sets following target during combat to on or off.") ),
        ( "positional X", Loc.Tr("Switch to positional when following targets. (any, rear, flank, front)") ),
        ( "maxdistancetarget X", Loc.Tr("Sets max distance to target. (default = 2.6)") ),
        ( "maxdistanceslot X", Loc.Tr("Sets max distance to slot. (default = 1)") ),
        ( "mindistance X", Loc.Tr("Sets min distance to hitbox. (default = 0)") ),
        ( "prefdistance X", Loc.Tr("Sets preferred distance to forbidden zones. (default = 0)") ),
        ( "movedelay X", Loc.Tr("Sets AI movement decision delay. (default = 0)") ),
        ( "obstaclemaps", Loc.Tr("Toggles loading obstacle maps.") ),
        ( "obstaclemaps on/off", Loc.Tr("Sets the loading of obstacle maps to on or off.") ),
    ];

    private static readonly (string, string)[] _autorotationCommands =
    [
        ( "ar clear", Loc.Tr("Clear all active presets; use the encounter plan when available") ),
        ( "ar disable", Loc.Tr("Force disable autorotation; no actions will be executed automatically even if plan is active.") ),
        ( "ar set Preset", Loc.Tr("Start executing specified preset.") ),
        ( "ar toggle", Loc.Tr("Force disable autorotation if not already; otherwise clear overrides.") ),
        ( "ar toggle Preset", Loc.Tr("Toggle the specified preset while preserving the other active presets") ),
        ( "ar ui", Loc.Tr("Toggle autorotation ui.") ),
    ];

    private static readonly (string, string)[] _availableOtherCommands =
    [
        ( "", Loc.Tr("Opens this window.") ),
        ( "restorerotation", Loc.Tr("Toggle restore character orientation after action use setting.") ),
        ( "resetcolors", Loc.Tr("Resets all colors to their default values.") ),
        ( "d", Loc.Tr("Opens the debug menu.") ),
        ( "r", Loc.Tr("Opens the replay menu.") ),
        ( "r on/off", Loc.Tr("Starts/stops recording a replay.") ),
        ( "gc", Loc.Tr("Triggers the garbage collection.") ),
        ( "radar", Loc.Tr("Toggles radar display.") ),
        ( "radar on/off", Loc.Tr("Sets radar display to on or off.") ),
        ( "radar reset", Loc.Tr("Moves the radar window back to the middle of the screen.") ),
        ( "cfg", Loc.Tr("Lists all configs; /bmr cfg <config> <field> <value> changes a setting.") )
    ];

    private static void DrawCommands()
    {
        DrawCommandCard("cmd_general", Loc.Tr("General"), FontAwesomeIcon.Terminal, "/bmr", _availableOtherCommands, 0);
        DrawCommandCard("cmd_ai", Loc.Tr("AI"), FontAwesomeIcon.Robot, "/bmrai", _availableAICommands, 1);
        DrawCommandCard("cmd_ar", Loc.Tr("Autorotation"), FontAwesomeIcon.Bolt, "/bmr", _autorotationCommands, 2);
    }

    private static void DrawCommandCard(string id, string title, FontAwesomeIcon icon, string prefix, (string, string)[] commands, int index)
    {
        using var card = M3Card.Begin(id, title, icon, M3.AccentFor(index), subtitle: Loc.Tr("Start with {0}", prefix));
        using var scope = ImRaii.PushId(id);
        var button = M3Widgets.IconButtonSize;
        foreach (var (args, description) in commands)
        {
            var command = args.Length > 0 ? $"{prefix} {args}" : prefix;
            var row = M3SettingRow.Begin(command, Loc.Tr(description), new(button, button), M3.Scheme.Primary);
            ImGui.SetCursorScreenPos(row.ControlPosition);
            _ = M3Widgets.CopyIconButton($"##{command}", command, snackbar: Loc.Tr("Copied {0}", command));
            M3SettingRow.End(row);
        }
    }
}
