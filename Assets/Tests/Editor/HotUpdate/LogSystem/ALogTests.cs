using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class ALogTests
{
    /// <summary>
    /// 双击控制台跳到业务代码而不是 ALog.cs,靠的就是这个特性:
    /// Unity 在解析日志位置时会跳过标了 HideInCallstack 的方法。去掉它跳转就会退回 ALog 自身。
    /// </summary>
    [TestCase("Log")]
    [TestCase("LogWarning")]
    [TestCase("LogError")]
    public void LogMethods_AreHiddenInCallstack(string methodName) {
        MethodInfo method = typeof(ALog).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);

        Assert.IsNotNull(method, $"ALog.{methodName} 不存在");
        Assert.IsTrue(method.IsDefined(typeof(HideInCallstackAttribute), false), $"ALog.{methodName} 缺少 [HideInCallstack]");
    }

    [Test]
    public void Format_PrefixesCategory() {
        Assert.AreEqual("[Network] Connection timed out", ALog.Format(ALogCategories.Net, "Connection timed out"));
    }

    [Test]
    public void LogMethods_DefaultToDefaultCategory() {
        MethodInfo method = typeof(ALog).GetMethod("Log", BindingFlags.Public | BindingFlags.Static);

        Assert.AreEqual(ALogCategories.Default, method.GetParameters()[1].DefaultValue);
    }
}
