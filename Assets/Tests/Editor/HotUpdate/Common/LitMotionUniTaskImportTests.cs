using Cysharp.Threading.Tasks;
using LitMotion;
using NUnit.Framework;

public class LitMotionUniTaskImportTests
{
    [Test]
    public void AssembliesResolve()
    {
        Assert.IsNotNull(typeof(LMotion));
        Assert.IsNotNull(typeof(UniTask));
    }
}
