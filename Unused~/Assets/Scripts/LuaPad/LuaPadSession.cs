using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed class LuaPadSession : IDisposable
{
    LuaPadLspClient m_lsp;
    LuaPadHttpServer m_http;
    int m_version = 1;
    string m_text = LuaPadView.Placeholder;
    string m_scratchPath;

    public string Origin => m_http != null ? m_http.Origin : null;

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
            session.m_http = LuaPadHttpServer.Start(session.HandleRpc);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[LuaPad] Monaco 启动失败: " + e.Message);
        }
        return session;
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
        if (!visible || m_http == null)
        {
            return;
        }
        Application.OpenURL(m_http.Origin + "/");
    }

    public JObject HandleRpc(JObject msg)
    {
        string method = (string)msg["method"];
        if (method == "completion")
        {
            return HandleCompletion(msg);
        }
        if (method == "signatureHelp")
        {
            return HandleSignatureHelp(msg);
        }
        if (method == "draftsList")
        {
            return HandleDraftsList();
        }
        if (method == "draftSave")
        {
            return HandleDraftSave(msg);
        }
        if (method == "draftLoad")
        {
            return HandleDraftLoad(msg);
        }
        var done = new ManualResetEventSlim(false);
        JObject result = null;
        Exception error = null;
        LuaPadMainThread.Enqueue(() =>
        {
            try
            {
                if (method == "changed")
                {
                    result = HandleChanged(msg);
                }
                else if (method == "run")
                {
                    result = HandleRun(msg);
                }
                else if (method == "close")
                {
                    LuaPadHost.Instance?.SetVisible(false);
                    result = new JObject();
                }
                else
                {
                    result = new JObject();
                }
            }
            catch (Exception e)
            {
                error = e;
            }
            finally
            {
                done.Set();
            }
        });
        done.Wait(30000);
        if (error != null)
        {
            throw error;
        }
        return result ?? new JObject();
    }

    JObject HandleCompletion(JObject msg)
    {
        string src = (string)msg["text"] ?? string.Empty;
        int cursor = LuaPadTextUtil.IndexAt(src, (int)msg["line"], (int)msg["character"]);
        JArray lsp = LuaPadTextUtil.NeedsLsp(src, cursor)
            ? Complete(src, (int)msg["line"], (int)msg["character"])
            : new JArray();
        return new JObject
        {
            ["id"] = msg["id"],
            ["items"] = LuaPadCompletion.BuildItems(LuaPadTextUtil.PrefixAt(src, cursor), lsp),
        };
    }

    JObject HandleSignatureHelp(JObject msg)
    {
        string src = (string)msg["text"] ?? string.Empty;
        SyncText(src);
        JObject help = m_lsp == null
            ? new JObject()
            : m_lsp.SignatureHelp(m_scratchPath, (int)msg["line"], (int)msg["character"]);
        return help ?? new JObject();
    }

    JObject HandleChanged(JObject msg)
    {
        SyncText((string)msg["text"]);
        return new JObject
        {
            ["diagnostics"] = m_lsp == null ? new JArray() : m_lsp.LatestDiagnostics(m_scratchPath),
        };
    }

    JObject HandleDraftsList()
    {
        var names = new JArray();
        foreach (string name in LuaPadWorkspace.ListDrafts())
        {
            names.Add(name);
        }
        return new JObject { ["names"] = names };
    }

    JObject HandleDraftSave(JObject msg)
    {
        string name = LuaPadWorkspace.SaveDraft((string)msg["name"], (string)msg["text"] ?? string.Empty);
        return new JObject { ["ok"] = true, ["name"] = name };
    }

    JObject HandleDraftLoad(JObject msg)
    {
        return new JObject { ["text"] = LuaPadWorkspace.LoadDraft((string)msg["name"]) };
    }

    JObject HandleRun(JObject msg)
    {
        string text = LuaPadTextUtil.SliceLines(
            (string)msg["text"] ?? m_text,
            (int)msg["startLine"],
            (int)msg["endLine"]);
        LuaPadRunResult result = LuaPadRunner.RunInGame(text);
        string output = result.Success
            ? (string.IsNullOrEmpty(result.Output) ? "(无输出)" : result.Output)
            : result.Error;
        if (!result.Success)
        {
            Debug.LogException(new Exception(result.Error));
        }
        return new JObject
        {
            ["ok"] = result.Success,
            ["output"] = output,
        };
    }

    public void Dispose()
    {
        m_http?.Dispose();
        m_lsp?.Dispose();
    }
}
