using System.Collections.Generic;
using NUnit.Framework;

public class ALogStackGraphTests
{
    [TestCase("Assets/Scripts/UI/ShopWindow.cs", ALogFrameKind.Application)]
    [TestCase(@"assets\scripts\UI\ShopWindow.cs", ALogFrameKind.Application)]
    [TestCase("Packages/com.example/Runtime/Client.cs", ALogFrameKind.Package)]
    [TestCase("Library/PackageCache/com.cysharp.unitask/Runtime/UniTask.cs", ALogFrameKind.Package)]
    [TestCase(null, ALogFrameKind.NoSource)]
    [TestCase("External/Generated.cs", ALogFrameKind.NoSource)]
    public void Classify_UsesNormalizedSourcePath(string path, ALogFrameKind expected) {
        var frame = new ALogFrame { FilePath = path, Line = 1 };

        Assert.AreEqual(expected, ALogStackGraph.Classify(frame));
    }

    [Test]
    public void FindRootCause_PrefersInnermostApplicationFrame() {
        var expected = Frame("Application.Inner", "Assets/Scripts/Inner.cs");
        var frames = new List<ALogFrame>
        {
            expected,
            Frame("Package.Middle", "Library/PackageCache/com.example/Middle.cs"),
            Frame("Application.Outer", "Assets/Scripts/Outer.cs"),
        };

        Assert.AreSame(expected, ALogStackGraph.FindRootCause(frames));
    }

    [Test]
    public void FindRootCause_FallsBackToInnermostJumpableFrame() {
        var expected = Frame("Package.Inner", "Packages/com.example/Inner.cs");
        var frames = new List<ALogFrame>
        {
            new ALogFrame { Signature = "Engine.Inner" },
            expected,
            Frame("Package.Outer", "Packages/com.example/Outer.cs"),
        };

        Assert.AreSame(expected, ALogStackGraph.FindRootCause(frames));
    }

    [Test]
    public void FindRootCause_NoJumpableFrame_ReturnsInnermostFrame() {
        var expected = new ALogFrame { Signature = "Engine.Inner" };
        var frames = new List<ALogFrame>
        {
            expected,
            new ALogFrame { Signature = "Engine.Outer" },
        };

        Assert.AreSame(expected, ALogStackGraph.FindRootCause(frames));
        Assert.IsNull(ALogStackGraph.FindRootCause(new List<ALogFrame>()));
    }

    [Test]
    public void MatchesFilter_SeparatesApplicationPackageAndNoSourceFrames() {
        var application = Frame("App", "Assets/Scripts/App.cs");
        var package = Frame("Package", "Packages/com.example/Package.cs");
        var noSource = new ALogFrame { Signature = "Engine" };

        Assert.IsTrue(ALogStackGraph.MatchesFilter(application, ALogFrameFilter.Application));
        Assert.IsTrue(ALogStackGraph.MatchesFilter(package, ALogFrameFilter.Package));
        Assert.IsTrue(ALogStackGraph.MatchesFilter(noSource, ALogFrameFilter.NoSource));
        Assert.IsTrue(ALogStackGraph.MatchesFilter(application, ALogFrameFilter.All));
        Assert.IsFalse(ALogStackGraph.MatchesFilter(package, ALogFrameFilter.Application));
    }

    [Test]
    public void Graph_FilterUpdatesVisibleAndTotalCounts() {
        var graph = new ALogStackGraph();
        graph.SetFrames(new List<ALogFrame>
        {
            Frame("App", "Assets/Scripts/App.cs"),
            Frame("Package", "Packages/com.example/Package.cs"),
            new ALogFrame { Signature = "Engine" },
        });

        graph.SetFilter(ALogFrameFilter.Application);

        Assert.AreEqual(1, graph.VisibleCount);
        Assert.AreEqual(3, graph.TotalCount);
    }

    [Test]
    public void SplitTitle_SupportsChineseAndEnglishSeparators() {
        ALogStackGraphWindow.SplitTitle("[UI] Shop configuration refresh failed: Cannot connect to destination host", out string summary, out string details);

        Assert.AreEqual("[UI] Shop configuration refresh failed", summary);
        Assert.AreEqual("Cannot connect to destination host", details);

        ALogStackGraphWindow.SplitTitle("Refresh failed：Network unavailable", out summary, out details);
        Assert.AreEqual("Refresh failed", summary);
        Assert.AreEqual("Network unavailable", details);
    }

    [Test]
    public void BuildCopyText_UsesDisplayedCallOrder() {
        var frames = new List<ALogFrame>
        {
            Frame("Inner.Call", "Assets/Scripts/Inner.cs"),
            Frame("Outer.Call", "Assets/Scripts/Outer.cs"),
        };

        string text = ALogStackGraphWindow.BuildCopyText("Failed", frames);

        Assert.That(text, Does.StartWith("Failed"));
        Assert.That(text.IndexOf("1. Outer.Call", System.StringComparison.Ordinal),
            Is.LessThan(text.IndexOf("2. Inner.Call", System.StringComparison.Ordinal)));
        Assert.That(text, Does.Contain("Assets/Scripts/Inner.cs:1"));
    }

    private static ALogFrame Frame(string signature, string path) {
        return new ALogFrame
        {
            Signature = signature,
            FilePath = path,
            Line = 1,
        };
    }
}
