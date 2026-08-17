using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LuaUiComponentCollectTests
{
    private GameObject m_root;

    [TearDown]
    public void TearDown()
    {
        if (m_root != null)
        {
            Object.DestroyImmediate(m_root);
        }
    }

    [Test]
    public void CollectUiBinds_BtnPrefix_AddsButtonAsConcatenatedName()
    {
        var lua = CreateHost();
        var btn = CreateChild("Btn_yy");
        btn.AddComponent<Button>();

        lua.CollectUiBinds();

        var binds = ReadBinds(lua);
        Assert.AreEqual(1, binds.Length);
        Assert.AreEqual("m_Btnyy", binds[0].luaName);
        Assert.AreEqual(btn, binds[0].target);
        Assert.AreEqual(btn.GetComponent<Button>(), binds[0].component);
    }

    [Test]
    public void CollectUiBinds_GoPrefix_BindsGameObject()
    {
        var lua = CreateHost();
        var go = CreateChild("Go_panel");

        lua.CollectUiBinds();

        var binds = ReadBinds(lua);
        Assert.AreEqual(1, binds.Length);
        Assert.AreEqual("m_Gopanel", binds[0].luaName);
        Assert.AreEqual(go, binds[0].target);
        Assert.IsNull(binds[0].component);
    }

    [Test]
    public void CollectUiBinds_TxtPrefix_BindsTmpText()
    {
        var lua = CreateHost();
        var txt = CreateChild("Txt_title");
        txt.AddComponent<TextMeshProUGUI>();

        lua.CollectUiBinds();

        var binds = ReadBinds(lua);
        Assert.AreEqual(1, binds.Length);
        Assert.AreEqual("m_Txttitle", binds[0].luaName);
        Assert.AreEqual(txt.GetComponent<TextMeshProUGUI>(), binds[0].component);
    }

    [Test]
    public void CollectUiBinds_UnknownPrefixOrMissingComponent_Skipped()
    {
        var lua = CreateHost();
        CreateChild("Foo_bar");
        CreateChild("Btn_empty");

        lua.CollectUiBinds();

        Assert.AreEqual(0, ReadBinds(lua).Length);
    }

    [Test]
    public void CollectUiBinds_NestedAndInactive_AreCollected()
    {
        var lua = CreateHost();
        var nested = CreateChild("Holder");
        var btn = new GameObject("Btn_ok", typeof(RectTransform));
        btn.transform.SetParent(nested.transform, false);
        btn.AddComponent<Button>();
        var hidden = CreateChild("Img_icon");
        hidden.AddComponent<Image>();
        hidden.SetActive(false);

        lua.CollectUiBinds();

        var names = ReadBinds(lua).Select(b => b.luaName).ToArray();
        CollectionAssert.AreEquivalent(new[] { "m_Btnok", "m_Imgicon" }, names);
    }

    [Test]
    public void CollectUiBinds_DuplicateLuaName_LastWins()
    {
        var lua = CreateHost();
        CreateChild("Btn_yy").AddComponent<Button>();
        var second = CreateChild("Btn_yy");
        second.AddComponent<Button>();

        lua.CollectUiBinds();

        var binds = ReadBinds(lua);
        Assert.AreEqual(1, binds.Length);
        Assert.AreEqual(second, binds[0].target);
    }

    [Test]
    public void CollectUiBinds_ReplacesPreviousBinds()
    {
        var lua = CreateHost();
        CreateChild("Btn_old").AddComponent<Button>();
        lua.CollectUiBinds();
        Object.DestroyImmediate(m_root.transform.Find("Btn_old").gameObject);
        CreateChild("Btn_new").AddComponent<Button>();

        lua.CollectUiBinds();

        var binds = ReadBinds(lua);
        Assert.AreEqual(1, binds.Length);
        Assert.AreEqual("m_Btnnew", binds[0].luaName);
    }

    [Test]
    public void CollectUiBinds_MapsAgreedPrefixes()
    {
        var lua = CreateHost();
        CreateChild("Btn_a").AddComponent<Button>();
        CreateChild("Img_a").AddComponent<Image>();
        CreateChild("Txt_a").AddComponent<TextMeshProUGUI>();
        CreateChild("Tog_a").AddComponent<Toggle>();
        CreateChild("Sld_a").AddComponent<Slider>();
        CreateChild("Inp_a").AddComponent<TMP_InputField>();
        CreateChild("Scr_a").AddComponent<ScrollRect>();
        CreateChild("Raw_a").AddComponent<RawImage>();
        CreateChild("Drop_a").AddComponent<TMP_Dropdown>();
        CreateChild("Go_a");

        lua.CollectUiBinds();

        CollectionAssert.AreEquivalent(
            new[] { "m_Btna", "m_Imga", "m_Txta", "m_Toga", "m_Slda", "m_Inpa", "m_Scra", "m_Rawa", "m_Dropa", "m_Goa" },
            ReadBinds(lua).Select(b => b.luaName).ToArray());
    }

    [Test]
    public void CollectUiBinds_DoesNotBindSelf()
    {
        m_root = new GameObject("Btn_host");
        var lua = m_root.AddComponent<LuaUiComponent>();
        m_root.AddComponent<Button>();

        lua.CollectUiBinds();

        Assert.AreEqual(0, ReadBinds(lua).Length);
    }

    [Test]
    public void BuildUiBindEmmyLua_Empty_ReturnsEmpty()
    {
        var lua = CreateHost();
        Assert.AreEqual("", lua.BuildUiBindEmmyLua());
    }

    [Test]
    public void BuildUiBindEmmyLua_WritesFieldAnnotations()
    {
        var lua = CreateHost();
        CreateChild("Btn_yy").AddComponent<Button>();
        CreateChild("Go_panel");
        CreateChild("Txt_title").AddComponent<TextMeshProUGUI>();
        lua.CollectUiBinds();

        Assert.AreEqual(
            "---@field m_Btnyy UnityEngine.UI.Button\n" +
            "---@field m_Gopanel UnityEngine.GameObject\n" +
            "---@field m_Txttitle TMPro.TextMeshProUGUI\n",
            lua.BuildUiBindEmmyLua());
    }

    [Test]
    public void CopyUiBindEmmyLua_WritesClipboard()
    {
        var lua = CreateHost();
        CreateChild("Btn_yy").AddComponent<Button>();
        lua.CollectUiBinds();

        lua.CopyUiBindEmmyLua();

        Assert.AreEqual("---@field m_Btnyy UnityEngine.UI.Button\n", EditorGUIUtility.systemCopyBuffer);
    }

    private LuaUiComponent CreateHost()
    {
        m_root = new GameObject("Host", typeof(RectTransform));
        return m_root.AddComponent<LuaUiComponent>();
    }

    private GameObject CreateChild(string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(m_root.transform, false);
        return go;
    }

    private static (string luaName, GameObject target, Component component)[] ReadBinds(LuaUiComponent target)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty("m_uiBinds");
        var result = new (string luaName, GameObject target, Component component)[prop.arraySize];
        for (int i = 0; i < prop.arraySize; i++)
        {
            var elem = prop.GetArrayElementAtIndex(i);
            result[i] = (
                elem.FindPropertyRelative("luaName").stringValue,
                (GameObject)elem.FindPropertyRelative("target").objectReferenceValue,
                (Component)elem.FindPropertyRelative("component").objectReferenceValue);
        }
        return result;
    }
}
