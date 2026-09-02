using System.IO;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UiScreenGeneratorTests
{
    private GameObject m_root;
    private string m_writtenPath;

    [TearDown]
    public void TearDown()
    {
        if (m_root != null)
        {
            Object.DestroyImmediate(m_root);
        }
        if (!string.IsNullOrEmpty(m_writtenPath) && File.Exists(m_writtenPath))
        {
            File.Delete(m_writtenPath);
            string meta = m_writtenPath + ".meta";
            if (File.Exists(meta))
            {
                File.Delete(meta);
            }
        }
    }

    [Test]
    public void CollectUiBinds_BtnPrefix_AddsButtonAsConcatenatedName()
    {
        var gen = CreateGenerator();
        var btn = CreateChild("Btn_yy");
        btn.AddComponent<Button>();

        gen.CollectUiBinds();

        var binds = ReadBinds(gen);
        Assert.AreEqual(1, binds.Length);
        Assert.AreEqual("m_Btnyy", binds[0].fieldName);
        Assert.AreEqual(btn, binds[0].target);
        Assert.AreEqual(btn.GetComponent<Button>(), binds[0].component);
    }

    [Test]
    public void CollectUiBinds_GoPrefix_BindsGameObject()
    {
        var gen = CreateGenerator();
        var go = CreateChild("Go_panel");

        gen.CollectUiBinds();

        var binds = ReadBinds(gen);
        Assert.AreEqual(1, binds.Length);
        Assert.AreEqual("m_Gopanel", binds[0].fieldName);
        Assert.AreEqual(go, binds[0].target);
        Assert.IsNull(binds[0].component);
    }

    [Test]
    public void CollectUiBinds_TxtPrefix_BindsTmpText()
    {
        var gen = CreateGenerator();
        var txt = CreateChild("Txt_title");
        txt.AddComponent<TextMeshProUGUI>();

        gen.CollectUiBinds();

        Assert.AreEqual(txt.GetComponent<TextMeshProUGUI>(), ReadBinds(gen)[0].component);
    }

    [Test]
    public void CollectUiBinds_UnknownPrefixOrMissingComponent_Skipped()
    {
        var gen = CreateGenerator();
        CreateChild("Foo_bar");
        CreateChild("Btn_empty");

        gen.CollectUiBinds();

        Assert.AreEqual(0, ReadBinds(gen).Length);
    }

    [Test]
    public void CollectUiBinds_NestedAndInactive_AreCollected()
    {
        var gen = CreateGenerator();
        var nested = CreateChild("Holder");
        var btn = new GameObject("Btn_ok", typeof(RectTransform));
        btn.transform.SetParent(nested.transform, false);
        btn.AddComponent<Button>();
        var hidden = CreateChild("Img_icon");
        hidden.AddComponent<Image>();
        hidden.SetActive(false);

        gen.CollectUiBinds();

        CollectionAssert.AreEquivalent(
            new[] { "m_Btnok", "m_Imgicon" },
            ReadBinds(gen).Select(b => b.fieldName).ToArray());
    }

    [Test]
    public void CollectUiBinds_DuplicateFieldName_LastWins()
    {
        var gen = CreateGenerator();
        CreateChild("Btn_yy").AddComponent<Button>();
        var second = CreateChild("Btn_yy");
        second.AddComponent<Button>();

        gen.CollectUiBinds();

        var binds = ReadBinds(gen);
        Assert.AreEqual(1, binds.Length);
        Assert.AreEqual(second, binds[0].target);
    }

    [Test]
    public void CollectUiBinds_ReplacesPreviousBinds()
    {
        var gen = CreateGenerator();
        CreateChild("Btn_old").AddComponent<Button>();
        gen.CollectUiBinds();
        Object.DestroyImmediate(m_root.transform.Find("Btn_old").gameObject);
        CreateChild("Btn_new").AddComponent<Button>();

        gen.CollectUiBinds();

        Assert.AreEqual("m_Btnnew", ReadBinds(gen)[0].fieldName);
    }

    [Test]
    public void CollectUiBinds_MapsAgreedPrefixes()
    {
        var gen = CreateGenerator();
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

        gen.CollectUiBinds();

        CollectionAssert.AreEquivalent(
            new[] { "m_Btna", "m_Imga", "m_Txta", "m_Toga", "m_Slda", "m_Inpa", "m_Scra", "m_Rawa", "m_Dropa", "m_Goa" },
            ReadBinds(gen).Select(b => b.fieldName).ToArray());
    }

    [Test]
    public void CollectUiBinds_DoesNotBindSelf()
    {
        m_root = new GameObject("Btn_host");
        var gen = m_root.AddComponent<UiScreenGenerator>();
        m_root.AddComponent<Button>();

        gen.CollectUiBinds();

        Assert.AreEqual(0, ReadBinds(gen).Length);
    }

    [Test]
    public void CollectUiBinds_AssignsFieldsOnSiblingScreen()
    {
        var gen = CreateGenerator();
        var panel = m_root.AddComponent<CollectTestPanel>();
        var btn = CreateChild("Btn_yy");
        btn.AddComponent<Button>();

        gen.CollectUiBinds();

        Assert.AreEqual(btn.GetComponent<Button>(), ReadField(panel, "m_Btnyy"));
    }

    [Test]
    public void BuildCsSource_Panel_WritesFieldsAndBaseType()
    {
        var gen = CreateGenerator();
        CreateChild("Btn_yy").AddComponent<Button>();
        CreateChild("Go_panel");
        SetAuthoring(gen, UiScreenGenerator.Kind.Panel, "PreGamePanel", "Assets/Scripts/UI");
        gen.CollectUiBinds();

        Assert.AreEqual(
            "using UnityEngine;\n" +
            "using UnityEngine.UI;\n\n" +
            "public class PreGamePanel : APanelController\n" +
            "{\n" +
            "    // --tag_start: 自动生成--\n" +
            "    [SerializeField] Button m_Btnyy;\n" +
            "    [SerializeField] GameObject m_Gopanel;\n" +
            "    // --tag_end: 自动生成--\n" +
            "}\n",
            gen.BuildCsSource());
    }

    [Test]
    public void BuildCsSource_Window_UsesWindowBaseAndTmpUsing()
    {
        var gen = CreateGenerator();
        CreateChild("Txt_title").AddComponent<TextMeshProUGUI>();
        SetAuthoring(gen, UiScreenGenerator.Kind.Window, "ShopWindow", "Assets/Scripts/UI");
        gen.CollectUiBinds();

        Assert.AreEqual(
            "using UnityEngine;\n" +
            "using TMPro;\n\n" +
            "public class ShopWindow : AWindowController\n" +
            "{\n" +
            "    // --tag_start: 自动生成--\n" +
            "    [SerializeField] TextMeshProUGUI m_Txttitle;\n" +
            "    // --tag_end: 自动生成--\n" +
            "}\n",
            gen.BuildCsSource());
    }

    [Test]
    public void ReplaceGeneratedFields_UpdatesOnlyTaggedRegion()
    {
        string source =
            "using UnityEngine;\n" +
            "using UnityEngine.UI;\n\n" +
            "public class PreGamePanel : APanelController\n" +
            "{\n" +
            "    // --tag_start: 自动生成--\n" +
            "    [SerializeField] Button m_BtnOld;\n" +
            "    // --tag_end: 自动生成--\n\n" +
            "    protected override void OnOpen()\n" +
            "    {\n" +
            "        KeepMe();\n" +
            "    }\n" +
            "}\n";

        string result = UiScreenGenerator.ReplaceGeneratedFields(source, new[]
        {
            "[SerializeField] Button m_BtnPlay;",
            "[SerializeField] Button m_BtnDeck;"
        });

        Assert.IsTrue(result.Contains("[SerializeField] Button m_BtnPlay;"));
        Assert.IsTrue(result.Contains("[SerializeField] Button m_BtnDeck;"));
        Assert.IsFalse(result.Contains("m_BtnOld"));
        Assert.IsTrue(result.Contains("KeepMe();"));
    }

    [Test]
    public void ReplaceGeneratedFields_InsertsTagsWhenMissing()
    {
        string source =
            "using UnityEngine;\n\n" +
            "public class PreGamePanel : APanelController\n" +
            "{\n" +
            "    protected override void OnOpen() { }\n" +
            "}\n";

        string result = UiScreenGenerator.ReplaceGeneratedFields(source, new[]
        {
            "[SerializeField] Button m_BtnPlay;"
        });

        StringAssert.Contains("// --tag_start: 自动生成--", result);
        StringAssert.Contains("[SerializeField] Button m_BtnPlay;", result);
        StringAssert.Contains("protected override void OnOpen() { }", result);
    }

    [Test]
    public void EnsureUsings_AddsMissingUiAndTmp()
    {
        string source = "using UnityEngine;\n\npublic class A : APanelController\n{\n}\n";
        string result = UiScreenGenerator.EnsureUsings(source, true, true);
        StringAssert.Contains("using UnityEngine.UI;", result);
        StringAssert.Contains("using TMPro;", result);
    }

    [Test]
    public void CreateUiScript_WritesFileAtFolderAndName()
    {
        var gen = CreateGenerator();
        CreateChild("Btn_yy").AddComponent<Button>();
        string folder = "Assets/Scripts/UI";
        SetAuthoring(gen, UiScreenGenerator.Kind.Panel, "UiScreenGeneratorTestPanel", folder);
        gen.CollectUiBinds();
        m_writtenPath = folder + "/UiScreenGeneratorTestPanel.cs";

        gen.CreateUiScript();

        Assert.IsTrue(File.Exists(m_writtenPath));
        Assert.AreEqual(gen.BuildCsSource(), File.ReadAllText(m_writtenPath));
    }

    [Test]
    public void CreateUiScript_WhenFileExists_ReplacesOnlyTaggedFields()
    {
        string folder = "Assets/Scripts/UI";
        m_writtenPath = folder + "/UiScreenGeneratorTestPanel.cs";
        File.WriteAllText(m_writtenPath,
            "using UnityEngine;\n" +
            "using UnityEngine.UI;\n\n" +
            "public class UiScreenGeneratorTestPanel : APanelController\n" +
            "{\n" +
            "    // --tag_start: 自动生成--\n" +
            "    [SerializeField] Button m_BtnOld;\n" +
            "    // --tag_end: 自动生成--\n\n" +
            "    protected override void OnOpen()\n" +
            "    {\n" +
            "        KeepMe();\n" +
            "    }\n" +
            "}\n");

        var gen = CreateGenerator();
        CreateChild("Btn_Play").AddComponent<Button>();
        SetAuthoring(gen, UiScreenGenerator.Kind.Panel, "UiScreenGeneratorTestPanel", folder);
        gen.CollectUiBinds();
        gen.CreateUiScript();

        string text = File.ReadAllText(m_writtenPath);
        StringAssert.Contains("[SerializeField] Button m_BtnPlay;", text);
        Assert.IsFalse(text.Contains("m_BtnOld"));
        StringAssert.Contains("KeepMe();", text);
    }

    [Test]
    public void TryAttachAndBind_AddsMissingComponentAndAssignsFields()
    {
        var gen = CreateGenerator();
        var btn = CreateChild("Btn_yy");
        btn.AddComponent<Button>();
        SetAuthoring(gen, UiScreenGenerator.Kind.Panel, "CollectTestPanel", "Assets/Scripts/UI");
        gen.CollectUiBinds();

        Assert.IsTrue(gen.TryAttachAndBind());
        var panel = m_root.GetComponent<CollectTestPanel>();
        Assert.IsNotNull(panel);
        Assert.AreEqual(btn.GetComponent<Button>(), ReadField(panel, "m_Btnyy"));
    }

    [Test]
    public void TryAttachAndBind_DoesNotDuplicateExistingComponent()
    {
        var gen = CreateGenerator();
        var panel = m_root.AddComponent<CollectTestPanel>();
        var btn = CreateChild("Btn_yy");
        btn.AddComponent<Button>();
        SetAuthoring(gen, UiScreenGenerator.Kind.Panel, "CollectTestPanel", "Assets/Scripts/UI");
        gen.CollectUiBinds();

        Assert.IsTrue(gen.TryAttachAndBind());
        Assert.AreEqual(1, m_root.GetComponents<CollectTestPanel>().Length);
        Assert.AreEqual(btn.GetComponent<Button>(), ReadField(panel, "m_Btnyy"));
    }

    [Test]
    public void TryAttachAndBind_UnknownType_ReturnsFalse()
    {
        var gen = CreateGenerator();
        SetAuthoring(gen, UiScreenGenerator.Kind.Panel, "DoesNotExistPanel", "Assets/Scripts/UI");

        Assert.IsFalse(gen.TryAttachAndBind());
        Assert.IsNull(m_root.GetComponent<CollectTestPanel>());
    }

    [Test]
    public void RebuildUiBinds_CollectsWritesAndBindsSiblingScreen()
    {
        var gen = CreateGenerator();
        var panel = m_root.AddComponent<CollectTestPanel>();
        var btn = CreateChild("Btn_yy");
        btn.AddComponent<Button>();
        string folder = "Assets/Scripts/UI";
        SetAuthoring(gen, UiScreenGenerator.Kind.Panel, "UiScreenGeneratorRebuildPanel", folder);
        m_writtenPath = folder + "/UiScreenGeneratorRebuildPanel.cs";

        gen.RebuildUiBinds();

        Assert.IsTrue(File.Exists(m_writtenPath));
        Assert.AreEqual(gen.BuildCsSource(), File.ReadAllText(m_writtenPath));
        Assert.AreEqual(btn.GetComponent<Button>(), ReadField(panel, "m_Btnyy"));
    }

    private UiScreenGenerator CreateGenerator()
    {
        m_root = new GameObject("Host", typeof(RectTransform));
        return m_root.AddComponent<UiScreenGenerator>();
    }

    private GameObject CreateChild(string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(m_root.transform, false);
        return go;
    }

    private static void SetAuthoring(UiScreenGenerator gen, UiScreenGenerator.Kind kind, string className, string folder)
    {
        var so = new SerializedObject(gen);
        so.FindProperty("m_kind").enumValueIndex = (int)kind;
        so.FindProperty("m_className").stringValue = className;
        so.FindProperty("m_folderPath").stringValue = folder;
        so.ApplyModifiedProperties();
    }

    private static (string fieldName, GameObject target, Component component)[] ReadBinds(MonoBehaviour target)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty("m_uiBinds");
        var result = new (string fieldName, GameObject target, Component component)[prop.arraySize];
        for (int i = 0; i < prop.arraySize; i++)
        {
            var elem = prop.GetArrayElementAtIndex(i);
            result[i] = (
                elem.FindPropertyRelative("fieldName").stringValue,
                (GameObject)elem.FindPropertyRelative("target").objectReferenceValue,
                (Component)elem.FindPropertyRelative("component").objectReferenceValue);
        }
        return result;
    }

    private static UnityEngine.Object ReadField(MonoBehaviour target, string field)
    {
        return new SerializedObject(target).FindProperty(field).objectReferenceValue;
    }
}

public class CollectTestPanel : APanelController
{
    [SerializeField] Button m_Btnyy;
    [SerializeField] GameObject m_Gopanel;
}
