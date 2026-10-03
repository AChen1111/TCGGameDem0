using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI 配置：Frame Prefab 以及首次使用时才创建的界面。
/// </summary>

[CreateAssetMenu(fileName = "UISettings", menuName = "TCG/UI/UI Settings")]
public class UISettings : ScriptableObject
{
    [Tooltip("UI Frame Prefab")]
    [SerializeField] private UIFrame templateUIPrefab = null;
    [Tooltip("初始化时只登记的界面 Prefab（Panel 和 Window），首次打开时实例化")]
    [SerializeField] private List<GameObject> screensToRegister = null;

    public UIFrame TemplateUIPrefab => templateUIPrefab;
    public IReadOnlyList<GameObject> ScreensToRegister => screensToRegister;

    /// <summary>
    /// 实例化 UI Frame。默认只登记配置里的界面 Prefab，不创建界面实例。
    /// </summary>
    /// <param name="instanceAndRegisterScreens">是否登记配置中的界面 Prefab，参数名保留既有调用兼容性</param>
    /// <returns>新的 UI Frame</returns>
    public UIFrame CreateUIInstance(bool instanceAndRegisterScreens = true) {
        var newUI = Instantiate(templateUIPrefab);

        if (instanceAndRegisterScreens) {
            foreach (var screen in screensToRegister) {
                newUI.RegisterScreenPrefab(screen.name, screen);
            }
        }

        return newUI;
    }
    
    private void OnValidate() {
        List<GameObject> objectsToRemove = new List<GameObject>();
        for(int i = 0; i < screensToRegister.Count; i++) {
            var screenCtl = screensToRegister[i].GetComponent<IUIScreenController>();
            if (screenCtl == null) {
                objectsToRemove.Add(screensToRegister[i]);
            }
        }

        if (objectsToRemove.Count > 0) {
            Debug.LogError("[UISettings] Some GameObjects that were added to the Screen Prefab List didn't have ScreenControllers attached to them! Removing.");
            foreach (var obj in objectsToRemove) {
                Debug.LogError("[UISettings] Removed " + obj.name + " from " + name + " as it has no Screen Controller attached!");
                screensToRegister.Remove(obj);
            }
        }
    }        
}
