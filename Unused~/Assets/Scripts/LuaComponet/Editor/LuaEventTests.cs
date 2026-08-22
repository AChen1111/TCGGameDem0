using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using XLua;

public class LuaEventTests
{
    LuaEnv m_env;

    [SetUp]
    public void SetUp()
    {
        m_env = new LuaEnv();
        string root = Application.dataPath + "/Scripts/LuaRaw/";
        m_env.AddLoader((ref string filepath) =>
        {
            string name = filepath;
            int lastDot = name.LastIndexOf('.');
            if (lastDot >= 0)
            {
                name = name.Substring(lastDot + 1);
            }
            string[] files = Directory.GetFiles(root, name + ".lua", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                return null;
            }
            filepath = files[0];
            return ReadLuaBytes(files[0]);
        });
        m_env.DoString("require 'Event'");
        m_env.DoString("require 'BaseScreen'");
    }

    [TearDown]
    public void TearDown()
    {
        m_env.Dispose();
    }

    [Test]
    public void Dispatch_CallsListenerWithTargetAndArgs()
    {
        AssertLua(@"
local target = {}
local nGot = 0
function target.OnGold(self, nGold)
    nGot = nGold
end
Event.Add('Gold', target, target.OnGold)
Event.Dispatch('Gold', 42)
assert(nGot == 42)
");
    }

    [Test]
    public void Remove_StopsDelivery()
    {
        AssertLua(@"
local target = {}
local nCount = 0
function target.OnEvt(self)
    nCount = nCount + 1
end
Event.Add('E', target, target.OnEvt)
Event.Remove('E', target, target.OnEvt)
Event.Dispatch('E')
assert(nCount == 0)
");
    }

    [Test]
    public void RemoveByTarget_UnbindsAll()
    {
        AssertLua(@"
local target = {}
local nCount = 0
function target.OnA(self)
    nCount = nCount + 1
end
function target.OnB(self)
    nCount = nCount + 10
end
Event.Add('A', target, target.OnA)
Event.Add('B', target, target.OnB)
Event.RemoveByTarget(target)
Event.Dispatch('A')
Event.Dispatch('B')
assert(nCount == 0)
");
    }

    [Test]
    public void Dispatch_SnapshotSurvivesRemoveDuringCallback()
    {
        AssertLua(@"
local a, b = {}, {}
local n = 0
function a.On(self)
    n = n + 1
    Event.RemoveByTarget(b)
end
function b.On(self)
    n = n + 10
end
Event.Add('E', a, a.On)
Event.Add('E', b, b.On)
Event.Dispatch('E')
assert(n == 11)
");
    }

    [Test]
    public void BaseScreen_OnDestroy_RemovesByTarget()
    {
        AssertLua(@"
local screen = setmetatable({}, BaseScreen)
local nCount = 0
function screen.OnEvt(self)
    nCount = nCount + 1
end
Event.Add('E', screen, screen.OnEvt)
screen:OnDestroy()
Event.Dispatch('E')
assert(nCount == 0)
");
    }

    void AssertLua(string chunk)
    {
        m_env.DoString(chunk);
    }

    static byte[] ReadLuaBytes(string fullPath)
    {
        byte[] bytes = File.ReadAllBytes(fullPath);
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            var sliced = new byte[bytes.Length - 3];
            Buffer.BlockCopy(bytes, 3, sliced, 0, sliced.Length);
            return sliced;
        }
        return bytes;
    }
}
