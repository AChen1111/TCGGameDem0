using UnityEngine;
using UnityEngine.UI;

public static class LuaUiUtil
{
    public static LuaUiComponent InstantiateUnder(string prefabPath, Transform parent)
    {
        var prefab = Resources.Load<GameObject>(prefabPath);
        var go = Object.Instantiate(prefab, parent, false);
        go.name = prefab.name;
        return go.GetComponent<LuaUiComponent>();
    }

    public static void SetRaycasterEnabled(GameObject canvasGo, bool enabled)
    {
        canvasGo.GetComponent<GraphicRaycaster>().enabled = enabled;
    }
}
