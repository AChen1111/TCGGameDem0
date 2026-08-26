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
}
