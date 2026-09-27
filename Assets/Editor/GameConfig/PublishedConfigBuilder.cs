#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AChen.Configuration;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class PublishedConfigBuilder
{
    public const string SourceRoot = "TableData";
    public const string Root = "Assets/GameConfiguration";
    public const string SettingsPath = Root + "/LocalizationSettings.asset";

    [MenuItem("Tools/AddToBytes")]
    public static void Prepare()
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var files = CompileDirectory(SourceRoot);
            ValidateAssets(GameConfigTables.Assemble(files));
            var settings = AddressableAssetSettingsDefaultObject.Settings ?? throw new InvalidOperationException("缺少 Addressables 设置");
            var group = settings.FindGroup("Remote_GameConfig");
            var expected = files.Keys.ToDictionary(name => name, name => Root + "/" + GameConfigTables.FileName(name), StringComparer.Ordinal);
            var stale = group == null ? new string[0] : group.entries
                .Where(e => e.labels.Contains(GameConfigTables.GeneratedLabel) && e.AssetPath.StartsWith(Root + "/", StringComparison.Ordinal)
                    && (!expected.TryGetValue(Path.GetFileNameWithoutExtension(e.AssetPath), out string path) || e.AssetPath != path))
                .Select(e => e.AssetPath).ToArray();
            var targets = expected.Values.Concat(stale).Distinct().ToArray();
            var backup = targets.SelectMany(path => new[] { path, path + ".meta" }).Distinct()
                .ToDictionary(path => path, path => File.Exists(path) ? File.ReadAllBytes(path) : null);
            string settingsBackup = EditorJsonUtility.ToJson(settings);
            string groupBackup = group == null ? null : EditorJsonUtility.ToJson(group);
            string temp = Path.Combine(Path.GetTempPath(), "AChenGameConfig-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
            Directory.CreateDirectory(temp);
            try
            {
                foreach (var pair in files)
                    File.WriteAllBytes(Path.Combine(temp, GameConfigTables.FileName(pair.Key)), pair.Value);
                foreach (var pair in files)
                {
                    string dest = expected[pair.Key];
                    string legacy = Root + "/" + pair.Key + ".json";
                    if (!File.Exists(dest) && File.Exists(legacy))
                    {
                        File.Move(legacy, dest);
                        if (File.Exists(legacy + ".meta")) File.Move(legacy + ".meta", dest + ".meta");
                    }
                    byte[] staged = File.ReadAllBytes(Path.Combine(temp, GameConfigTables.FileName(pair.Key)));
                    if (!File.Exists(dest) || !File.ReadAllBytes(dest).SequenceEqual(staged)) File.WriteAllBytes(dest, staged);
                    AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceSynchronousImport);
                }
                group = group ?? settings.CreateGroup("Remote_GameConfig", false, false, false, null,
                    typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
                var schema = group.GetSchema<BundledAssetGroupSchema>();
                schema.BuildPath.SetVariableByName(settings, "Remote.BuildPath");
                schema.LoadPath.SetVariableByName(settings, "Remote.LoadPath");
                schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
                group.GetSchema<ContentUpdateGroupSchema>().StaticContent = false;
                settings.AddLabel(GameConfigTables.Label); settings.AddLabel(GameConfigTables.GeneratedLabel);
                foreach (string name in files.Keys)
                {
                    var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(expected[name]), group);
                    entry.SetAddress(GameConfigTables.Address(name));
                    entry.SetLabel(GameConfigTables.Label, true); entry.SetLabel(GameConfigTables.GeneratedLabel, true);
                }
                var font = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(SettingsPath), group);
                font.SetAddress("GameConfig/LocalizationSettings"); font.SetLabel(GameConfigTables.Label, true);
                var logging = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(ALogSettings.AssetPath), group);
                logging.SetAddress(ALogSettings.Address); logging.SetLabel(GameConfigTables.Label, true);
                foreach (var entry in group.entries.Where(e => e.address == "GameConfig/Data").ToArray()) settings.RemoveAssetEntry(entry.guid);
                foreach (string path in stale)
                {
                    settings.RemoveAssetEntry(AssetDatabase.AssetPathToGUID(path));
                    if (!AssetDatabase.DeleteAsset(path)) throw new IOException("清理旧产物失败: " + path);
                }
                EditorUtility.SetDirty(schema); EditorUtility.SetDirty(group); EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
            catch
            {
                foreach (var pair in backup)
                {
                    if (pair.Value == null) { if (File.Exists(pair.Key)) File.Delete(pair.Key); }
                    else File.WriteAllBytes(pair.Key, pair.Value);
                }
                if (groupBackup != null) EditorJsonUtility.FromJsonOverwrite(groupBackup, group);
                else if (group != null) settings.RemoveGroup(group);
                EditorJsonUtility.FromJsonOverwrite(settingsBackup, settings);
                AssetDatabase.Refresh(); throw;
            }
            finally
            {
                if (Directory.Exists(temp)) Directory.Delete(temp, true);
            }
            ALog.Log($"生成全部配置完成. Source={SourceRoot}; Generated={files.Count}; Removed={stale.Length}; Milliseconds={timer.ElapsedMilliseconds}; Result=Success", ALogCategories.Default);
        }
        catch (Exception ex)
        {
            ALog.LogError("生成配置失败. Result=Failed; Error=" + ex.Message, ALogCategories.Default); throw;
        }
    }

    public static Dictionary<string, byte[]> CompileDirectory(string root)
    {
        var files = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string fullRoot = Path.GetFullPath(root);
        foreach (string path in Directory.GetFiles(root, "*", SearchOption.AllDirectories).Where(p => p.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)).OrderBy(p => p, StringComparer.Ordinal))
        {
            if (!string.Equals(Path.GetDirectoryName(Path.GetFullPath(path)), fullRoot, StringComparison.OrdinalIgnoreCase))
                throw new FormatException("源表必须平铺, 请迁入 " + root + ": " + path);
            string name = Path.GetFileNameWithoutExtension(path);
            if (!GameConfigTables.ValidName(name) || !names.Add(name)) throw new FormatException("表名无效或大小写冲突: " + path);
            files.Add(name, BinaryTableCsv.Compile(path));
        }
        GameConfigTables.Assemble(files);
        DeckRulesConfiguration.Load(files);
        return files;
    }

    static void ValidateAssets(PublishedGameConfig data)
    {
        var localization = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(SettingsPath);
        if (localization == null || localization.chineseFont == null || localization.englishFont == null) throw new FormatException("字体映射或字体引用缺失");
        var catalog = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/AddressableCatalogs/SpriteCatalog.asset");
        if (catalog == null) throw new FormatException("缺少 SpriteCatalog");
        var entries = new SerializedObject(catalog).FindProperty("m_entries");
        var addresses = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < entries.arraySize; i++) addresses.Add(entries.GetArrayElementAtIndex(i).FindPropertyRelative("assetName").stringValue);
        foreach (string key in data.Catalog.Avatars.Concat(data.Catalog.Wallpapers).Select(x => x.ResourceKey)
            .Concat(data.Catalog.Wallpapers.SelectMany(x => new[] { $"w_{x.Id:D2}_Down", $"w_{x.Id:D2}_Sprite" }))
            .Concat(data.Catalog.CardPacks.Select(x => x.CoverResourceKey)))
            if (!addresses.Contains(key)) throw new FormatException("配置引用缺少资源: " + key);
        var folders = new Dictionary<string, string> { ["Card01"] = "CardBag01_BlueEyes", ["Card02"] = "CardBag02_Hero", ["Card03"] = "CardBag03_SkyStriker" };
        foreach (var card in data.AllCards.Select(x => (x.CardId, x.SourcePool)).Concat(data.PoolEntries.Select(x => (x.CardId, x.PoolKey))))
            if (!folders.TryGetValue(card.Item2, out string folder) || !File.Exists("Assets/UI/Card/" + folder + "/" + card.Item1 + ".jpg"))
                throw new FormatException("卡图引用无效: " + card.Item2 + "/" + card.Item1);
    }
}
#endif
