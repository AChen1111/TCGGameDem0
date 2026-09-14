using System.Text;
using UnityEditor;
using UnityEngine;

public static class ProbeFontGlyphs
{
    static readonly string[] Fonts =
    {
        "Assets/UI/Fonts/FZZYJW.ttf",
        "Assets/UI/Fonts/FZYouHJW_512B.ttf",
        "Assets/UI/Fonts/FZYouHJW_509R.ttf",
        "Assets/UI/Fonts/FZBWKSJW.ttf",
        "Assets/UI/Fonts/DFHeiBold-B5.ttf",
        "Assets/UI/Fonts/DFZongYiBold-B.ttf",
        "Assets/UI/Fonts/YGO_Card_NA.ttf"
    };

    const string Sample = "【】「」『』（）①②③：、。太古的白石龙族调整效果";

    public static string Run()
    {
        var report = new StringBuilder();
        foreach (string path in Fonts)
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>(path);
            report.Append(System.IO.Path.GetFileName(path));
            report.Append(':');
            if (font == null)
            {
                report.Append("missing;");
                continue;
            }

            foreach (char c in Sample)
            {
                if (!font.HasCharacter(c))
                {
                    report.Append('[');
                    report.Append(c);
                    report.Append(']');
                }
            }

            report.Append(';');
        }

        return report.ToString();
    }
}
