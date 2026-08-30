using System;
using NUnit.Framework;

public class ALogConsoleToolbarTests
{
    [Test]
    public void Categories_ComeFromALogCategoriesConstants() {
        string[] categories = ALogConsoleToolbar.Categories;

        Assert.Contains(ALogCategories.Default, categories);
        Assert.Contains(ALogCategories.Net, categories);
        Assert.Contains(ALogCategories.Event, categories);
        Assert.Contains(ALogCategories.UI, categories);
    }

    [Test]
    public void ToSearchText_MatchesLogPrefix() {
        string message = ALog.Format(ALogCategories.Net, "Connection timed out");

        Assert.That(message, Does.StartWith(ALogConsoleToolbar.ToSearchText(ALogCategories.Net)));
    }

    [Test]
    public void ToSearchText_AllOrEmpty_ClearsFilter() {
        Assert.AreEqual(string.Empty, ALogConsoleToolbar.ToSearchText("All"));
        Assert.AreEqual(string.Empty, ALogConsoleToolbar.ToSearchText(string.Empty));
        Assert.AreEqual(string.Empty, ALogConsoleToolbar.ToSearchText(null));
    }

    [Test]
    public void FromSearchText_RoundTripsEveryCategory() {
        foreach (string category in ALogConsoleToolbar.Categories)
        {
            Assert.AreEqual(category, ALogConsoleToolbar.FromSearchText(ALogConsoleToolbar.ToSearchText(category)));
        }
    }

    [Test]
    public void FromSearchText_UnknownFilter_FallsBackToAll() {
        Assert.AreEqual("All", ALogConsoleToolbar.FromSearchText("unknown search"));
        Assert.AreEqual("All", ALogConsoleToolbar.FromSearchText(string.Empty));
    }

    [Test]
    public void Categories_AreUnique() {
        string[] categories = ALogConsoleToolbar.Categories;
        Assert.AreEqual(categories.Length, new System.Collections.Generic.HashSet<string>(categories, StringComparer.Ordinal).Count);
    }
}
