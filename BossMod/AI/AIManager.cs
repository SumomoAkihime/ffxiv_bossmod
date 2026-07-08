using BossMod.Autorotation;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.Game.Group;

namespace BossMod.AI;

sealed class AIManager : IDisposable
{
    private const string VBMAIPresetName = "VBM AI";

    public static AIManager? Instance;
    public readonly RotationModuleManager Autorot;
    public readonly AIController Controller;
    private static readonly AIConfig _config = Service.Config.Get<AIConfig>();
    private readonly AIManagementWindow _wndAI;
    public int MasterSlot = PartyState.PlayerSlot; // non-zero means corresponding player is master
    public AIBehaviour? Beh;
    private bool _autoStartedByVBMAI;

    public WorldState WorldState => Autorot.Bossmods.WorldState;
    public float ForceMovementIn => Beh?.ForceMovementIn ?? float.MaxValue;
    public string GetAIPreset => string.Empty;

    public AIManager(RotationModuleManager autorot, ActionManagerEx amex, MovementOverride movement)
    {
        Instance = this;
        _wndAI = new AIManagementWindow(this);
        Autorot = autorot;
        Controller = new(autorot.WorldState, amex, movement);
        Service.CommandManager.AddHandler("/bmrai", new Dalamud.Game.Command.CommandInfo(OnCommand) { HelpMessage = "Toggle AI mode" });
    }

    public void SetAIPreset(Preset? _) { }

    public void Dispose()
    {
        SwitchToIdle();
        _wndAI.Dispose();
        Service.CommandManager.RemoveHandler("/bmrai");
        Instance = null;
    }

    public void Update()
    {
        SyncVBMAIAutoMovement();

        if (!WorldState.Party.Members[MasterSlot].IsValid())
        {
            SwitchToIdle();
        }

        var player = WorldState.Party.Player();
        var master = WorldState.Party[MasterSlot];
        if (Beh != null && player != null && master != null && !WorldState.Party.Members[PartyState.PlayerSlot].InCutscene)
        {
            _ = Beh.Execute(player, master);
        }
        else
        {
            Controller.Clear();
        }

        Controller.Update(player, Autorot.Hints, WorldState.CurrentTime);
    }

    public bool SetConfigEnabled(bool enable)
    {
        var modified = _config.Enabled != enable;
        _config.Enabled = enable;
        if (enable)
        {
            SwitchToFollow(_config.FollowSlot);
        }
        else
        {
            SwitchToIdle();
        }
        return modified;
    }

    public bool ToggleConfig()
    {
        SetConfigEnabled(Beh == null);
        return true;
    }

    public void SwitchToIdle()
    {
        Beh?.Dispose();
        Beh = null;
        MasterSlot = PartyState.PlayerSlot;
        if (Autorot.IsForceDisabled)
            Autorot.Clear();
        _autoStartedByVBMAI = false;
        Controller.Clear();
        _wndAI.UpdateTitle();
    }

    public void SwitchToFollow(int masterSlot)
    {
        SwitchToIdle();
        _autoStartedByVBMAI = false;
        MasterSlot = WorldState.Party[masterSlot]?.Name == null ? 0 : masterSlot;
        Beh = new AIBehaviour(Controller, Autorot);
        _wndAI.UpdateTitle();
    }

    private void SyncVBMAIAutoMovement()
    {
        var vbmAIActive = IsVBMAIPresetActive();
        if (vbmAIActive)
        {
            if (Beh == null && !_config.Enabled)
                SwitchToFollowForVBMAI(_config.FollowSlot);
        }
        else if (_autoStartedByVBMAI)
        {
            StopAutoStartedBehaviour();
        }
    }

    private void SwitchToFollowForVBMAI(int masterSlot)
    {
        Beh?.Dispose();
        Beh = null;
        MasterSlot = WorldState.Party[masterSlot]?.Name == null ? 0 : masterSlot;
        _autoStartedByVBMAI = true;
        Beh = new AIBehaviour(Controller, Autorot);
        _wndAI.UpdateTitle();
    }

