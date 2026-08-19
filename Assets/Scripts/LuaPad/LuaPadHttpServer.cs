using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed class LuaPadHttpServer : IDisposable
{
    readonly HttpListener m_listener;
    readonly Thread m_thread;
    readonly string m_root;
    readonly Func<JObject, JObject> m_onRpc;
    volatile bool m_run = true;

    public string Origin { get; }

    LuaPadHttpServer(HttpListener listener, string root, int port, Func<JObject, JObject> onRpc)
    {
        m_listener = listener;
        m_root = root;
        m_onRpc = onRpc;
        Origin = "http://127.0.0.1:" + port;
        m_thread = new Thread(Loop) { IsBackground = true, Name = "LuaPadHttp" };
        m_thread.Start();
    }

    public static LuaPadHttpServer Start()
    {
        return Start(null);
    }

    public static LuaPadHttpServer Start(Func<JObject, JObject> onRpc)
    {
        string root = Path.GetFullPath(Path.Combine(Application.streamingAssetsPath, "LuaPad"));
        var tcp = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        tcp.Start();
        int port = ((IPEndPoint)tcp.LocalEndpoint).Port;
        tcp.Stop();
        var listener = new HttpListener();
        listener.Prefixes.Add("http://127.0.0.1:" + port + "/");
        listener.Start();
        return new LuaPadHttpServer(listener, root, port, onRpc);
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
                if (e is IOException || e is HttpListenerException)
                {
                    continue;
                }
                Debug.LogWarning("[LuaPadHttp] " + e.Message);
            }
        }
    }

    void Serve(HttpListenerContext ctx)
    {
        string rel = Uri.UnescapeDataString(ctx.Request.Url.AbsolutePath).TrimStart('/');
        if (rel == "rpc" && ctx.Request.HttpMethod == "POST")
        {
            ServeRpc(ctx);
            return;
        }
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
        WriteBytes(ctx, data);
    }

    void ServeRpc(HttpListenerContext ctx)
    {
        if (m_onRpc == null)
        {
            ctx.Response.StatusCode = 404;
            ctx.Response.Close();
            return;
        }
        string body;
        using (var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding))
        {
            body = reader.ReadToEnd();
        }
        JObject res;
        try
        {
            res = m_onRpc(JObject.Parse(body)) ?? new JObject();
        }
        catch (Exception e)
        {
            res = new JObject { ["error"] = e.Message };
        }
        byte[] data = Encoding.UTF8.GetBytes(res.ToString(Formatting.None));
        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = "application/json; charset=utf-8";
        ctx.Response.ContentLength64 = data.Length;
        WriteBytes(ctx, data);
    }

    static void WriteBytes(HttpListenerContext ctx, byte[] data)
    {
        try
        {
            ctx.Response.OutputStream.Write(data, 0, data.Length);
            ctx.Response.Close();
        }
        catch (IOException)
        {
        }
        catch (HttpListenerException)
        {
        }
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
