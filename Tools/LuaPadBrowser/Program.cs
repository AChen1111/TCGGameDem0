using System.Net.Sockets;
using System.Text;
using Microsoft.Web.WebView2.WinForms;

namespace LuaPadBrowser;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        string url = "http://127.0.0.1/";
        int port = 0;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--url" && i + 1 < args.Length)
            {
                url = args[++i];
            }
            else if (args[i] == "--port" && i + 1 < args.Length)
            {
                port = int.Parse(args[++i]);
            }
        }

        ApplicationConfiguration.Initialize();
        var form = new Form
        {
            Text = "Lua Pad",
            FormBorderStyle = FormBorderStyle.Sizable,
            ShowInTaskbar = true,
            StartPosition = FormStartPosition.CenterScreen,
            Size = new Size(1100, 720),
            MinimumSize = new Size(640, 400),
            BackColor = Color.FromArgb(30, 30, 30),
        };
        var web = new WebView2 { Dock = DockStyle.Fill };
        form.Controls.Add(web);

        var client = new TcpClient();
        client.Connect("127.0.0.1", port);
        var stream = client.GetStream();
        var reader = new StreamReader(stream, Encoding.UTF8);
        var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        bool allowQuit = false;

        void Send(string line)
        {
            try
            {
                lock (writer)
                {
                    writer.WriteLine(line);
                }
            }
            catch
            {
            }
        }

        form.FormClosing += (_, e) =>
        {
            if (allowQuit)
            {
                return;
            }
            e.Cancel = true;
            form.Hide();
            Send("{\"method\":\"close\"}");
        };

        form.Load += async (_, _) =>
        {
            form.Hide();
            try
            {
                await web.EnsureCoreWebView2Async();
            }
            catch (Exception e)
            {
                Send("{\"event\":\"error\",\"message\":\"WebView2 Runtime 未安装: " + Escape(e.Message) + "\"}");
                return;
            }
            web.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            web.CoreWebView2.Settings.IsStatusBarEnabled = false;
            web.CoreWebView2.WebMessageReceived += (_, e) =>
            {
                Send(e.TryGetWebMessageAsString());
            };
            web.CoreWebView2.Navigate(url);
        };

        var pump = new Thread(() =>
        {
            while (true)
            {
                string? line;
                try
                {
                    line = reader.ReadLine();
                }
                catch
                {
                    break;
                }
                if (line == null)
                {
                    break;
                }
                form.BeginInvoke(new Action(async () =>
                {
                    var cmd = System.Text.Json.JsonDocument.Parse(line).RootElement;
                    string name = cmd.GetProperty("cmd").GetString() ?? "";
                    if (name == "quit")
                    {
                        allowQuit = true;
                        form.Close();
                        return;
                    }
                    if (name == "visible")
                    {
                        if (cmd.GetProperty("value").GetBoolean())
                        {
                            form.Show();
                            form.Activate();
                            web.Focus();
                        }
                        else
                        {
                            form.Hide();
                        }
                        return;
                    }
                    if (name == "eval" && web.CoreWebView2 != null)
                    {
                        string id = cmd.GetProperty("id").GetString() ?? "";
                        string js = cmd.GetProperty("js").GetString() ?? "";
                        string result = await web.ExecuteScriptAsync(js);
                        Send("{\"event\":\"eval\",\"id\":\"" + Escape(id) + "\",\"value\":" + result + "}");
                    }
                }));
            }
            try
            {
                form.BeginInvoke(new Action(() =>
                {
                    allowQuit = true;
                    form.Close();
                }));
            }
            catch
            {
            }
        })
        { IsBackground = true };
        pump.Start();

        Application.Run(form);
    }

    static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
