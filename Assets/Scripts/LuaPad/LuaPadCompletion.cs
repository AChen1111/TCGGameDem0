using Newtonsoft.Json.Linq;

public static class LuaPadCompletion
{
    public static JArray BuildItems(string prefix, JArray lspItems)
    {
        var slim = new JArray();
        foreach (LuaPadKeyword kw in LuaPadTextUtil.KeywordItems(prefix))
        {
            slim.Add(FromKeyword(kw));
        }
        if (lspItems == null)
        {
            return slim;
        }
        foreach (JToken it in lspItems)
        {
            JObject mapped = FromLsp(it);
            if (mapped == null)
            {
                continue;
            }
            string label = (string)mapped["label"];
            bool dup = false;
            foreach (JToken existing in slim)
            {
                if ((string)existing["label"] == label)
                {
                    dup = true;
                    break;
                }
            }
            if (!dup)
            {
                slim.Add(mapped);
            }
        }
        return slim;
    }

    public static JObject FromKeyword(LuaPadKeyword kw)
    {
        bool snippet = kw.Insert != null && kw.Insert.IndexOf("${") >= 0;
        var obj = new JObject
        {
            ["label"] = kw.Label,
            ["insertText"] = kw.Insert,
            ["kind"] = snippet ? 27 : 17,
        };
        if (!string.IsNullOrEmpty(kw.Detail))
        {
            obj["detail"] = kw.Detail;
        }
        return obj;
    }

    public static JObject FromLsp(JToken it)
    {
        if (it == null)
        {
            return null;
        }
        string label = (string)it["label"];
        if (string.IsNullOrEmpty(label))
        {
            return null;
        }
        string insert = it["insertText"] != null && it["insertText"].Type == JTokenType.String
            ? (string)it["insertText"]
            : label;
        var obj = new JObject
        {
            ["label"] = label,
            ["insertText"] = insert,
        };
        string detail = Hint(it);
        if (!string.IsNullOrEmpty(detail))
        {
            obj["detail"] = detail;
        }
        if (it["kind"] != null && it["kind"].Type == JTokenType.Integer)
        {
            obj["kind"] = ToMonacoKind((int)it["kind"]);
        }
        string doc = Documentation(it["documentation"]);
        if (!string.IsNullOrEmpty(doc))
        {
            obj["documentation"] = doc;
        }
        return obj;
    }

    public static int ToMonacoKind(int lspKind)
    {
        switch (lspKind)
        {
            case 1: return 18;
            case 2: return 0;
            case 3: return 1;
            case 4: return 2;
            case 5: return 3;
            case 6: return 4;
            case 7: return 5;
            case 8: return 7;
            case 9: return 8;
            case 10: return 9;
            case 11: return 12;
            case 12: return 13;
            case 13: return 15;
            case 14: return 17;
            case 15: return 27;
            case 16: return 19;
            case 17: return 20;
            case 18: return 21;
            case 19: return 23;
            case 20: return 16;
            case 21: return 14;
            case 22: return 6;
            case 23: return 10;
            case 24: return 11;
            case 25: return 24;
            default: return 18;
        }
    }

    static string Hint(JToken it)
    {
        JToken details = it["labelDetails"];
        string extra = details != null ? (string)details["detail"] : null;
        if (string.IsNullOrEmpty(extra))
        {
            extra = (string)it["detail"];
        }
        return string.IsNullOrEmpty(extra) ? null : extra.Trim();
    }

    static string Documentation(JToken doc)
    {
        if (doc == null || doc.Type == JTokenType.Null)
        {
            return null;
        }
        if (doc.Type == JTokenType.Object)
        {
            return (string)doc["value"];
        }
        return (string)doc;
    }
}
