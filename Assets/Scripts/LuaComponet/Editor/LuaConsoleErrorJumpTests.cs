using NUnit.Framework;

public class LuaConsoleErrorJumpTests
{
    [Test]
    public void TryParseLuaLocation_ExtractsMixedSlashPathAndLine()
    {
        const string log =
            "LuaException: error loading module BaseUI from CustomLoader, " +
            @"D:/GameWorkplace/Doing/AChenFrameWork/Assets/Scripts/LuaRaw/UI\BaseUI.lua:6: unexpected symbol near '1'";

        bool ok = LuaConsoleErrorJump.TryParseLuaLocation(log, out string path, out int line);

        Assert.IsTrue(ok);
        Assert.AreEqual(6, line);
        StringAssert.EndsWith("BaseUI.lua", path.Replace('\\', '/'));
    }

    [Test]
    public void TryParseLuaLocation_ReturnsFalseWhenNoLuaPath()
    {
        bool ok = LuaConsoleErrorJump.TryParseLuaLocation(
            "NullReferenceException: Object reference not set",
            out _,
            out _);

        Assert.IsFalse(ok);
    }
}
