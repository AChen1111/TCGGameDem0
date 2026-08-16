using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public sealed class LuaPadLspClient : IDisposable
{
    readonly Process m_process;
    readonly object m_gate = new object();
    readonly Dictionary<int, Pending> m_pending = new Dictionary<int, Pending>();
    readonly StringBuilder m_header = new StringBuilder();
    readonly List<JObject> m_notes = new List<JObject>();
    int m_nextId = 1;
    bool m_inBody;
    int m_bodyLen;
    MemoryStream m_body = new MemoryStream();

    sealed class Pending
    {
        public readonly ManualResetEventSlim Done = new ManualResetEventSlim(false);
        public JToken Result;
        public string Error;
    }

    LuaPadLspClient(Process process)
    {
        m_process = process;
        var thread = new Thread(ReadLoop) { IsBackground = true, Name = "LuaPadLsp" };
        thread.Start();
    }

    public static LuaPadLspClient Start(string exePath, string workspaceRoot)
    {
        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            WorkingDirectory = Path.GetDirectoryName(exePath) ?? "",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
        };
        Process process = Process.Start(psi);
        process.ErrorDataReceived += (_, __) => { };
        process.BeginErrorReadLine();
        var client = new LuaPadLspClient(process);
        string uri = new Uri(workspaceRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar).AbsoluteUri;
        client.Request("initialize", new JObject
        {
            ["processId"] = Process.GetCurrentProcess().Id,
            ["rootUri"] = uri,
            ["rootPath"] = workspaceRoot,
            ["capabilities"] = new JObject
            {
                ["textDocument"] = new JObject
                {
                    ["completion"] = new JObject { ["completionItem"] = new JObject { ["snippetSupport"] = false } },
                    ["publishDiagnostics"] = new JObject(),
                },
                ["workspace"] = new JObject { ["workspaceFolders"] = true },
            },
            ["workspaceFolders"] = new JArray
            {
                new JObject { ["uri"] = uri, ["name"] = "LuaRaw" },
            },
        }, 20000);
        client.Notify("initialized", new JObject());
        return client;
    }

    public void DidOpen(string filePath, string text)
    {
        Notify("textDocument/didOpen", new JObject
        {
            ["textDocument"] = new JObject
            {
                ["uri"] = new Uri(filePath).AbsoluteUri,
                ["languageId"] = "lua",
                ["version"] = 1,
                ["text"] = text,
            },
        });
    }

    public void DidChange(string filePath, int version, string text)
    {
        Notify("textDocument/didChange", new JObject
        {
            ["textDocument"] = new JObject
            {
                ["uri"] = new Uri(filePath).AbsoluteUri,
                ["version"] = version,
            },
            ["contentChanges"] = new JArray { new JObject { ["text"] = text } },
        });
    }

    public JArray Complete(string filePath, int line, int character)
    {
        JToken result;
        try
        {
            result = Request("textDocument/completion", new JObject
            {
                ["textDocument"] = new JObject { ["uri"] = new Uri(filePath).AbsoluteUri },
                ["position"] = new JObject { ["line"] = line, ["character"] = character },
            }, 2000);
        }
        catch
        {
            return new JArray();
        }
        if (result == null || result.Type == JTokenType.Null || result.Type == JTokenType.Undefined)
        {
            return new JArray();
        }
        if (result.Type == JTokenType.Array)
        {
            return (JArray)result;
        }
        if (result.Type == JTokenType.Object)
        {
            return result["items"] as JArray ?? new JArray();
        }
        return new JArray();
    }

    public JArray LatestDiagnostics(string filePath)
    {
        string uri = NormalizeUri(filePath);
        lock (m_gate)
        {
            for (int i = m_notes.Count - 1; i >= 0; i--)
            {
                JObject note = m_notes[i];
                if ((string)note["method"] != "textDocument/publishDiagnostics")
                {
                    continue;
                }
                JToken p = note["params"];
                if (p != null && NormalizeUri((string)p["uri"]) == uri)
                {
                    return p["diagnostics"] as JArray ?? new JArray();
                }
            }
        }
        return new JArray();
    }

    static string NormalizeUri(string pathOrUri)
    {
        if (string.IsNullOrEmpty(pathOrUri))
        {
            return string.Empty;
        }
        if (!pathOrUri.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            pathOrUri = new Uri(pathOrUri).AbsoluteUri;
        }
        return Uri.UnescapeDataString(pathOrUri).Replace('\\', '/').ToLowerInvariant();
    }

    public JToken Request(string method, JObject paramsObj, int timeoutMs)
    {
        Pending pending;
        int id;
        lock (m_gate)
        {
            id = m_nextId++;
            pending = new Pending();
            m_pending[id] = pending;
        }
        Write(new JObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id,
            ["method"] = method,
            ["params"] = paramsObj,
        });
        if (!pending.Done.Wait(timeoutMs))
        {
            throw new TimeoutException("LSP " + method);
        }
        if (pending.Error != null)
        {
            throw new InvalidOperationException(pending.Error);
        }
        return pending.Result;
    }

    public void Notify(string method, JObject paramsObj)
    {
        Write(new JObject
        {
            ["jsonrpc"] = "2.0",
            ["method"] = method,
            ["params"] = paramsObj,
        });
    }

    void Write(JObject msg)
    {
        byte[] body = Encoding.UTF8.GetBytes(msg.ToString(Formatting.None));
        byte[] head = Encoding.ASCII.GetBytes("Content-Length: " + body.Length + "\r\n\r\n");
        lock (m_gate)
        {
            Stream stdin = m_process.StandardInput.BaseStream;
            stdin.Write(head, 0, head.Length);
            stdin.Write(body, 0, body.Length);
            stdin.Flush();
        }
    }

    void ReadLoop()
    {
        Stream stdout = m_process.StandardOutput.BaseStream;
        var buf = new byte[4096];
        while (!m_process.HasExited)
        {
            int n;
            try
            {
                n = stdout.Read(buf, 0, buf.Length);
            }
            catch
            {
                break;
            }
            if (n <= 0)
            {
                break;
            }
            for (int i = 0; i < n; i++)
            {
                Consume(buf[i]);
            }
        }
    }

    void Consume(byte b)
    {
        if (!m_inBody)
        {
            m_header.Append((char)b);
            if (m_header.Length >= 4 && m_header.ToString().EndsWith("\r\n\r\n"))
            {
                string header = m_header.ToString();
                const string key = "Content-Length:";
                int idx = header.IndexOf(key, StringComparison.OrdinalIgnoreCase);
                int start = idx + key.Length;
                int end = header.IndexOf("\r\n", start, StringComparison.Ordinal);
                m_bodyLen = int.Parse(header.Substring(start, end - start).Trim());
                m_header.Length = 0;
                m_body.SetLength(0);
                m_inBody = true;
            }
            return;
        }
        m_body.WriteByte(b);
        if (m_body.Length >= m_bodyLen)
        {
            string json = Encoding.UTF8.GetString(m_body.ToArray());
            m_inBody = false;
            Handle(JObject.Parse(json));
        }
    }

    void Handle(JObject msg)
    {
        if (msg["id"] != null && msg["method"] != null)
        {
            Write(new JObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = msg["id"],
                ["result"] = null,
            });
            return;
        }
        if (msg["id"] != null && msg["method"] == null)
        {
            int id = msg["id"].Value<int>();
            lock (m_gate)
            {
                if (m_pending.TryGetValue(id, out Pending pending))
                {
                    m_pending.Remove(id);
                    pending.Result = msg["result"];
                    if (msg["error"] != null)
                    {
                        pending.Error = msg["error"].ToString();
                    }
                    pending.Done.Set();
                }
            }
            return;
        }
        lock (m_gate)
        {
            m_notes.Add(msg);
        }
    }

    public void Dispose()
    {
        try
        {
            if (!m_process.HasExited)
            {
                m_process.Kill();
            }
        }
        catch
        {
        }
        m_process.Dispose();
        lock (m_gate)
        {
            foreach (Pending pending in m_pending.Values)
            {
                pending.Done.Set();
            }
        }
    }
}
