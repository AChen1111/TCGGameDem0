using AChen.Networking;
using NUnit.Framework;

public sealed class AuthSessionStoreTests
{
    PlayerPrefsAuthSessionStore m_first;
    PlayerPrefsAuthSessionStore m_second;

    [SetUp]
    public void SetUp()
    {
        m_first = new PlayerPrefsAuthSessionStore(
            new BackendConfig("https://auth-session-one.example.test"));
        m_second = new PlayerPrefsAuthSessionStore(
            new BackendConfig("https://auth-session-two.example.test"));
        m_first.Clear();
        m_second.Clear();
    }

    [TearDown]
    public void TearDown()
    {
        m_first.Clear();
        m_second.Clear();
    }

    [Test]
    public void Store_persists_refresh_token_and_clear_removes_it()
    {
        m_first.Save("refresh-token-one");

        Assert.IsTrue(m_first.TryLoad(out string refreshToken));
        Assert.AreEqual("refresh-token-one", refreshToken);

        m_first.Clear();

        Assert.IsFalse(m_first.TryLoad(out _));
    }

    [Test]
    public void Store_isolates_sessions_by_backend()
    {
        m_first.Save("refresh-token-one");

        Assert.IsFalse(m_second.TryLoad(out _));
    }
}
