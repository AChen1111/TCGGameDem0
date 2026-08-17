using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

public class LuaPadWorkspaceTests
{
    [Test]
    public void SourceRoot_ContainsIncludeAndMain()
    {
        Assert.IsTrue(File.Exists(Path.Combine(LuaPadWorkspace.SourceRoot, "Include.lua")));
        Assert.IsTrue(File.Exists(Path.Combine(LuaPadWorkspace.SourceRoot, "Main.lua")));
    }

    [Test]
    public void RuntimeRoot_InEditor_IsSourceRoot()
    {
        Assert.AreEqual(LuaPadWorkspace.SourceRoot, LuaPadWorkspace.RuntimeRoot);
    }

    [Test]
    public void CopySourceTo_WritesIncludeLua()
    {
        string dest = Path.Combine(Application.temporaryCachePath, "LuaPadWorkspaceTest");
        if (Directory.Exists(dest))
        {
            Directory.Delete(dest, true);
        }
        LuaPadWorkspace.CopySourceTo(dest);
        Assert.IsTrue(File.Exists(Path.Combine(dest, "Include.lua")));
        Assert.IsTrue(File.Exists(Path.Combine(dest, "Main.lua")));
        Assert.IsTrue(File.Exists(Path.Combine(dest, ".emmyrc.json")));
        Directory.Delete(dest, true);
    }

    [Test]
    public void MonacoDist_HasIndexHtml()
    {
        Assert.IsTrue(File.Exists(Path.Combine(Application.streamingAssetsPath, "LuaPad", "index.html")));
    }

    [Test]
    public void SanitizeDraftName_AllowsWordCharsAndAddsLua()
    {
        Assert.AreEqual("foo_bar-1.lua", LuaPadWorkspace.SanitizeDraftName("foo_bar-1"));
        Assert.AreEqual("foo.lua", LuaPadWorkspace.SanitizeDraftName("foo.lua"));
    }

    [Test]
    public void SanitizeDraftName_RejectsPathChars()
    {
        Assert.Throws<ArgumentException>(() => LuaPadWorkspace.SanitizeDraftName("../x"));
        Assert.Throws<ArgumentException>(() => LuaPadWorkspace.SanitizeDraftName("a b"));
    }

    [Test]
    public void ResolveDraftPath_StaysInsideDraftsRoot()
    {
        string root = Path.GetFullPath(LuaPadWorkspace.DraftsRoot);
        string path = LuaPadWorkspace.ResolveDraftPath("ok_name");
        Assert.IsTrue(path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
        Assert.AreEqual("ok_name.lua", Path.GetFileName(path));
    }

    [Test]
    public void SkipRuntimeScan_DraftsOnly()
    {
        Assert.IsTrue(LuaPadWorkspace.SkipRuntimeScan(@"D:\x\LuaRaw\LuaPadDrafts\a.lua"));
        Assert.IsFalse(LuaPadWorkspace.SkipRuntimeScan(@"D:\x\LuaRaw\Main.lua"));
        Assert.IsFalse(LuaPadWorkspace.SkipRuntimeScan(@"D:\x\LuaRaw\EmmyApi\GameObject.lua"));
    }

    [Test]
    public void DraftSaveLoadList_RoundTrip()
    {
        string name = "_t_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        try
        {
            LuaPadWorkspace.SaveDraft(name, "print(42)");
            CollectionAssert.Contains(LuaPadWorkspace.ListDrafts(), name);
            Assert.AreEqual("print(42)", LuaPadWorkspace.LoadDraft(name));
        }
        finally
        {
            DeleteDraft(name);
        }
    }

    [Test]
    public void HandleRpc_DraftMethods_RoundTrip()
    {
        var session = new LuaPadSession();
        string name = "_rpc_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        try
        {
            session.HandleRpc(new JObject { ["method"] = "draftSave", ["name"] = name, ["text"] = "print(7)" });
            JObject list = session.HandleRpc(new JObject { ["method"] = "draftsList" });
            CollectionAssert.Contains(list["names"].ToObject<List<string>>(), name);
            JObject load = session.HandleRpc(new JObject { ["method"] = "draftLoad", ["name"] = name });
            Assert.AreEqual("print(7)", (string)load["text"]);
        }
        finally
        {
            DeleteDraft(name);
        }
    }

    [Test]
    public void CopySourceTo_SkipsDraftsFolder()
    {
        string dest = Path.Combine(Application.temporaryCachePath, "LuaPadWorkspaceDraftSkip");
        string name = "_copy_" + Guid.NewGuid().ToString("N").Substring(0, 8);
        LuaPadWorkspace.SaveDraft(name, "print(1)");
        try
        {
            if (Directory.Exists(dest))
            {
                Directory.Delete(dest, true);
            }
            LuaPadWorkspace.CopySourceTo(dest);
            Assert.IsFalse(Directory.Exists(Path.Combine(dest, LuaPadWorkspace.DraftsFolder)));
        }
        finally
        {
            DeleteDraft(name);
            if (Directory.Exists(dest))
            {
                Directory.Delete(dest, true);
            }
        }
    }

    static void DeleteDraft(string name)
    {
        string path = LuaPadWorkspace.ResolveDraftPath(name);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        string meta = path + ".meta";
        if (File.Exists(meta))
        {
            File.Delete(meta);
        }
    }
}
