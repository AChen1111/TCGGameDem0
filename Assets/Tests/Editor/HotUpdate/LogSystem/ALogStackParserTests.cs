using System.Collections.Generic;
using NUnit.Framework;

public class ALogStackParserTests
{
    [Test]
    public void ParseCSharp_ExtractsFileAndLine() {
        const string stack =
            "GameManager.Reload (System.String typeName) (at Assets/Scripts/Game/GameManager.cs:55)\n" +
            "UnityEngine.Debug:Log (object)";

        List<ALogFrame> frames = ALogStackParser.ParseCSharp(stack);

        Assert.AreEqual(2, frames.Count);
        Assert.AreEqual("Assets/Scripts/Game/GameManager.cs", frames[0].FilePath);
        Assert.AreEqual(55, frames[0].Line);
        Assert.IsFalse(frames[1].CanJump);
    }

    [Test]
    public void TrimLoggingFrames_DropsForwardingFramesSoCallSiteIsFirst() {
        const string stack =
            "UnityEngine.Debug:LogError (object)\n" +
            "ALog:LogError (string,string)\n" +
            "ALog.Write () (at Assets/Scripts/LogSystem/Runtime/ALog.cs:27)\n" +
            "ShopWindow.Refresh () (at Assets/Scripts/UI/PreGameUI/Shop/ShopWindow.cs:112)";

        List<ALogFrame> frames = ALogStackParser.TrimLoggingFrames(ALogStackParser.ParseCSharp(stack));

        Assert.AreEqual(1, frames.Count);
        Assert.AreEqual("Assets/Scripts/UI/PreGameUI/Shop/ShopWindow.cs", frames[0].FilePath);
        Assert.AreEqual(112, frames[0].Line);
    }

    [Test]
    public void TrimLoggingFrames_KeepsFramesBelowTheCallSite() {
        const string stack =
            "UnityEngine.Debug:Log (object)\n" +
            "ShopWindow.Refresh () (at Assets/Scripts/UI/PreGameUI/Shop/ShopWindow.cs:112)\n" +
            "UIFrame.ShowPanel () (at Assets/Scripts/UI/UIFrame.cs:40)";

        List<ALogFrame> frames = ALogStackParser.TrimLoggingFrames(ALogStackParser.ParseCSharp(stack));

        Assert.AreEqual(2, frames.Count);
        Assert.AreEqual("Assets/Scripts/UI/UIFrame.cs", frames[1].FilePath);
    }

    [Test]
    public void TrimLoggingFrames_NoBusinessFrame_KeepsOriginal() {
        const string stack = "ALog.Log () (at Assets/Scripts/LogSystem/Runtime/ALog.cs:27)";

        List<ALogFrame> frames = ALogStackParser.TrimLoggingFrames(ALogStackParser.ParseCSharp(stack));

        Assert.AreEqual(1, frames.Count);
        Assert.AreEqual("Assets/Scripts/LogSystem/Runtime/ALog.cs", frames[0].FilePath);
    }

    [Test]
    public void TrimLoggingFrames_NoStack_IsEmpty() {
        Assert.IsEmpty(ALogStackParser.TrimLoggingFrames(ALogStackParser.ParseCSharp(string.Empty)));
    }

    [Test]
    public void IsLogSystem_MatchesBackslashPaths() {
        Assert.IsTrue(ALogStackParser.IsLogSystem(@"Assets\Scripts\LogSystem\Editor\ALogSourceJump.cs"));
        Assert.IsFalse(ALogStackParser.IsLogSystem("Assets/Scripts/Common/EventCenter.cs"));
        Assert.IsFalse(ALogStackParser.IsLogSystem(null));
    }
}
