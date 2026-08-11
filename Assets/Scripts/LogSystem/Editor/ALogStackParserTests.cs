using System.Collections.Generic;
using NUnit.Framework;

public class ALogStackParserTests
{
    [Test]
    public void ParseLua_ExtractsFramesInnerToOuter() {
        const string traceback =
            "\nstack traceback:\n" +
            "\t[C]: in function 'error'\n" +
            "\tD:/Proj/Assets/Scripts/LuaRaw/Log.lua:32: in function 'Throw'\n" +
            "\tD:/Proj/Assets/Scripts/LuaRaw/UI/BaseUI.lua:13: in function 'OnClicked'\n" +
            "\tD:/Proj/Assets/Scripts/LuaRaw/Main.lua:20: in main chunk";

        List<ALogFrame> frames = ALogStackParser.ParseLua(traceback);

        Assert.AreEqual(4, frames.Count);
        Assert.IsFalse(frames[0].CanJump);
        Assert.AreEqual("function 'OnClicked'", frames[2].Signature);
        Assert.AreEqual(13, frames[2].Line);
        StringAssert.EndsWith("UI/BaseUI.lua", frames[2].FilePath);
        Assert.IsTrue(frames[2].IsLua);
        Assert.AreEqual("main chunk", frames[3].Signature);
    }

    [Test]
    public void ParseLua_HandlesDoStringChunkNames() {
        const string traceback =
            "\nstack traceback:\n" +
            "\t[string \"D:/Proj/Temp/Verify.lua\"]:12: in function <[string \"D:/Proj/Temp/Verify.lua\"]:11>\n" +
            "\t[C]: in function 'pcall'";

        List<ALogFrame> frames = ALogStackParser.ParseLua(traceback);

        Assert.AreEqual(2, frames.Count);
        Assert.AreEqual("D:/Proj/Temp/Verify.lua", frames[0].FilePath);
        Assert.AreEqual(12, frames[0].Line);
        Assert.AreEqual("function <Verify.lua:11>", frames[0].Signature);
    }

    [Test]
    public void ParseLua_ReturnsEmptyWhenNoTraceback() {
        Assert.IsEmpty(ALogStackParser.ParseLua(null));
        Assert.IsEmpty(ALogStackParser.ParseLua("stack traceback:"));
    }

    [Test]
    public void ParseCSharp_ExtractsFileAndLine() {
        const string stack =
            "LuaManager.RuntimeReload (System.String typeName) (at Assets/Scripts/LuaComponet/LuaManager.cs:55)\n" +
            "UnityEngine.Debug:Log (object)";

        List<ALogFrame> frames = ALogStackParser.ParseCSharp(stack);

        Assert.AreEqual(2, frames.Count);
        Assert.AreEqual("Assets/Scripts/LuaComponet/LuaManager.cs", frames[0].FilePath);
        Assert.AreEqual(55, frames[0].Line);
        Assert.IsFalse(frames[0].IsLua);
        Assert.IsFalse(frames[1].CanJump);
    }
}
