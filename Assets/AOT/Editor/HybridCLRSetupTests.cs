using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HybridCLR.Editor.Settings;
using NUnit.Framework;
using UnityEditorInternal;

public class HybridCLRSetupTests
{
    [Test]
    public void HotUpdate_Asmdef_DisablesAutoReferenced()
    {
        string json = File.ReadAllText("Assets/Scripts/HotUpdate.asmdef");
        StringAssert.Contains("\"name\": \"HotUpdate\"", json);
        StringAssert.Contains("\"autoReferenced\": false", json);
    }

    [Test]
    public void LoadDll_LivesInAotAssembly()
    {
        Assert.AreEqual("Assembly-CSharp", typeof(LoadDll).Assembly.GetName().Name);
    }

    [Test]
    public void XLua_LivesInOwnAotAssembly()
    {
        Assert.AreEqual("XLua", typeof(XLua.LuaEnv).Assembly.GetName().Name);
    }

    [Test]
    public void HotUpdateEntry_LivesInHotUpdateAssembly()
    {
        Assembly hotUpdate = AppDomain.CurrentDomain.GetAssemblies()
            .First(a => a.GetName().Name == "HotUpdate");
        Assert.IsNotNull(hotUpdate.GetType("HotUpdateEntry"));
        Assert.IsNull(hotUpdate.GetType("LoadDll"));
    }

    [Test]
    public void HybridCLRSettings_ContainsHotUpdateDefinition()
    {
        AssemblyDefinitionAsset[] defs = HybridCLRSettings.Instance.hotUpdateAssemblyDefinitions;
        Assert.IsNotNull(defs);
        Assert.IsTrue(defs.Any(d => d != null && d.name == "HotUpdate"));
    }
}
