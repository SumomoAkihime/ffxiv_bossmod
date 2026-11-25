using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using RebornMaterial;
using System.Diagnostics;
using System.IO;

namespace BossMod.RebornUI;

internal sealed class HomePage(DirectoryInfo? replayDir, Action<string> navigate)
{
    public const string DiscordUrl = "https://discord.gg/p54TZMPnC9";
    public const string GitHubUrl = "https://github.com/FFXIV-CombatReborn/BossmodReborn";
    public const string WikiUrl = "https://github.com/awgil/ffxiv_bossmod/wiki";

    private static readonly Vector4 DiscordColor = Color.FromComponents(88u, 101u, 242u).ToFloat4();

    private readonly record struct Feature(string Title, FontAwesomeIcon Icon, string Page, string Action, string[] Points);

    private readonly Feature[] _features =
    [
        new(Loc.Tr("Radar"), FontAwesomeIcon.Bullseye, nameof(BossModuleConfig), Loc.Tr("Radar settings"),
        [
            Loc.Tr("An on-screen mini-map of the arena showing players, bosses, imminent AOEs and other mechanics."),
            Loc.Tr("No need to remember what every ability does: see whether an incoming AOE will clip you."),
            Loc.Tr("Works in every fight listed under Supported fights."),
        ]),
        new(Loc.Tr("Autorotation"), FontAwesomeIcon.Bolt, SettingsWindow.PresetsPage, Loc.Tr("Edit presets"),
        [
            Loc.Tr("VBM's autorotation system."),
            Loc.Tr("Choose multiple presets and control their strategies from the autorotation window."),
        ]),
        new(Loc.Tr("Cooldown planner"), FontAwesomeIcon.CalendarAlt, SettingsWindow.FightsPage, Loc.Tr("Supported fights"),
        [
            Loc.Tr("Creates cooldown plans for supported bosses."),
            Loc.Tr("Times specific abilities to be used at specific moments with special compatibility with Rotation Solver Reborn."),
        ]),
        new("Automovement", FontAwesomeIcon.Robot, nameof(AI.AIConfig), Loc.Tr("Automovement settings"),
        [
            Loc.Tr("Moves your character to the safe spots that a fight module shows on the radar."),
            "Shouldn't be used when playing with players you don't know.",
        ]),
        new(Loc.Tr("Replays"), FontAwesomeIcon.Film, nameof(ReplayManagementConfig), Loc.Tr("Replay settings"),
        [
            Loc.Tr("Used to create fight modules, analyse problems with them and make cooldown plans."),
            Loc.Tr("When asking for help, include a replay. Replays contain your character's name unless anonymised."),
            Loc.Tr("Turn on the replay management window or automatic recording in Replays."),
        ]),
    ];

    public void Draw()
    {
        var s = M3.Scheme;
        using (M3Card.Begin("home_intro", "BossMod Reborn", FontAwesomeIcon.None, s.Primary, M3CardStyle.Elevated))
        {
            var logoSize = 64f * M3.Scale;
            ImGui.Dummy(new(logoSize));
            RebornFoldWindow.DrawLogo(ImGui.GetWindowDrawList(), ImGui.GetItemRectMin(), logoSize);
            ImGui.SameLine(0f, M3.Space3);
            using (ImRaii.Group())
            {
                M3Text.Draw(Loc.Tr("Configure encounter radars, autorotation presets, movement, cooldown plans and replays."), M3TextStyle.Body, true);
            }
            ImGui.Dummy(new(0f, M3.Space1));

        }

        DrawFeatures();
        DrawLinks();
    }

    private void DrawFeatures()
    {
        var columns = ImGui.GetContentRegionAvail().X >= 620f * M3.Scale ? 2 : 1;
        using var table = ImRaii.Table("##home_features", columns, ImGuiTableFlags.SizingStretchSame);
        if (!table)
        {
            return;
        }

        for (var i = 0; i < _features.Length; ++i)
        {
            ImGui.TableNextColumn();
            ref readonly var feature = ref _features[i];
            using var card = M3Card.Begin($"home_feature_{i}", feature.Title, feature.Icon, M3.AccentFor(i));
            foreach (var point in feature.Points)
            {
                M3Text.Bullet(point, M3TextStyle.Muted, dot: M3.AccentFor(i));
            }

            ImGui.Dummy(new(0f, M3.Space1));
            if (M3Widgets.Button($"##home_open_{i}", feature.Action, M3ButtonStyle.Tonal, FontAwesomeIcon.ArrowRight))
            {
                navigate(feature.Page);
            }
        }
    }

    private void DrawLinks()
    {
        using var card = M3Card.Begin("home_links", Loc.Tr("Links"), FontAwesomeIcon.Link, M3.Scheme.Secondary);
        var error = "";
        var flow = new M3Flow();

        flow.Next(M3Widgets.ButtonWidth(FontAwesomeIcon.Comments, "Combat Reborn Discord"));
        if (M3Widgets.Button("##home_discord", "Combat Reborn Discord", M3ButtonStyle.Filled, FontAwesomeIcon.Comments, accent: DiscordColor))
        {
            error = OpenLink(DiscordUrl);
        }

        flow.Next(M3Widgets.ButtonWidth(FontAwesomeIcon.CodeBranch, "GitHub"));
        if (M3Widgets.Button("##home_github", "GitHub", M3ButtonStyle.Tonal, FontAwesomeIcon.CodeBranch))
        {
            error = OpenLink(GitHubUrl);
        }

        flow.Next(M3Widgets.ButtonWidth(FontAwesomeIcon.Book, Loc.Tr("BossMod wiki")));
        if (M3Widgets.Button("##home_wiki", Loc.Tr("BossMod wiki"), M3ButtonStyle.Tonal, FontAwesomeIcon.Book))
        {
            error = OpenLink(WikiUrl);
        }

        flow.Next(M3Widgets.ButtonWidth(FontAwesomeIcon.FolderOpen, Loc.Tr("Open replay folder")));
        if (M3Widgets.Button("##home_replays", Loc.Tr("Open replay folder"), M3ButtonStyle.Outlined, FontAwesomeIcon.FolderOpen, enabled: replayDir != null, tooltip: replayDir?.FullName))
        {
            error = OpenDirectory(replayDir!);
        }

        if (error.Length > 0)
        {
            M3Snackbar.Show(error, severity: M3Severity.Error);
        }
    }

    public static string OpenLink(string link)
    {
        try
        {
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening link {link}: {e}");
            return Loc.Tr("Failed to open link '{0}', open it manually in the browser.", link);
        }
    }

    private static string OpenDirectory(DirectoryInfo dir)
    {
        if (!dir.Exists)
        {
            return Loc.Tr("Directory '{0}' not found.", dir);
        }

        try
        {
            Process.Start(new ProcessStartInfo(dir.FullName) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening directory {dir}: {e}");
            return Loc.Tr("Failed to open folder '{0}', open it manually.", dir);
        }
    }
}
