using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using System.Diagnostics;
using System.IO;

namespace BossMod;

public sealed class AboutTab(DirectoryInfo? replayDir)
{
    private readonly Color TitleColor = Color.FromComponents(255, 165, 0);
    private readonly Color SectionBgColor = Color.FromComponents(38, 38, 38);
    private readonly Color BorderColor = Color.FromComponents(178, 178, 178, 204);
    private readonly Color DiscordColor = Color.FromComponents(88, 101, 242);

    private string _lastErrorMessage = "";

    public void Draw()
    {
        using var wrap = ImRaii.TextWrapPos(0);

        ImGui.TextUnformatted("Boss Mod (vbm) 提供 Boss 战雷达、自动循环、爆发规划和 AI 功能。所有模块都可单独开关。支持渠道见本页底部链接。");
        ImGui.Spacing();
        DrawSection("雷达",
        [
            "在屏幕上显示区域小地图，包含玩家位置、Boss 位置、即将出现的 AoE 和其他机制信息。",
            "不需要强记技能名也能快速判断机制。",
            "可以直观看到自己是否会吃到即将命中的 AoE。",
            "仅在已支持的 Boss 中生效，可在“支持的 Boss”页查看。",
        ]);
        ImGui.Spacing();
        DrawSection("自动循环",
        [
            "尽可能执行最优输出循环。",
            "可在“自动循环预设”页创建和管理预设。",
            "每个循环模块的成熟度可在提示中查看。",
            "详细使用说明见项目 GitHub Wiki。",
        ]);
        ImGui.Spacing();
        DrawSection("爆发规划",
        [
            "为已支持的 Boss 创建技能规划方案。",
            "可在特定战斗中覆盖默认自动循环行为。",
            "按时间轴精确安排关键技能释放时机。",
            "详细使用说明见项目 GitHub Wiki。",
        ]);
        ImGui.Spacing();
        DrawSection("AI",
        [
            "在 Boss 战中自动执行走位。",
            "根据模块判定的安全区自动移动（可在雷达中看到）。",
            "不建议在任何组队内容中使用。",
            "可被其他插件调用用于流程自动化。",
        ]);
        ImGui.Spacing();
        DrawSection("回放",
        [
            "用于制作模块、排查机制问题和编排技能时间轴。",
            "反馈问题时请尽量附带回放；注意回放中会包含你的角色名。",
            "可在 设置 > 显示回放管理界面（或启用自动录制）中开启。",
            $"文件位置：'{replayDir}'。",
        ]);
        ImGui.Spacing();
        ImGui.Spacing();

        using (ImRaii.PushColor(ImGuiCol.Button, DiscordColor.ABGR))
            if (ImGui.Button("Puni.sh Discord", new(180, 0)))
                _lastErrorMessage = OpenLink("https://discord.gg/Zzrcc8kmvy");
        ImGui.SameLine();
        if (ImGui.Button("Boss Mod 仓库", new(180, 0)))
            _lastErrorMessage = OpenLink("https://github.com/awgil/ffxiv_bossmod");
        ImGui.SameLine();
        if (ImGui.Button("Boss Mod Wiki 教程", new(180, 0)))
            _lastErrorMessage = OpenLink("https://github.com/awgil/ffxiv_bossmod/wiki");
        ImGui.SameLine();
        if (ImGui.Button("打开回放文件夹", new(180, 0)) && replayDir != null)
            _lastErrorMessage = OpenDirectory(replayDir);

        if (_lastErrorMessage.Length > 0)
        {
            using var color = ImRaii.PushColor(ImGuiCol.Text, 0xff0000ff);
            ImGui.TextUnformatted(_lastErrorMessage);
        }
    }

    private void DrawSection(string title, string[] bulletPoints)
    {
        using var colorBackground = ImRaii.PushColor(ImGuiCol.ChildBg, SectionBgColor.ABGR);
        using var colorBorder = ImRaii.PushColor(ImGuiCol.Border, BorderColor.ABGR);
        using var section = ImRaii.Child(title, new(0, 150), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysUseWindowPadding);
        if (!section)
            return;

        using (ImRaii.PushColor(ImGuiCol.Text, TitleColor.ABGR))
        {
            ImGui.TextUnformatted(title);
        }

        ImGui.Separator();

        foreach (var point in bulletPoints)
        {
            ImGui.Bullet();
            ImGui.SameLine();
            ImGui.TextUnformatted(point);
        }
    }

    private string OpenLink(string link)
    {
        try
        {
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening link {link}: {e}");
            return $"无法打开链接 '{link}'，请手动在浏览器打开。";
        }
    }

    private string OpenDirectory(DirectoryInfo dir)
    {
        if (!dir.Exists)
            return $"目录 '{dir}' 不存在。";

        try
        {
            Process.Start(new ProcessStartInfo(dir.FullName) { UseShellExecute = true });
            return "";
        }
        catch (Exception e)
        {
            Service.Log($"Error opening directory {dir}: {e}");
            return $"无法打开目录 '{dir}'，请手动打开。";
        }
    }
}