    private void StopAutoStartedBehaviour()
    {
        Beh?.Dispose();
        Beh = null;
        MasterSlot = PartyState.PlayerSlot;
        _autoStartedByVBMAI = false;
        Controller.Clear();
        _wndAI.UpdateTitle();
    }

    private bool IsVBMAIPresetActive()
    {
        var preset = Autorot.Database.Presets.FindPresetByName(VBMAIPresetName, StringComparison.OrdinalIgnoreCase);
        return preset != null && Autorot.Presets.Contains(preset);
    }

    private unsafe int FindPartyMemberSlotFromSender(SeString sender)
    {
        var sources = sender.Payloads.Count != 0 ? sender.Payloads[0] : null;
        if (sources is not PlayerPayload source)
        {
            return -1;
        }

        var group = GroupManager.Instance()->GetGroup();
        var slot = -1;
        for (var i = 0; i < group->MemberCount; ++i)
        {
            if (group->PartyMembers[i].HomeWorld == source.World.RowId && group->PartyMembers[i].NameString == source.PlayerName)
            {
                slot = i;
                break;
            }
        }
        return slot >= 0 ? Array.FindIndex(WorldState.Party.Members, m => m.ContentId == group->PartyMembers[slot].ContentId) : -1;
    }

    private void OnCommand(string cmd, string message)
    {
        var messageData = message.Split(' ');
        if (messageData.Length == 0)
        {
            return;
        }

        var configModified = false;

        switch (messageData[0].ToUpperInvariant())
        {
            case "ON":
                configModified = EnableConfig(true);
                break;
            case "OFF":
                configModified = EnableConfig(false);
                break;
            case "TOGGLE":
                configModified = ToggleConfig();
                break;
            case "TARGETMASTER":
                configModified = ToggleFocusTargetMaster();
                break;
            case "FOLLOW":
                var cfgFollowSlot = _config.FollowSlot;
                HandleFollowCommand(messageData);
                configModified = cfgFollowSlot != _config.FollowSlot;
                break;
            case "UI":
                configModified = ToggleDebugMenu();
                break;
            case "FORBIDMOVEMENT":
                var cfgForbidMovement = _config.ForbidMovement;
                ToggleForbidMovement(messageData);
                configModified = cfgForbidMovement != _config.ForbidMovement;
                break;
            case "IDLEWHILEMOUNTED":
                var cfgMountedIdle = _config.ForbidAIMovementMounted;
                ToggleIdleWhileMounted(messageData);
                configModified = cfgMountedIdle != _config.ForbidAIMovementMounted;
                break;
            case "FOLLOWOUTOFCOMBAT":
                var cfgFollowOOC = _config.FollowOutOfCombat;
                ToggleFollowOutOfCombat(messageData);
                configModified = cfgFollowOOC != _config.FollowOutOfCombat;
                break;
            case "FOLLOWCOMBAT":
                var cfgFollowIC = _config.FollowDuringCombat;
                ToggleFollowCombat(messageData);
                configModified = cfgFollowIC != _config.FollowDuringCombat;
                break;
            case "FOLLOWMODULE":
                var cfgFollowM = _config.FollowDuringActiveBossModule;
                ToggleFollowModule(messageData);
                configModified = cfgFollowM != _config.FollowDuringActiveBossModule;
                break;
            case "FOLLOWTARGET":
                var cfgFollowT = _config.FollowTarget;
                ToggleFollowTarget(messageData);
                configModified = cfgFollowT != _config.FollowTarget;
                break;
            case "OBSTACLEMAPS":
                var cfgOM = _config.DisableObstacleMaps;
                ToggleObstacleMaps(messageData);
                configModified = cfgOM != _config.DisableObstacleMaps;
                break;

            case "POSITIONAL":
                var cfgPositional = _config.DesiredPositional;
                HandlePositionalCommand(messageData);
                configModified = cfgPositional != _config.DesiredPositional;
                break;
            case "MAXDISTANCETARGET":
                var cfgMDT = _config.MaxDistanceToTarget;
                HandleMaxDistanceTargetCommand(messageData);
                configModified = cfgMDT != _config.MaxDistanceToTarget;
                break;
            case "MAXDISTANCESLOT":
                var cfgMDS = _config.MaxDistanceToSlot;
                HandleMaxDistanceSlotCommand(messageData);
                configModified = cfgMDS != _config.MaxDistanceToSlot;
                break;
            case "MINDISTANCE":
                var cfgMinDT = _config.MinDistance;
                HandleMinDistanceCommand(messageData);
                configModified = cfgMinDT != _config.MinDistance;
                break;
            case "PREFDISTANCE":
                var cfgPrefDS = _config.PreferredDistance;
                HandlePrefDistanceCommand(messageData);
                configModified = cfgPrefDS != _config.PreferredDistance;
                break;
            case "MOVEDELAY":
                var cfgMD = _config.MoveDelay;
                HandleMoveDelayCommand(messageData);
                configModified = cfgMD != _config.MoveDelay;
                break;
            case "SETPRESETNAME":
                ParseAIAutorotationSetCommand();
                break;
            default:
                if (_config.EchoToChat)
                {
                    Service.ChatGui.Print($"[BMRAI] Unknown command: {messageData[0]}");
                }
                return;
        }

        if (configModified)
        {
            _config.Modified.Fire();
        }
    }

