using System.Collections.Generic;

public readonly struct LuaPadKeyword
{
    public readonly string Label;
    public readonly string Insert;
    public readonly string Detail;

    public LuaPadKeyword(string label, string insert)
        : this(label, insert, null)
    {
    }

    public LuaPadKeyword(string label, string insert, string detail)
    {
        Label = label;
        Insert = insert;
        Detail = detail;
    }
}

public static class LuaPadTextUtil
{
    static readonly LuaPadKeyword[] Keywords =
    {
        new LuaPadKeyword("and", "and"),
        new LuaPadKeyword("break", "break"),
        new LuaPadKeyword("do", "do"),
        new LuaPadKeyword("else", "else"),
        new LuaPadKeyword("elseif", "elseif"),
        new LuaPadKeyword("end", "end"),
        new LuaPadKeyword("false", "false"),
        new LuaPadKeyword("for", "for ${1:i} = ${2:1}, ${3:10} do\n\t$0\nend", "for i = 1, 10 do .. end"),
        new LuaPadKeyword("function", "function ${1:name}(${2:})\n\t$0\nend", "function name() .. end"),
        new LuaPadKeyword("goto", "goto"),
        new LuaPadKeyword("if", "if ${1:condition} then\n\t$0\nend", "if condition then .. end"),
        new LuaPadKeyword("in", "in"),
        new LuaPadKeyword("ipairs", "ipairs()"),
        new LuaPadKeyword("local", "local"),
        new LuaPadKeyword("nil", "nil"),
        new LuaPadKeyword("not", "not"),
        new LuaPadKeyword("or", "or"),
        new LuaPadKeyword("pairs", "pairs()"),
        new LuaPadKeyword("pcall", "pcall()"),
        new LuaPadKeyword("print", "print()"),
        new LuaPadKeyword("repeat", "repeat\n\t$0\nuntil ${1:condition}", "repeat .. until condition"),
        new LuaPadKeyword("require", "require(\"\")"),
        new LuaPadKeyword("return", "return"),
        new LuaPadKeyword("then", "then"),
        new LuaPadKeyword("true", "true"),
        new LuaPadKeyword("type", "type()"),
        new LuaPadKeyword("until", "until"),
        new LuaPadKeyword("while", "while ${1:condition} do\n\t$0\nend", "while condition do .. end"),
    };

    public static void LineChar(string text, int index, out int line, out int character)
    {
        line = 0;
        character = 0;
        int n = index < 0 ? 0 : (index > text.Length ? text.Length : index);
        for (int i = 0; i < n; i++)
        {
            if (text[i] == '\n')
            {
                line++;
                character = 0;
            }
            else
            {
                character++;
            }
        }
    }

    public static int IndexAt(string text, int line, int character)
    {
        int i = 0;
        int l = 0;
        while (i < text.Length && l < line)
        {
            if (text[i] == '\n')
            {
                l++;
            }
            i++;
        }
        int n = i + character;
        return n < 0 ? 0 : (n > text.Length ? text.Length : n);
    }

    public static string PrefixAt(string text, int cursor)
    {
        if (text == null || cursor <= 0 || cursor > text.Length)
        {
            return string.Empty;
        }
        int start = PrefixStart(text, cursor);
        return text.Substring(start, cursor - start);
    }

    public static int PrefixStart(string text, int cursor)
    {
        int start = cursor;
        while (start > 0 && IsIdent(text[start - 1]))
        {
            start--;
        }
        return start;
    }

    public static List<LuaPadKeyword> KeywordItems(string prefix)
    {
        var list = new List<LuaPadKeyword>();
        if (string.IsNullOrEmpty(prefix))
        {
            return list;
        }
        for (int i = 0; i < Keywords.Length; i++)
        {
            LuaPadKeyword kw = Keywords[i];
            if (kw.Label.StartsWith(prefix))
            {
                list.Add(kw);
            }
        }
        return list;
    }

    public static int CaretAfterInsert(int start, string insert)
    {
        int pos = start + insert.Length;
        if (insert.EndsWith("(\"\")"))
        {
            return pos - 2;
        }
        if (insert.EndsWith("()"))
        {
            return pos - 1;
        }
        return pos;
    }

    public static int EffectiveCursor(string text, int cursor)
    {
        if (text == null)
        {
            return 0;
        }
        if (cursor >= 0 && cursor <= text.Length && ShouldComplete(text, cursor))
        {
            return cursor;
        }
        return text.Length;
    }

    public static bool NeedsLsp(string text, int cursor)
    {
        if (cursor <= 0 || cursor > text.Length)
        {
            return false;
        }
        char c = text[cursor - 1];
        if (c == '.' || c == ':')
        {
            return true;
        }
        return IsIdent(c);
    }

    public static bool ShouldComplete(string text, int cursor)
    {
        if (cursor <= 0 || cursor > text.Length)
        {
            return false;
        }
        if (IsIdent(text[cursor - 1]))
        {
            return true;
        }
        return NeedsLsp(text, cursor);
    }

    public static string ApplyCompletion(string text, int cursor, string label)
    {
        int start = PrefixStart(text, cursor);
        return text.Substring(0, start) + label + text.Substring(cursor);
    }

    public static string SliceLines(string text, int startLine, int endLine)
    {
        string[] lines = text.Split('\n');
        return string.Join("\n", lines, startLine - 1, endLine - startLine + 1);
    }

    static bool IsIdent(char c)
    {
        return c == '_' || (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
    }
}
