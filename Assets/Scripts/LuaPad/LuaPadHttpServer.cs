using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public sealed class LuaPadHttpServer : IDisposable
{
    readonly HttpListener m_listener;
    readonly Thread m_thread;
    readonly string m_root;
    volatile bool m_run = true;

    public string Origin { get; }

    LuaPadHttpServer(HttpListener listener, string root, int port)
    {
        m_listener = listener;
        m_root = root;
        Origin = "http://127.0.0.1:" + port;
        m_thread = new Thread(Loop) { IsBackground = true, Name = "LuaPadHttp" };
        m_thread.Start();
    }

    public static LuaPadHttpServer Start()
    {
        string root = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, "LuaPad"));
        var tcp = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        tcp.Start();
        int port = ((IPEndPoint)tcp.LocalEndpoint).Port;
        tcp.Stop();
        var listener = new HttpListener();
        listener.Prefixes.Add("http://127.0.0.1:" + port + "/");
        listener.Start();
        return new LuaPadHttpServer(listener, root, port);
    }

    void Loop()
    {
        while (m_run)
        {
            HttpListenerContext ctx;
            try
            {
                ctx = m_listener.GetContext();
            }
            catch
            {
                break;
            }
            try
            {
                Serve(ctx);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[LuaPadHttp] " + e.Message);
            }
        }
    }

    void Serve(HttpListenerContext ctx)
    {
        string rel = Uri.UnescapeDataString(ctx.Request.Url.AbsolutePath).TrimStart('/');
        if (string.IsNullOrEmpty(rel))
        {
            rel = "index.html";
        }
        string path = Path.GetFullPath(Path.Combine(m_root, rel.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(m_root, StringComparison.OrdinalIgnoreCase) || !File.Exists(path))
        {
            ctx.Response.StatusCode = 404;
            ctx.Response.Close();
            return;
        }
        byte[] data = File.ReadAllBytes(path);
        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = Mime(path);
        ctx.Response.ContentLength64 = data.Length;
        ctx.Response.OutputStream.Write(data, 0, data.Length);
        ctx.Response.Close();
    }

    static string Mime(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        switch (ext)
        {
            case ".html": return "text/html; charset=utf-8";
            case ".js": return "text/javascript; charset=utf-8";
            case ".css": return "text/css; charset=utf-8";
            case ".json": return "application/json";
            case ".wasm": return "application/wasm";
            case ".ttf": return "font/ttf";
            case ".woff": return "font/woff";
            case ".woff2": return "font/woff2";
            default: return "application/octet-stream";
        }
    }

    public void Dispose()
    {
        m_run = false;
        try
        {
            m_listener.Stop();
        }
        catch
        {
        }
        m_listener.Close();
    }
}
