#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using AChen.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class ActivityConfigBuilder
{
    public const string SourceRoot = "ActivityTableData", Root = "Assets/ActivityConfiguration";
    public const string ManifestPath = Root + "/generated.json";
    public static string SourceHash()
    {
        var files = Directory.GetFiles(SourceRoot, "*.csv", SearchOption.AllDirectories).OrderBy(x => x, StringComparer.Ordinal);
        return ActivityCsvConfiguration.Hash(Encoding.UTF8.GetBytes(string.Join("\n", files.Select(x =>
            x.Replace('\\', '/') + ":" + ActivityCsvConfiguration.Hash(Encoding.UTF8.GetBytes(File.ReadAllText(x).Replace("\r\n", "\n")))))));
    }
    [MenuItem("Tools/AddToActBytes")]
    public static void Prepare()
    {
        var files = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        string root = Path.GetFullPath(SourceRoot);
        foreach (string path in Directory.GetFiles(SourceRoot, "*.csv", SearchOption.AllDirectories))
        {
            if (Path.GetDirectoryName(Path.GetFullPath(path)) != root) throw new FormatException("活动 CSV 必须平铺: " + path);
            string name = Path.GetFileNameWithoutExtension(path);
            if (!ActivityCsvConfiguration.ValidName(name) || files.Keys.Any(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase))) throw new FormatException("活动表名无效或重复: " + path);
            try { files.Add(name, BinaryTableCsv.Load(path).Encode()); }
            catch (Exception e) { throw new FormatException(path + ": " + e.Message, e); }
        }
        var definitions = ActivityCsvConfiguration.Package(files);
        var ordinary = Directory.GetFiles(PublishedConfigBuilder.Root, "*.bytes").ToDictionary(Path.GetFileNameWithoutExtension, File.ReadAllBytes);
        var config = GameConfigTables.Assemble(ordinary);
        foreach (var d in definitions.Values) ActivityCsvConfiguration.Resources(d, config);
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var catalog = new SerializedObject(AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/AddressableCatalogs/SpriteCatalog.asset"));
        var entries = catalog.FindProperty("m_entries");
        var images = new HashSet<string>();
        for (int i = 0; i < entries.arraySize; i++)
        {
            var item = entries.GetArrayElementAtIndex(i);
            string guid = item.FindPropertyRelative("reference").FindPropertyRelative("m_AssetGUID").stringValue;
            string sub = item.FindPropertyRelative("reference").FindPropertyRelative("m_SubObjectName").stringValue;
            if (settings.FindAssetEntry(guid) != null && AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guid)).OfType<Sprite>().Any(x => x.name == sub))
                images.Add(item.FindPropertyRelative("assetName").stringValue);
        }
        foreach (var d in definitions.Values)
            foreach (var key in new[] { d.BannerResourceKey, d.Notice.ImageResourceKey }.Where(x => x.Length > 0))
                if (!images.Contains(key)) throw new FormatException(d.Id + ".csv / 图片: 缺少已绑定 SpriteCatalog 资源 " + key);
        var manifest = new ActivityPackageManifest { SourceHash = SourceHash(), Files = files.OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => new ActivityFileInfo { Table = x.Key, Size = x.Value.LongLength, Sha256 = ActivityCsvConfiguration.Hash(x.Value) }).ToList() };
        Directory.CreateDirectory(Root);
        // 校验全部完成才覆盖产物；生成清单最后写入，发布时再次核对每个文件。
        foreach (var pair in files)
        {
            string path = Root + "/" + pair.Key + ".bytes";
            if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(pair.Value)) File.WriteAllBytes(path, pair.Value);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }
        var group = settings.FindGroup("ActivityConfiguration") ?? settings.CreateGroup("ActivityConfiguration", false, false, false, null, typeof(BundledAssetGroupSchema));
        // 子表经独立 HTTP 发布，普通 Addressables 构建不携带活动包。
        group.GetSchema<BundledAssetGroupSchema>().IncludeInBuild = false;
        settings.AddLabel(ActivityCsvConfiguration.Label); settings.AddLabel(ActivityCsvConfiguration.GeneratedLabel);
        foreach (var file in files)
        {
            var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(Root + "/" + file.Key + ".bytes"), group);
            entry.SetAddress("ActivityConfig/" + file.Key); entry.SetLabel(ActivityCsvConfiguration.Label, true); entry.SetLabel(ActivityCsvConfiguration.GeneratedLabel, true);
        }
        foreach (var entry in group.entries.Where(x => !files.ContainsKey(Path.GetFileNameWithoutExtension(x.AssetPath))).ToArray())
        {
            settings.RemoveAssetEntry(entry.guid); AssetDatabase.DeleteAsset(entry.AssetPath);
        }
        File.WriteAllText(ManifestPath, JsonConvert.SerializeObject(manifest, Formatting.Indented), new UTF8Encoding(false));
        AssetDatabase.ImportAsset(ManifestPath); EditorUtility.SetDirty(settings); EditorUtility.SetDirty(group); EditorUtility.SetDirty(group.GetSchema<BundledAssetGroupSchema>());
        AssetDatabase.SaveAssets();
        Debug.Log("活动配置已生成: " + Root + "；未上传。活动源哈希: " + manifest.SourceHash);
    }
    public static byte[] Package(ActivityPackageManifest manifest)
    {
        if (SourceHash() != manifest.SourceHash) throw new InvalidOperationException("活动 CSV 自生成后已变化，请先执行 Tools/AddToActBytes");
        using (var memory = new MemoryStream())
        {
            using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, true))
            {
                foreach (var file in manifest.Files)
                {
                    byte[] bytes = File.ReadAllBytes(Root + "/" + file.Table + ".bytes");
                    if (bytes.LongLength != file.Size || ActivityCsvConfiguration.Hash(bytes) != file.Sha256) throw new InvalidOperationException(file.Table + ".bytes 已变化，请先执行 Tools/AddToActBytes");
                    using (var stream = zip.CreateEntry(file.Table + ".bytes").Open()) stream.Write(bytes, 0, bytes.Length);
                }
                using (var writer = new StreamWriter(zip.CreateEntry("manifest.json").Open(), new UTF8Encoding(false)))
                    writer.Write(JsonConvert.SerializeObject(manifest));
            }
            return memory.ToArray();
        }
    }
}

