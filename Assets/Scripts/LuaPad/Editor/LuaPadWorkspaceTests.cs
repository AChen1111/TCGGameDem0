using System.IO;
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
}
