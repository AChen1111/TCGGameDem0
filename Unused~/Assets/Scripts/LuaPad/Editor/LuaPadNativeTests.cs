using NUnit.Framework;
using UnityEngine;

public class LuaPadNativeTests
{
    [Test]
    public void ScaleHostToClient_IdentityWhenPanelMatchesGame()
    {
        LuaPadNative.ScaleHostToClient(
            new Rect(100, 50, 200, 100),
            new Rect(10, 40, 800, 400),
            800f,
            400f,
            out int x,
            out int y,
            out int w,
            out int h);
        Assert.AreEqual(110, x);
        Assert.AreEqual(90, y);
        Assert.AreEqual(200, w);
        Assert.AreEqual(100, h);
    }

    [Test]
    public void ScaleHostToClient_ScalesWhenPanelDiffersFromGame()
    {
        LuaPadNative.ScaleHostToClient(
            new Rect(192, 108, 384, 216),
            new Rect(0, 20, 960, 540),
            1920f,
            1080f,
            out int x,
            out int y,
            out int w,
            out int h);
        Assert.AreEqual(96, x);
        Assert.AreEqual(74, y);
        Assert.AreEqual(192, w);
        Assert.AreEqual(108, h);
    }

    [Test]
    public void TryHostBounds_RejectsTinyHost()
    {
        Assert.IsFalse(LuaPadNative.TryHostBounds(
            new Rect(0, 0, 8, 8),
            new Rect(0, 0, 800, 400),
            new Rect(100, 200, 800, 400),
            out _,
            out _,
            out _,
            out _));
    }

    [Test]
    public void TryHostBounds_MapsOntoScreenRect()
    {
        Assert.IsTrue(LuaPadNative.TryHostBounds(
            new Rect(80, 40, 640, 240),
            new Rect(0, 0, 800, 400),
            new Rect(100, 200, 800, 400),
            out int x,
            out int y,
            out int w,
            out int h));
        Assert.AreEqual(180, x);
        Assert.AreEqual(240, y);
        Assert.AreEqual(640, w);
        Assert.AreEqual(240, h);
    }

    [Test]
    public void EditorFontSize_StaysConstantWhenPanelMatchesScreen()
    {
        Assert.AreEqual(18f, LuaPadNative.EditorFontSize(600f, 600f, 18f));
    }

    [Test]
    public void EditorFontSize_ScalesWithScreenRelativeToPanel()
    {
        Assert.AreEqual(9f, LuaPadNative.EditorFontSize(600f, 300f, 18f));
        Assert.AreEqual(36f, LuaPadNative.EditorFontSize(600f, 1200f, 18f));
    }
}
