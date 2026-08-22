using NUnit.Framework;
using XLua;

public class LuaPadRunnerTests
{
    [Test]
    public void Run_PrintsOk_CapturesOutput()
    {
        var env = new LuaEnv();
        try
        {
            LuaPadRunResult result = LuaPadRunner.Run("print('ok')", chunk => env.DoString(chunk));
            Assert.IsTrue(result.Success);
            StringAssert.Contains("ok", result.Output);
        }
        finally
        {
            env.Dispose();
        }
    }

    [Test]
    public void Run_SyntaxError_ReturnsFailure()
    {
        var env = new LuaEnv();
        try
        {
            LuaPadRunResult result = LuaPadRunner.Run("local x =", chunk => env.DoString(chunk));
            Assert.IsFalse(result.Success);
            Assert.IsNotEmpty(result.Error);
        }
        finally
        {
            env.Dispose();
        }
    }

    [Test]
    public void RunInGame_WhenExistingLuaNotDone_DoesNotDoubleInit()
    {
        foreach (LuaManager m in UnityEngine.Object.FindObjectsByType<LuaManager>(
                     UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None))
        {
            UnityEngine.Object.DestroyImmediate(m.gameObject);
        }
        var go = new UnityEngine.GameObject("LuaManagerPending");
        go.AddComponent<LuaManager>();
        try
        {
            LuaPadRunResult result = LuaPadRunner.RunInGame("print('x')");
            Assert.IsFalse(result.Success);
            StringAssert.Contains("正在初始化", result.Error);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
