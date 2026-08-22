using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LuaComponetBinderTests
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
    public void FindInTree_CollectsWholeTreeIncludingInactiveAndSubclasses()
    {
        m_root = new GameObject("Root");
        m_root.AddComponent<LuaComponet>();

        var childA = new GameObject("ChildA");
        childA.transform.SetParent(m_root.transform);
        childA.AddComponent<LuaUiComponent>();

        var grandchild = new GameObject("Button");
        grandchild.transform.SetParent(childA.transform);

        var childB = new GameObject("ChildB");
        childB.transform.SetParent(m_root.transform);
        childB.AddComponent<LuaComponet>();
        childB.SetActive(false);

        var childC = new GameObject("ChildC");
        childC.transform.SetParent(m_root.transform);
        childC.AddComponent<LuaComponet>();

        var found = LuaComponetBinder.FindInTree(grandchild);

        Assert.AreEqual(4, found.Length);
        CollectionAssert.AreEquivalent(
            new[] { "Root", "ChildA", "ChildB", "ChildC" },
            found.Select(c => c.gameObject.name).ToArray());
    }

    [Test]
    public void DefaultName_GameObject_UsesObjectName()
    {
        m_root = new GameObject("Button");
        Assert.AreEqual("m_Button", LuaComponetBinder.DefaultName(m_root));
    }

    [Test]
    public void DefaultName_Component_UsesObjectAndTypeName()
    {
        m_root = new GameObject("Button", typeof(RectTransform));
        var button = m_root.AddComponent<Button>();
        Assert.AreEqual("m_Button_Button", LuaComponetBinder.DefaultName(button));
    }

    [Test]
    public void AddReference_WritesObjectReferences()
    {
        m_root = new GameObject("Host");
        var lua = m_root.AddComponent<LuaComponet>();
        var button = new GameObject("Button");
        button.transform.SetParent(m_root.transform);

        string written = LuaComponetBinder.AddReference(lua, "m_Button", button);

        Assert.AreEqual("m_Button", written);
        var refs = ReadRefs(lua);
        Assert.AreEqual(1, refs.Length);
        Assert.AreEqual("m_Button", refs[0].name);
        Assert.AreEqual(button, refs[0].value);
    }

    [Test]
    public void AddReference_LuaUiComponent_WritesObjectReferencesNotUiBinds()
    {
        m_root = new GameObject("Host");
        var lua = m_root.AddComponent<LuaUiComponent>();
        var button = m_root.AddComponent<Button>();

        LuaComponetBinder.AddReference(lua, "m_Host_Button", button);

        var refs = ReadRefs(lua);
        Assert.AreEqual(1, refs.Length);
        Assert.AreEqual("m_Host_Button", refs[0].name);
        Assert.AreEqual(button, refs[0].value);
        Assert.AreEqual(0, ReadUiBindCount(lua));
    }

    [Test]
    public void AddReference_DuplicateName_AppendsNumericSuffix()
    {
        m_root = new GameObject("Host");
        var lua = m_root.AddComponent<LuaComponet>();
        var a = new GameObject("A");
        a.transform.SetParent(m_root.transform);
        var b = new GameObject("B");
        b.transform.SetParent(m_root.transform);
        var c = new GameObject("C");
        c.transform.SetParent(m_root.transform);

        Assert.AreEqual("m_Button", LuaComponetBinder.AddReference(lua, "m_Button", a));
        Assert.AreEqual("m_Button_1", LuaComponetBinder.AddReference(lua, "m_Button", b));
        Assert.AreEqual("m_Button_2", LuaComponetBinder.AddReference(lua, "m_Button", c));

        var refs = ReadRefs(lua);
        Assert.AreEqual(3, refs.Length);
        Assert.AreEqual("m_Button", refs[0].name);
        Assert.AreEqual(a, refs[0].value);
        Assert.AreEqual("m_Button_1", refs[1].name);
        Assert.AreEqual(b, refs[1].value);
        Assert.AreEqual("m_Button_2", refs[2].name);
        Assert.AreEqual(c, refs[2].value);
    }

    private static (string name, Object value)[] ReadRefs(LuaComponet target)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty("m_objectReferences");
        var result = new (string name, Object value)[prop.arraySize];
        for (int i = 0; i < prop.arraySize; i++)
        {
            var elem = prop.GetArrayElementAtIndex(i);
            result[i] = (
                elem.FindPropertyRelative("name").stringValue,
                elem.FindPropertyRelative("value").objectReferenceValue);
        }
        return result;
    }

    private static int ReadUiBindCount(LuaUiComponent target)
    {
        return new SerializedObject(target).FindProperty("m_uiBinds").arraySize;
    }
}
