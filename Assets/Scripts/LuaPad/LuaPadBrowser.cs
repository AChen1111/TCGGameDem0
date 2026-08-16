using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;

public sealed class LuaPadBrowser : IDisposable
{
    readonly Process m_process;
    readonly TcpClient m_tcp;
    readonly StreamWriter m_writer;
    readonly StreamReader m_reader;
    readonly object m_gate = new object();
    readonly Action<JObject> m_onMessage;
    volatile bool m_run = true;

    public bool IsLive { get; private set; }
    public bool HasPlacement { get; private set; }

    public static string BuildStartArguments(string origin, int port)
    {
        return "--port " + port + " --url " + origin + "/";
    }

    LuaPadBrowser(Process process, TcpClient tcp, Action<JObject> onMessage)
    {
        m_process = process;
        m_tcp = tcp;
        m_onMessage = onMessage;
        m_writer = new StreamWriter(tcp.GetStream(), Encoding.UTF8) { AutoFlush = true };
        m_reader = new StreamReader(tcp.GetStream(), Encoding.UTF8);
        IsLive = true;
        var thread = new Thread(ReadLoop) { IsBackground = true, Name = "LuaPadBrowser" };
        thread.Start();
    }

    public static LuaPadBrowser TryStart(string origin, Action<JObject> onMessage)
    {
        foreach (Process leftover in Process.GetProcessesByName("LuaPadBrowser"))
        {
            try
            {
                leftover.Kill();
            }
            catch
            {
            }
        }
        string exe = LuaPadBrowserInstaller.EnsureHelper();
        if (string.IsNullOrEmpty(exe) || !File.Exists(exe))
        {
            UnityEngine.Debug.LogWarning("[LuaPad] WebView 助手不可用");
            return null;
        }
        var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        var psi = new ProcessStartInfo
        {
            FileName = exe,
            Arguments = BuildStartArguments(origin, port),
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        Process process = Process.Start(psi);
        IAsyncResult ar = listener.BeginAcceptTcpClient(null, null);
        if (!ar.AsyncWaitHandle.WaitOne(8000))
        {
            listener.Stop();
            try
            {
                process?.Kill();
            }
            catch
            {
            }
            UnityEngine.Debug.LogWarning("[LuaPad] WebView 助手未在超时内连接");
            return null;
        }
        TcpClient tcp = listener.EndAcceptTcpClient(ar);
        listener.Stop();
        return new LuaPadBrowser(process, tcp, onMessage);
    }

    public void SetVisible(bool visible)
    {
        if (!IsLive)
        {
            return;
        }
        HasPlacement = visible;
        Send(new JObject { ["cmd"] = "visible", ["value"] = visible });
    }

    public string GetText()
    {
        if (!IsLive)
        {
            return null;
        }
        string id = Guid.NewGuid().ToString("N");
        var waiter = new ManualResetEventSlim(false);
        string text = null;
        void Handler(JObject msg)
        {
            if ((string)msg["event"] == "eval" && (string)msg["id"] == id)
            {
                text = (string)msg["value"];
                waiter.Set();
            }
        }
        m_onEval = Handler;
        Send(new JObject { ["cmd"] = "eval", ["id"] = id, ["js"] = "window.luaPadGetText && window.luaPadGetText()" });
        waiter.Wait(2000);
        m_onEval = null;
        return text;
    }

    Action<JObject> m_onEval;

    public void SendJsonToPage(string json)
    {
        Send(new JObject { ["cmd"] = "eval", ["id"] = "push", ["js"] = "window.luaPadOnHost && window.luaPadOnHost(" + json + ")" });
    }

    public static bool TryWriteLine(StreamWriter writer, string line)
    {
        try
        {
            writer.WriteLine(line);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    void Send(JObject msg)
    {
        if (!IsLive)
        {
            return;
        }
        lock (m_gate)
        {
            if (!TryWriteLine(m_writer, msg.ToString(Newtonsoft.Json.Formatting.None)))
            {
                IsLive = false;
            }
        }
    }

    void ReadLoop()
    {
        while (m_run)
        {
            string line;
            try
            {
                line = m_reader.ReadLine();
            }
            catch
            {
                IsLive = false;
                break;
            }
            if (line == null)
            {
                IsLive = false;
                break;
            }
            JObject msg;
            try
            {
                msg = JObject.Parse(line);
            }
            catch
            {
                continue;
            }
            m_onEval?.Invoke(msg);
            m_onMessage?.Invoke(msg);
        }
    }

    public void Dispose()
    {
        m_run = false;
        try
        {
            Send(new JObject { ["cmd"] = "quit" });
        }
        catch
        {
        }
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
        m_tcp.Dispose();
        m_process.Dispose();
    }
}
