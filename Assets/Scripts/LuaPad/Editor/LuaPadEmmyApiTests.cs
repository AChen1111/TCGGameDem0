using System.IO;
using NUnit.Framework;
using UnityEngine;

public class LuaPadEmmyApiTests
{
    [Test]
    public void LuaType_MapsPrimitives()
    {
        Assert.AreEqual("number", LuaPadEmmyApiGenerator.LuaType(typeof(int)));
        Assert.AreEqual("boolean", LuaPadEmmyApiGenerator.LuaType(typeof(bool)));
        Assert.AreEqual("string", LuaPadEmmyApiGenerator.LuaType(typeof(string)));
        Assert.AreEqual("UnityEngine.GameObject", LuaPadEmmyApiGenerator.LuaType(typeof(GameObject)));
    }

    [Test]
    public void Render_GameObject_HasSetActiveFindAndCtor()
    {
        string text = LuaPadEmmyApiGenerator.Render(typeof(GameObject));
        StringAssert.Contains("---@class UnityEngine.GameObject", text);
        StringAssert.Contains("SetActive", text);
        StringAssert.Contains("Find", text);
        StringAssert.Contains("---@overload fun(", text);
        StringAssert.Contains("---@alias CS.UnityEngine.GameObject UnityEngine.GameObject", text);
    }

    [Test]
    public void Generate_WritesGameObjectStub()
    {
        int n = LuaPadEmmyApiGenerator.Generate();
        Assert.Greater(n, 10);
        string path = Path.Combine(LuaPadEmmyApiGenerator.RelativeDir, "UnityEngine_GameObject.lua");
        Assert.IsTrue(File.Exists(path), path);
        StringAssert.Contains("SetActive", File.ReadAllText(path));
        string buttonPath = Path.Combine(LuaPadEmmyApiGenerator.RelativeDir, "UnityEngine_UI_Button.lua");
        Assert.IsTrue(File.Exists(buttonPath), buttonPath);
        StringAssert.Contains("---@class UnityEngine.UI.Button", File.ReadAllText(buttonPath));
    }
}
