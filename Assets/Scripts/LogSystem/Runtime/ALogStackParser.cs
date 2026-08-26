using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// 把Unity的C#堆栈文本解析成结构化栈帧,供控制台点击跳转和调用堆栈可视化使用。
/// </summary>
public static class ALogStackParser
{
    //  Foo.Bar () (at Assets/Scripts/X.cs:23)
    private static readonly Regex s_csFrame = new Regex(@"^\s*(?<sig>.+?)\s*\(at\s+(?<path>.+?):(?<line>\d+)\)\s*$", RegexOptions.Compiled);

    /// <summary>解析Unity给出的C#堆栈文本</summary>
    public static List<ALogFrame> ParseCSharp(string stackTrace) {
        var frames = new List<ALogFrame>();
        if (string.IsNullOrEmpty(stackTrace))
        {
            return frames;
        }

        foreach (string raw in SplitLines(stackTrace))
        {
            Match match = s_csFrame.Match(raw);
            if (match.Success)
            {
                frames.Add(new ALogFrame
                {
                    Signature = match.Groups["sig"].Value,
                    FilePath = match.Groups["path"].Value.Replace('\\', '/'),
                    Line = int.Parse(match.Groups["line"].Value),
                });
                continue;
            }
            string trimmed = raw.Trim();
            if (trimmed.Length > 0)
            {
                frames.Add(new ALogFrame { Signature = trimmed });
            }
        }
        return frames;
    }

    private static string[] SplitLines(string text) {
        return text.Replace("\r\n", "\n").Split('\n');
    }
}
