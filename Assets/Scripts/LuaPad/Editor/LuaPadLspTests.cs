using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

public class LuaPadLspTests
{
    [Test]
    public void Complete_LogDot_ContainsInfoWarnError()
    {
        string exe = LuaPadEmmyLuaInstaller.EnsureBinary();
        string root = LuaPadWorkspace.SourceRoot;
        string scratch = LuaPadWorkspace.ScratchPath;
        File.WriteAllText(scratch, "Log.");
        try
        {
            using (var client = LuaPadLspClient.Start(exe, root))
            {
                client.DidOpen(scratch, "Log.");
                JArray items = null;
                for (int i = 0; i < 15; i++)
                {
                    Thread.Sleep(400);
                    items = client.Complete(scratch, 0, 4);
                    if (items.Any(it => (string)it["label"] == "Info"))
                    {
                        break;
                    }
                }
                var labels = items.Select(i => (string)i["label"]).ToList();
                CollectionAssert.Contains(labels, "Info");
                CollectionAssert.Contains(labels, "Warn");
                CollectionAssert.Contains(labels, "Error");
                JToken err = items.First(it =>
                {
                    string l = (string)it["label"];
                    return l == "Error" || (l != null && l.StartsWith("Error"));
                });
                JObject page = LuaPadCompletion.FromLsp(err);
                string shown = ((string)page["label"] ?? "") + ((string)page["detail"] ?? "") + ((string)page["documentation"] ?? "");
                StringAssert.Contains("strMessage", shown);
            }
        }
        finally
        {
            if (File.Exists(scratch))
            {
                File.Delete(scratch);
            }
        }
    }

    [Test]
    public void Diagnostics_UnclosedAssign_ReportsError()
    {
        string exe = LuaPadEmmyLuaInstaller.EnsureBinary();
        string root = LuaPadWorkspace.SourceRoot;
        string scratch = LuaPadWorkspace.ScratchPath;
        const string src = "local x =";
        File.WriteAllText(scratch, src);
        try
        {
            using (var client = LuaPadLspClient.Start(exe, root))
            {
                client.DidOpen(scratch, src);
                JArray diags = new JArray();
                for (int i = 0; i < 20; i++)
                {
                    Thread.Sleep(300);
                    diags = client.LatestDiagnostics(scratch);
                    if (diags.Count > 0)
                    {
                        break;
                    }
                }
                Assert.Greater(diags.Count, 0);
            }
        }
        finally
        {
            if (File.Exists(scratch))
            {
                File.Delete(scratch);
            }
        }
    }

    [Test]
    public void Complete_L_ContainsLog()
    {
        string exe = LuaPadEmmyLuaInstaller.EnsureBinary();
        string root = LuaPadWorkspace.SourceRoot;
        string scratch = LuaPadWorkspace.ScratchPath;
        File.WriteAllText(scratch, "L");
        try
        {
            using (var client = LuaPadLspClient.Start(exe, root))
            {
                client.DidOpen(scratch, "L");
                JArray items = null;
                for (int i = 0; i < 15; i++)
                {
                    Thread.Sleep(400);
                    items = client.Complete(scratch, 0, 1);
                    if (items.Any(it => (string)it["label"] == "Log"))
                    {
                        break;
                    }
                }
                var labels = items.Select(i => (string)i["label"]).ToList();
                CollectionAssert.Contains(labels, "Log");
            }
        }
        finally
        {
            if (File.Exists(scratch))
            {
                File.Delete(scratch);
            }
        }
    }

    [Test]
    public void Complete_AfterDispose_ReturnsEmpty()
    {
        string exe = LuaPadEmmyLuaInstaller.EnsureBinary();
        string root = LuaPadWorkspace.SourceRoot;
        string scratch = LuaPadWorkspace.ScratchPath;
        File.WriteAllText(scratch, "Log.");
        try
        {
            var client = LuaPadLspClient.Start(exe, root);
            client.DidOpen(scratch, "Log.");
            client.Dispose();
            JArray items = client.Complete(scratch, 0, 4);
            Assert.AreEqual(0, items.Count);
        }
        finally
        {
            if (File.Exists(scratch))
            {
                File.Delete(scratch);
            }
        }
    }

    [Test]
    public void Complete_BaseScreenColon_ContainsAddListeners()
    {
        string exe = LuaPadEmmyLuaInstaller.EnsureBinary();
        string root = LuaPadWorkspace.SourceRoot;
        string scratch = LuaPadWorkspace.ScratchPath;
        File.WriteAllText(scratch, "BaseScreen:");
        try
        {
            using (var client = LuaPadLspClient.Start(exe, root))
            {
                client.DidOpen(scratch, "BaseScreen:");
                JArray items = null;
                for (int i = 0; i < 15; i++)
                {
                    Thread.Sleep(400);
                    items = client.Complete(scratch, 0, 11);
                    if (items.Any(it => (string)it["label"] == "AddListeners"))
                    {
                        break;
                    }
                }
                var labels = items.Select(i => (string)i["label"]).ToList();
                CollectionAssert.Contains(labels, "AddListeners");
            }
        }
        finally
        {
            if (File.Exists(scratch))
            {
                File.Delete(scratch);
            }
        }
    }

    [Test]
    public void Complete_TypedGoDot_ContainsSetActive()
    {
        LuaPadEmmyApiGenerator.Generate();
        string exe = LuaPadEmmyLuaInstaller.EnsureBinary();
        string root = LuaPadWorkspace.SourceRoot;
        string scratch = LuaPadWorkspace.ScratchPath;
        const string src = "---@type UnityEngine.GameObject\nlocal go\ngo.";
        File.WriteAllText(scratch, src);
        try
        {
            using (var client = LuaPadLspClient.Start(exe, root))
            {
                client.DidOpen(scratch, src);
                JArray items = null;
                for (int i = 0; i < 15; i++)
                {
                    Thread.Sleep(400);
                    items = client.Complete(scratch, 2, 3);
                    if (items.Any(it => (string)it["label"] == "SetActive"))
                    {
                        break;
                    }
                }
                var labels = items.Select(i => (string)i["label"]).ToList();
                CollectionAssert.Contains(labels, "SetActive");
            }
        }
        finally
        {
            if (File.Exists(scratch))
            {
                File.Delete(scratch);
            }
        }
    }
}