    private bool EnableConfig(bool enable)
        => SetConfigEnabled(enable);

    private bool ToggleFocusTargetMaster()
    {
        _config.FocusTargetMaster = !_config.FocusTargetMaster;
        return true;
    }

    private void ToggleObstacleMaps(string[] messageData)
    {
        if (messageData.Length == 1)
        {
            _config.DisableObstacleMaps = !_config.DisableObstacleMaps;
        }
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _config.DisableObstacleMaps = false;
                    break;
                case "OFF":
                    _config.DisableObstacleMaps = true;
                    break;
                default:
                    if (_config.EchoToChat)
                    {
                        Service.ChatGui.Print($"[BMRAI] Unknown obstacle map command: {messageData[1]}");
                    }
                    return;
            }
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] Obstacle maps are now {(_config.DisableObstacleMaps ? "disabled" : "enabled")}");
        }
    }

    private void ToggleIdleWhileMounted(string[] messageData)
    {
        if (messageData.Length == 1)
        {
            _config.ForbidAIMovementMounted = !_config.ForbidAIMovementMounted;
        }
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _config.ForbidAIMovementMounted = true;
                    break;
                case "OFF":
                    _config.ForbidAIMovementMounted = false;
                    break;
                default:
                    if (_config.EchoToChat)
                    {
                        Service.ChatGui.Print($"[BMRAI] Unknown idle while mounted command: {messageData[1]}");
                    }
                    return;
            }
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] Idle while mounted is now {(_config.ForbidAIMovementMounted ? "enabled" : "disabled")}");
        }
    }

    private void HandleFollowCommand(string[] messageData)
    {
        if (messageData.Length < 2 && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] Missing follow target.");
            return;
        }

        if (messageData[1].StartsWith("Slot", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(messageData[1].AsSpan(4), out var slot) && slot >= 1 && slot <= 8)
        {
            SwitchToFollow(slot - 1);
            _config.FollowSlot = slot - 1;
        }
        else
        {
            var sb = new StringBuilder();
            for (var i = 1; i < messageData.Length; ++i)
            {
                if (i > 1)
                {
                    sb.Append(' ');
                }

                sb.Append(messageData[i]);
            }
            var memberIndex = FindPartyMemberByName(sb.ToString());
            if (memberIndex >= 0)
            {
                SwitchToFollow(memberIndex);
                _config.FollowSlot = memberIndex;
            }
            else
                if (_config.EchoToChat)
                {
                    Service.ChatGui.Print($"[BMRAI] Unknown party member: {sb}");
                }
        }
    }

    private bool ToggleDebugMenu()
    {
        _config.DrawUI = !_config.DrawUI;
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] AI menu is now {(_config.DrawUI ? "enabled" : "disabled")}");
        }
        return true;
    }

    private void ToggleForbidMovement(string[] messageData)
    {
        if (messageData.Length == 1)
        {
            _config.ForbidMovement = !_config.ForbidMovement;
        }
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _config.ForbidMovement = true;
                    break;
                case "OFF":
                    _config.ForbidMovement = false;
                    break;
                default:
                    if (_config.EchoToChat)
                    {
                        Service.ChatGui.Print($"[BMRAI] Unknown forbid movement command: {messageData[1]}");
                    }
                    return;
            }
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] Forbid movement is now {(_config.ForbidMovement ? "enabled" : "disabled")}");
        }
    }

    private void ToggleFollowOutOfCombat(string[] messageData)
    {
        if (messageData.Length == 1)
        {
            _config.FollowOutOfCombat = !_config.FollowOutOfCombat;
        }
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _config.FollowOutOfCombat = true;
                    break;
                case "OFF":
                    _config.FollowOutOfCombat = false;
                    break;
                default:
                    if (_config.EchoToChat)
                    {
                        Service.ChatGui.Print($"[BMRAI] Unknown follow out of combat command: {messageData[1]}");
                    }
                    return;
            }
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] Follow out of combat is now {(_config.FollowOutOfCombat ? "enabled" : "disabled")}");
        }
    }

    private void ToggleFollowCombat(string[] messageData)
    {
        if (messageData.Length == 1)
        {
            if (_config.FollowDuringCombat)
            {
                _config.FollowDuringCombat = false;
                _config.FollowDuringActiveBossModule = false;
            }
            else
            {
                _config.FollowDuringCombat = true;
            }
        }
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _config.FollowDuringCombat = true;
                    break;
                case "OFF":
                    _config.FollowDuringCombat = false;
                    _config.FollowDuringActiveBossModule = false;
                    break;
                default:
                    Service.ChatGui.Print($"[BMRAI] Unknown follow during combat command: {messageData[1]}");
                    return;
            }
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] Follow during combat is now {(_config.FollowDuringCombat ? "enabled" : "disabled")}");
            Service.ChatGui.Print($"[BMRAI] Follow during active boss module is now {(_config.FollowDuringActiveBossModule ? "enabled" : "disabled")}");
        }
    }

    private void ToggleFollowModule(string[] messageData)
    {
        if (messageData.Length == 1)
        {
            _config.FollowDuringActiveBossModule = !_config.FollowDuringActiveBossModule;
            if (!_config.FollowDuringCombat)
            {
                _config.FollowDuringCombat = true;
            }
        }
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _config.FollowDuringActiveBossModule = true;
                    _config.FollowDuringCombat = true;
                    break;
                case "OFF":
                    _config.FollowDuringActiveBossModule = false;
                    break;
                default:
                    Service.ChatGui.Print($"[BMRAI] Unknown follow during active boss module command: {messageData[1]}");
                    return;
            }
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] Follow during active boss module is now {(_config.FollowDuringActiveBossModule ? "enabled" : "disabled")}");
            Service.ChatGui.Print($"[BMRAI] Follow during combat is now {(_config.FollowDuringCombat ? "enabled" : "disabled")}");
        }
    }

    private void ToggleFollowTarget(string[] messageData)
    {
        if (messageData.Length == 1)
        {
            _config.FollowTarget = !_config.FollowTarget;
        }
        else
        {
            switch (messageData[1].ToUpperInvariant())
            {
                case "ON":
                    _config.FollowTarget = true;
                    break;
                case "OFF":
                    _config.FollowTarget = false;
                    break;
                default:
                    if (_config.EchoToChat)
                    {
                        Service.ChatGui.Print($"[BMRAI] Unknown follow target command: {messageData[1]}");
                    }
                    return;
            }
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] Following targets is now {(_config.FollowTarget ? "enabled" : "disabled")}");
        }
    }

    private void HandlePositionalCommand(string[] messageData)
    {
        if (messageData.Length < 2 && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] Missing positional type.");
            return;
        }

        var msg = messageData[1];
        switch (msg.ToUpperInvariant())
        {
            case "ANY":
                _config.DesiredPositional = Positional.Any;
                break;
            case "FLANK":
                _config.DesiredPositional = Positional.Flank;
                break;
            case "REAR":
                _config.DesiredPositional = Positional.Rear;
                break;
            case "FRONT":
                _config.DesiredPositional = Positional.Front;
                break;
            default:
                if (_config.EchoToChat)
                {
                    Service.ChatGui.Print($"[BMRAI] Unknown positional: {msg}");
                }
                return;
        }
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] Desired positional set to {_config.DesiredPositional}");
        }
    }

    private void HandleMaxDistanceTargetCommand(string[] messageData)
    {
        if (messageData.Length < 2 && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] Missing distance value.");
            return;
        }

        if (!float.TryParse(messageData[1].Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var distance) && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] Invalid distance value.");
            return;
        }

        _config.MaxDistanceToTarget = distance;
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] Max distance to target set to {distance.ToString(System.Globalization.CultureInfo.InvariantCulture)}y");
        }
    }

    private void HandleMaxDistanceSlotCommand(string[] messageData)
    {
        if (messageData.Length < 2 && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] Missing distance value.");
            return;
        }

        if (!float.TryParse(messageData[1].Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var distance) && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] Invalid distance value.");
            return;
        }

        _config.MaxDistanceToSlot = distance;
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] Max distance to slot set to {distance.ToString(System.Globalization.CultureInfo.InvariantCulture)}y");
        }
    }

    private void HandleMinDistanceCommand(string[] messageData)
    {
        if (messageData.Length < 2 && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] Missing distance value.");
            return;
        }

        if (!float.TryParse(messageData[1].Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var distance) && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] Invalid distance value.");
            return;
        }

        _config.MinDistance = distance;
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] Min distance to slot set to {distance.ToString(System.Globalization.CultureInfo.InvariantCulture)}y");
        }
    }

    private void HandlePrefDistanceCommand(string[] messageData)
    {
        if (messageData.Length < 2 && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] Missing distance value.");
            return;
        }

        if (!float.TryParse(messageData[1].Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var distance) && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] Invalid distance value.");
            return;
        }

        _config.PreferredDistance = distance;
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] Preferred distance to slot set to {distance.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        }
    }

    private void HandleMoveDelayCommand(string[] messageData)
    {
        if (messageData.Length < 2 && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] Missing delay value.");
            return;
        }

        if (!float.TryParse(messageData[1].Replace(',', '.'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var delay) && _config.EchoToChat)
        {
            Service.ChatGui.Print("[BMRAI] Invalid delay value.");
            return;
        }

        _config.MoveDelay = delay;
        if (_config.EchoToChat)
        {
            Service.ChatGui.Print($"[BMRAI] Max distance to target set to {delay.ToString(System.Globalization.CultureInfo.InvariantCulture)}s");
        }
    }

    private int FindPartyMemberByName(string name)
    {
        for (var i = 0; i < 8; ++i)
        {
            var member = Autorot.WorldState.Party[i];
            if (member != null && member.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return -1;
    }

    private void ParseAIAutorotationSetCommand()
    {
        if (_config.EchoToChat)
            Service.ChatGui.Print("AI movement no longer uses autorotation presets. Enable VBM AI directly in Autorotation if you want the AI preset.");
    }
}
