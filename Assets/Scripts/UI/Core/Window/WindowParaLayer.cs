using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 高优先级 Window（默认弹窗）所在层，由 WindowUILayer 控制。
/// </summary>
public class WindowParaLayer : MonoBehaviour {
    [SerializeField] 
    private GameObject darkenBgObject = null;

    private List<GameObject> containedScreens = new List<GameObject>();
    
    public void AddScreen(Transform screenRectTransform) {
        screenRectTransform.SetParent(transform, false);
        containedScreens.Add(screenRectTransform.gameObject);
    }

    public void RefreshDarken() {
        // 已销毁的弹窗顺手移出列表, 避免长期运行后列表堆积空引用.
        containedScreens.RemoveAll(screen => screen == null);
        for (int i = 0; i < containedScreens.Count; i++) {
            if (containedScreens[i].activeSelf) {
                darkenBgObject.SetActive(true);
                return;
            }
        }

        darkenBgObject.SetActive(false);
    }

    public void DarkenBG() {
        if (darkenBgObject == null) {
            return;
        }

        darkenBgObject.SetActive(true);
        darkenBgObject.transform.SetAsLastSibling();
    }

    public void HideDarken() {
        if (darkenBgObject == null) {
            return;
        }

        darkenBgObject.SetActive(false);
    }
}
