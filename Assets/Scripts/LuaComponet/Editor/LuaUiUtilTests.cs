using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class LuaUiUtilTests
{
    [Test]
    public void SetRaycasterEnabled_TogglesGraphicRaycaster()
    {
        var go = new GameObject("Canvas", typeof(Canvas), typeof(GraphicRaycaster));
        try
        {
            var raycaster = go.GetComponent<GraphicRaycaster>();
            Assert.IsTrue(raycaster.enabled);

            LuaUiUtil.SetRaycasterEnabled(go, false);
            Assert.IsFalse(raycaster.enabled);

            LuaUiUtil.SetRaycasterEnabled(go, true);
            Assert.IsTrue(raycaster.enabled);
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }
}
