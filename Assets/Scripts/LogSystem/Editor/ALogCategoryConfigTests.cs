using NUnit.Framework;

public class ALogCategoryConfigTests
{
    [Test]
    public void BuildCSharp_MapsVariableToDisplayName() {
        var data = new ALogCategoryConfigData();
        data.Items.Add(new ALogCategoryItem { DisplayName = "背包", VariableName = "Inventory" });
        data.Items.Add(new ALogCategoryItem { DisplayName = "网络", VariableName = "Net" });

        string code = ALogCategoryConfig.BuildCSharp(data);

        StringAssert.Contains("public const string Inventory = \"背包\";", code);
        StringAssert.Contains("public const string Net = \"网络\";", code);
    }

    [Test]
    public void BuildLua_MapsVariableToDisplayName() {
        var data = new ALogCategoryConfigData();
        data.Items.Add(new ALogCategoryItem { DisplayName = "背包", VariableName = "Inventory" });

        string code = ALogCategoryConfig.BuildLua(data);

        StringAssert.Contains("Inventory = \"背包\",", code);
    }

    [Test]
    public void Validate_RejectsInvalidVariableName() {
        var data = new ALogCategoryConfigData();
        data.Items.Add(new ALogCategoryItem { DisplayName = "背包", VariableName = "1Bad" });

        Assert.AreEqual("变量名非法: 1Bad", ALogCategoryConfig.Validate(data));
    }
}
