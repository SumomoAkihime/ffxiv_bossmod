using System.Globalization;

namespace BossMod;

public static class Loc
{
    private static Dictionary<string, string> _strings = [];

    public static bool Active { get; private set; }

    public static void Initialize()
    {
        Active = DetectChineseClient();
        if (!Active)
        {
            return;
        }

        try
        {
            _strings = Utils.LoadFromAssembly<Dictionary<string, string>>("BossMod.Localization.zh-CN.json");
            Service.Log($"[Loc] zh-CN localization enabled, loaded {_strings.Count} strings.");
        }
        catch (Exception e)
        {
            Active = false;
            _strings = [];
            Service.Log($"[Loc] Failed to load zh-CN localization: {e}");
        }
    }

    public static string Tr(string text)
        => Active && _strings.TryGetValue(text, out var translated) ? translated : text;

    public static string Tr(string format, params object?[] args)
        => string.Format(CultureInfo.CurrentCulture, Tr(format), args);

    public static string UiLabel(string text)
    {
        var translated = Tr(text);
        return translated == text ? text : $"{translated}###{text}";
    }

    private static bool DetectChineseClient()
    {
        var signals = new[]
        {
            SafeSignal(() => Service.ClientState.ClientLanguage.ToString()),
            SafeSignal(() => Service.DataManager.GetType().GetProperty("Language")?.GetValue(Service.DataManager)?.ToString()),
            SafeSignal(DetectChineseLumina),
            CultureInfo.CurrentUICulture.Name,
            CultureInfo.CurrentCulture.Name,
        };

        return signals.Any(IsChineseSignal);
    }

    private static string SafeSignal(Func<string?> signal)
    {
        try
        {
            return signal() ?? "";
        }
        catch
        {
            return "";
        }
    }

    private static bool IsChineseSignal(string signal)
    {
        if (string.IsNullOrWhiteSpace(signal))
        {
            return false;
        }

        return signal.Contains("Chinese", StringComparison.OrdinalIgnoreCase)
            || signal.Contains("China", StringComparison.OrdinalIgnoreCase)
            || signal.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
            || signal.Contains("Hans", StringComparison.OrdinalIgnoreCase)
            || signal.Contains("Hant", StringComparison.OrdinalIgnoreCase)
            || signal.Contains("重生", StringComparison.Ordinal)
            || signal.Contains("苍穹", StringComparison.Ordinal)
            || signal.Contains("红莲", StringComparison.Ordinal)
            || signal.Contains("暗影", StringComparison.Ordinal)
            || signal.Contains("晓月", StringComparison.Ordinal)
            || signal.Contains("金曦", StringComparison.Ordinal);
    }

    private static string DetectChineseLumina()
    {
        var sheet = Service.LuminaSheet<Lumina.Excel.Sheets.ExVersion>();
        if (sheet == null)
        {
            return "";
        }

        var names = new List<string>();
        for (var i = 0u; i <= 5u; ++i)
        {
            var row = sheet.GetRowOrDefault(i);
            if (row != null)
            {
                names.Add(row.Value.Name.ToString());
            }
        }

        return string.Join('|', names);
    }
}
