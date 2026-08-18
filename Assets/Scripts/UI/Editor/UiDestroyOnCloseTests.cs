using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class UiDestroyOnCloseTests
{
    readonly List<Object> m_owned = new List<Object>();

    [TearDown]
    public void TearDown()
    {
        for (int i = 0; i < m_owned.Count; i++)
        {
            if (m_owned[i] != null)
            {
                Object.DestroyImmediate(m_owned[i]);
            }
        }
        m_owned.Clear();
    }

    [Test]
    public void Close_DestroyOnClose_DestroysGameObject()
    {
        var panel = CreatePanel(true);
        panel.Close(false);
        Assert.IsTrue(panel == null);
    }

    [Test]
    public void Hide_DestroyOnClose_DoesNotDestroy()
    {
        var panel = CreatePanel(true);
        panel.Hide(false);
        Assert.IsFalse(panel == null);
        Assert.IsFalse(panel.gameObject.activeSelf);
    }

    [Test]
    public void WindowHide_DestroyOnClose_DoesNotDestroy()
    {
        var window = CreateWindow(true);
        window.Hide(false);
        Assert.IsFalse(window == null);
        Assert.IsFalse(window.gameObject.activeSelf);
    }

    [Test]
    public void HidePanel_DestroyOnClose_DestroysAndShowRecreates()
    {
        var frame = CreateFrame();
        var prefab = CreatePanelPrefab("DestroyOnClosePanel", true);
        frame.RegisterScreenPrefab("DestroyOnClosePanel", prefab);

        frame.ShowPanel("DestroyOnClosePanel");
        var first = Object.FindFirstObjectByType<DestroyOnCloseTestPanel>();
        Assert.IsNotNull(first);
        Assert.AreNotEqual(prefab, first.gameObject);

        frame.HidePanel("DestroyOnClosePanel");
        Assert.IsTrue(first == null);
        Assert.IsFalse(frame.IsScreenRegistered("DestroyOnClosePanel"));

        frame.ShowPanel("DestroyOnClosePanel");
        var second = Object.FindFirstObjectByType<DestroyOnCloseTestPanel>();
        Assert.IsNotNull(second);
        Assert.IsTrue(second.gameObject.activeSelf);
    }

    DestroyOnCloseTestPanel CreatePanel(bool destroyOnClose)
    {
        var go = new GameObject("DestroyOnCloseTestPanel");
        m_owned.Add(go);
        var panel = go.AddComponent<DestroyOnCloseTestPanel>();
        SetDestroyOnClose(panel, destroyOnClose);
        return panel;
    }

    DestroyOnCloseTestWindow CreateWindow(bool destroyOnClose)
    {
        var go = new GameObject("DestroyOnCloseTestWindow");
        m_owned.Add(go);
        var window = go.AddComponent<DestroyOnCloseTestWindow>();
        SetDestroyOnClose(window, destroyOnClose);
        return window;
    }

    GameObject CreatePanelPrefab(string screenId, bool destroyOnClose)
    {
        var go = new GameObject(screenId);
        m_owned.Add(go);
        var panel = go.AddComponent<DestroyOnCloseTestPanel>();
        SetDestroyOnClose(panel, destroyOnClose);
        go.SetActive(false);
        return go;
    }

    UIFrame CreateFrame()
    {
        var root = new GameObject("UIFrame", typeof(RectTransform), typeof(Canvas));
        m_owned.Add(root);
        var panelGo = new GameObject("PanelLayer", typeof(RectTransform), typeof(PanelUILayer));
        panelGo.transform.SetParent(root.transform, false);
        var windowGo = new GameObject("WindowLayer", typeof(RectTransform), typeof(WindowUILayer));
        windowGo.transform.SetParent(root.transform, false);
        BindPanelPriority(panelGo.GetComponent<PanelUILayer>(), panelGo.transform);
        var frame = root.AddComponent<UIFrame>();
        frame.Initialize();
        return frame;
    }

    static void SetDestroyOnClose(MonoBehaviour screen, bool value)
    {
        var so = new SerializedObject(screen);
        so.FindProperty("m_destroyOnClose").boolValue = value;
        so.ApplyModifiedProperties();
    }

    static void BindPanelPriority(PanelUILayer layer, Transform parent)
    {
        var so = new SerializedObject(layer);
        var para = so.FindProperty("priorityLayers").FindPropertyRelative("paraLayers");
        para.arraySize = 1;
        var elem = para.GetArrayElementAtIndex(0);
        elem.FindPropertyRelative("priority").enumValueIndex = (int)PanelPriority.None;
        elem.FindPropertyRelative("targetParent").objectReferenceValue = parent;
        so.ApplyModifiedProperties();
    }
}

public class DestroyOnCloseTestPanel : APanelController { }

public class DestroyOnCloseTestWindow : AWindowController { }
