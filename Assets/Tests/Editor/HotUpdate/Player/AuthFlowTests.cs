using AChen.Player;
using NUnit.Framework;

public sealed class AuthFlowTests
{
    const string ValidPassword = "Passw0rd!";

    [TestCase("ab")]
    [TestCase("")]
    [TestCase(null)]
    [TestCase("has space")]
    [TestCase("中文名")]
    [TestCase("a_very_long_username_over_24")]
    public void Validate_RejectsInvalidUsername(string username)
    {
        Assert.AreEqual("账号需为 3-24 位英文、数字或下划线", AuthFlow.Validate(AuthMode.Login, username, ValidPassword, null));
    }

    [TestCase("short7!")]
    [TestCase(null)]
    public void Validate_RejectsPasswordLength(string password)
    {
        Assert.AreEqual("密码长度需为 8-128 位", AuthFlow.Validate(AuthMode.Login, "player_1", password, null));
    }

    [Test]
    public void Validate_Register_RejectsWeakPassword()
    {
        Assert.AreEqual("密码过弱", AuthFlow.Validate(AuthMode.Register, "player_1", "onlyletters", "onlyletters"));
    }

    [Test]
    public void Validate_Login_AllowsWeakPassword()
    {
        Assert.IsNull(AuthFlow.Validate(AuthMode.Login, "player_1", "onlyletters", null));
    }

    [Test]
    public void Validate_Register_RejectsMismatchedConfirm()
    {
        Assert.AreEqual("两次输入的密码不一致", AuthFlow.Validate(AuthMode.Register, "player_1", ValidPassword, ValidPassword + "x"));
    }

    [Test]
    public void Validate_Register_PassesWithMatchingStrongPassword()
    {
        Assert.IsNull(AuthFlow.Validate(AuthMode.Register, "player_1", ValidPassword, ValidPassword));
    }

    [TestCase("ACCOUNT_EXISTS", "该账号已被注册")]
    [TestCase("INVALID_CREDENTIALS", "账号或密码错误")]
    [TestCase("VALIDATION_ERROR", "账号或密码格式不正确")]
    [TestCase("NETWORK_ERROR", "无法连接服务器，请检查网络")]
    [TestCase("RATE_LIMITED", "操作过于频繁，请稍后再试")]
    [TestCase("SOMETHING_ELSE", "账号操作失败，请稍后再试")]
    [TestCase(null, "账号操作失败，请稍后再试")]
    public void GetErrorMessage_MapsBackendCodes(string code, string expected)
    {
        Assert.AreEqual(expected, AuthFlow.GetErrorMessage(code));
    }
}
