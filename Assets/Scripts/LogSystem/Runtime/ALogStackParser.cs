using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// 把Lua的debug.traceback文本与Unity的C#堆栈文本解析成结构化栈帧,
/// 供控制台点击跳转和调用堆栈可视化使用。
/// </summary>
public static class ALogStackParser
{
    //  require加载的chunk:  D:/xx/BaseUI.lua:13: in function 'OnClicked'  /  ...: in main chunk
    //  DoString加载的chunk: [string "D:/xx/BaseUI.lua"]:13: in ...
    private static readonly Regex s_luaFrame = new Regex(@"^\s*(?:\[string "")?(?<path>[^""]+?\.lua)(?:""\])?:(?<line>\d+):\s*in\s+(?<sig>.+?)\s*$", RegexOptions.Compiled);

    //签名里内嵌的 [string "D:/xx/BaseUI.lua"] 太长,显示成文件名即可
    private static readonly Regex s_inlineChunk = new Regex(@"\[string ""(?<path>[^""]+)""\]", RegexOptions.Compiled);

    //  [C]: in function 'error'
    private static readonly Regex s_luaNativeFrame = new Regex(@"^\s*\[C\]:\s*in\s+(?<sig>.+?)\s*$", RegexOptions.Compiled);

    //  Foo.Bar () (at Assets/Scripts/X.cs:23)
    private static readonly Regex s_csFrame = new Regex(@"^\s*(?<sig>.+?)\s*\(at\s+(?<path>.+?):(?<line>\d+)\)\s*$", RegexOptions.Compiled);

    /// <summary>解析Lua的debug.traceback,返回由内向外的栈帧</summary>
    public static List<ALogFrame> ParseLua(string traceback) {
        var frames = new List<ALogFrame>();
        if (string.IsNullOrEmpty(traceback))
        {
            return frames;
        }

        foreach (string raw in SplitLines(traceback))
        {
            Match match = s_luaFrame.Match(raw);
            if (match.Success)
            {
                frames.Add(new ALogFrame
                {
                    Signature = ShortenSignature(match.Groups["sig"].Value),
                    FilePath = match.Groups["path"].Value.Replace('\\', '/'),
                    Line = int.Parse(match.Groups["line"].Value),
                    IsLua = true,
                });
                continue;
            }

            Match nativeMatch = s_luaNativeFrame.Match(raw);
            if (nativeMatch.Success)
            {
                frames.Add(new ALogFrame { Signature = nativeMatch.Groups["sig"].Value, IsLua = true });
            }
        }
        return frames;
    }

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

    private static string ShortenSignature(string signature) {
        return s_inlineChunk.Replace(signature, match => {
            string path = match.Groups["path"].Value.Replace('\\', '/');
            return path.Substring(path.LastIndexOf('/') + 1);
        });
    }

    private static string[] SplitLines(string text) {
        return text.Replace("\r\n", "\n").Split('\n');
    }
}