public sealed class ActivityPublishWindow : EditorWindow
{
    string m_url = BackendServiceController.BaseUrl, m_key = "", m_status = "";
    bool m_busy;
    [MenuItem("Tools/活动配置/发布活动")]
    static void Open() => GetWindow<ActivityPublishWindow>("发布活动");
    void OnGUI()
    {
        m_url = EditorGUILayout.TextField("后端地址", m_url); m_key = PublishKeyProvider.DrawField(m_key);
        EditorGUILayout.HelpBox("上传最近生成的总表与全部子表；全平台使用同一版本。请先通过普通内容发布准备新增文案、图片与卡牌。", MessageType.Info);
        using (new EditorGUI.DisabledScope(m_busy))
            if (GUILayout.Button("发布已生成活动包")) Publish();
        EditorGUILayout.LabelField(m_status, EditorStyles.wordWrappedLabel);
    }
    async void Publish()
    {
        m_busy = true;
        try
        {
            PublishKeyProvider.RequireKey(m_key); EditorBackendHttp.ValidateBaseUrl(m_url);
            var manifest = JsonConvert.DeserializeObject<ActivityPackageManifest>(File.ReadAllText(ActivityConfigBuilder.ManifestPath));
            // 生成和发布分开，读取发布版本用于乐观并发检查。
            var current = JObject.Parse(await EditorBackendHttp.GetAsync(m_url, "/api/admin/activities", PublishKeyProvider.Resolve(m_key)));
            manifest.ExpectedRevision = current["current"]?.Type == JTokenType.Null ? 0 : current["current"]?["revision"]?.Value<long>() ?? 0;
            var content = JObject.Parse(await EditorBackendHttp.GetAsync(m_url, "/api/content/latest/Editor", PublishKeyProvider.Resolve(m_key)));
            manifest.ReferenceTarget = "Editor"; manifest.ReferenceConfigHash = content["configHash"].Value<string>(); manifest.ReleaseId = Guid.NewGuid().ToString("D");
            byte[] package = ActivityConfigBuilder.Package(manifest);
            m_status = "正在上传活动包…"; Repaint();
            var result = JObject.Parse(await EditorBackendHttp.SendRawAsync(m_url, "PUT", "/api/admin/activities/config", package, "application/zip", PublishKeyProvider.Resolve(m_key)));
            m_status = "已发布全平台活动版本 " + result["revision"] + " / " + result["releaseId"];
        }
        catch (Exception e) { m_status = e.Message; Debug.LogError("活动发布失败: " + e.Message); }
        finally { m_busy = false; Repaint(); }
    }
}
#endif
