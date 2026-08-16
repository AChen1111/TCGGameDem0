using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed class LuaPadSession : IDisposable
{
    LuaPadLspClient m_lsp;
    LuaPadHttpServer m_http;
    LuaPadBrowser m_browser;
    int m_version = 1;
    string m_text = LuaPadView.Placeholder;
    string m_scratchPath;

    public bool EditorIsLive => m_browser != null && m_browser.IsLive && m_browser.HasPlacement;

    public static LuaPadSession Start()
    {
        var session = new LuaPadSession();
        session.m_scratchPath = LuaPadWorkspace.ScratchPath;
        try
        {
            string exe = LuaPadEmmyLuaInstaller.EnsureBinary();
            session.m_lsp = LuaPadLspClient.Start(exe, LuaPadWorkspace.RuntimeRoot);
            File.WriteAllText(session.m_scratchPath, session.m_text);
            session.m_lsp.DidOpen(session.m_scratchPath, session.m_text);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[LuaPad] 语言服务启动失败: " + e.Message);
        }
        try
        {
            session.m_http = LuaPadHttpServer.Start();
            session.m_browser = LuaPadBrowser.TryStart(session.m_http.Origin, msg =>
            {
                if ((string)msg["method"] == "completion")
                {
                    System.Threading.ThreadPool.QueueUserWorkItem(_ => session.OnBrowserMessage(msg));
                    return;
                }
                LuaPadMainThread.Enqueue(() => session.OnBrowserMessage(msg));
            });
        }
        catch (Exception e)
        {
            Debug.LogWarning("[LuaPad] Monaco 启动失败: " + e.Message);
        }
        return session;
    }

    public string ReadBuffer(string fallback = null)
    {
        if (m_browser != null && m_browser.HasPlacement)
        {
            string text = m_browser.GetText();
            if (text != null)
            {
                SyncText(text);
                return m_text;
            }
        }
        if (fallback != null)
        {
            SyncText(fallback);
            return fallback;
        }
        return m_text;
    }

    public void SyncText(string text)
    {
        m_text = text ?? string.Empty;
        if (m_lsp == null)
        {
            return;
        }
        m_version++;
        File.WriteAllText(m_scratchPath, m_text);
        m_lsp.DidChange(m_scratchPath, m_version, m_text);
    }

    public JArray Complete(string text, int line, int character)
    {
        SyncText(text);
        if (m_lsp == null)
        {
            return new JArray();
        }
        return m_lsp.Complete(m_scratchPath, line, character);
    }

    public void SetVisible(bool visible)
    {
        m_browser?.SetVisible(visible);
    }

    void OnBrowserMessage(JObject msg)
    {
        string method = (string)msg["method"];
        if (method == "changed")
        {
            SyncText((string)msg["text"]);
            PushDiagnostics();
            return;
        }
        if (method == "run")
        {
            string text = (string)msg["text"] ?? m_text;
            LuaPadRunResult result = LuaPadRunner.RunInGame(text);
            PushOutput(result);
            return;
        }
        if (method == "close")
        {
            LuaPadHost.Instance?.SetVisible(false);
            return;
        }
        if (method != "completion")
        {
            return;
        }
        var slim = new JArray();
        string src = (string)msg["text"] ?? string.Empty;
        int cursor = LuaPadTextUtil.IndexAt(src, (int)msg["line"], (int)msg["character"]);
        foreach (LuaPadKeyword kw in LuaPadTextUtil.KeywordItems(LuaPadTextUtil.PrefixAt(src, cursor)))
        {
            slim.Add(new JObject { ["label"] = kw.Label, ["insertText"] = kw.Insert });
        }
        JArray items = LuaPadTextUtil.NeedsLsp(src, cursor)
            ? Complete(src, (int)msg["line"], (int)msg["character"])
            : new JArray();
        foreach (JToken it in items)
        {
            string label = (string)it["label"];
            if (string.IsNullOrEmpty(label))
            {
                continue;
            }
            bool dup = false;
            foreach (JToken existing in slim)
            {
                if ((string)existing["label"] == label)
                {
                    dup = true;
                    break;
                }
            }
            if (dup)
            {
                continue;
            }
            slim.Add(new JObject
            {
                ["label"] = label,
                ["insertText"] = it["insertText"] ?? label,
            });
        }
        m_browser.SendJsonToPage(new JObject
        {
            ["id"] = msg["id"],
            ["items"] = slim,
        }.ToString(Formatting.None));
    }

    void PushOutput(LuaPadRunResult result)
    {
        string text = result.Success
            ? (string.IsNullOrEmpty(result.Output) ? "(无输出)" : result.Output)
            : result.Error;
        if (!result.Success)
        {
            Debug.LogException(new Exception(result.Error));
        }
        if (!EditorIsLive)
        {
            return;
        }
        m_browser.SendJsonToPage(new JObject
        {
            ["ok"] = result.Success,
            ["output"] = text,
        }.ToString(Formatting.None));
    }

    void PushDiagnostics()
    {
        if (m_lsp == null || !EditorIsLive)
        {
            return;
        }
        m_browser.SendJsonToPage(new JObject
        {
            ["diagnostics"] = m_lsp.LatestDiagnostics(m_scratchPath),
        }.ToString(Formatting.None));
    }

    public void Dispose()
    {
        m_browser?.Dispose();
        m_http?.Dispose();
        m_lsp?.Dispose();
    }
}
