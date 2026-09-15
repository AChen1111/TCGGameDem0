#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AChen.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

public static class PublishedConfigBuilder
{
    public const string Root = "Assets/GameConfiguration";
    public const string ConfigPath = Root + "/config.json";
    public const string SettingsPath = Root + "/LocalizationSettings.asset";
    public const string WallpaperPath = "Assets/UI/Prefab/Hall/PreGameUI/WallpaperDisplayConfig.asset";

    public static void Prepare()
    {
        Export("Card");
        Export("Localization");
        var serializer = JsonSerializer.Create(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        var catalog = Newtonsoft.Json.Linq.JObject.FromObject(GameConfigCsvEditorParser.ParseFile("GameConfig/game-config.csv"))
            .ToObject<CatalogData>(serializer);
        var poolRows = GameConfigCsvEditorParser.ReadFields(File.ReadAllText("GameConfig/card-gacha.csv"));
        if (poolRows.Length == 0 || !poolRows[0].SequenceEqual(new[] { "Table", "PoolKey", "CardId", "Weight", "Rarity" })) throw new FormatException("卡池 CSV 表头无效");
        var pools = new System.Collections.Generic.List<PoolEntry>();
        var rarities = new System.Collections.Generic.List<RarityWeight>();
        foreach (var row in poolRows.Skip(1))
        {
            if (row.Length != 5) throw new FormatException("卡池 CSV 列数无效");
            if (row[0] == "Card") pools.Add(new PoolEntry { PoolKey = row[1], CardId = row[2], Weight = int.Parse(row[3]) });
            else if (row[0] == "Rarity") rarities.Add(new RarityWeight { Rarity = int.Parse(row[4]), Weight = int.Parse(row[3]) });
            else throw new FormatException("卡池 CSV 类型无效: " + row[0]);
        }
        var allRows = GameConfigCsvEditorParser.ReadFields(File.ReadAllText("GameConfig/all-cards.csv"));
        if (allRows.Length == 0 || !allRows[0].SequenceEqual(new[] { "CardId", "SourcePool" })) throw new FormatException("全卡 CSV 表头无效");
        var wallpaperAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(WallpaperPath);
        if (wallpaperAsset == null) throw new FormatException("缺少壁纸显示参数资产");
        var wallpaper = new SerializedObject(wallpaperAsset);
        var items = wallpaper.FindProperty("m_Items");
        var offsets = new WallpaperOffset[items.arraySize];
        for (int i = 0; i < items.arraySize; i++)
        {
            var item = items.GetArrayElementAtIndex(i);
            var sprite = item.FindPropertyRelative("spriteOffset").vector3Value;
            var down = item.FindPropertyRelative("downOffset").vector3Value;
            offsets[i] = new WallpaperOffset { Id = item.FindPropertyRelative("id").intValue,
                Sprite = new[] { sprite.x, sprite.y, sprite.z }, Down = new[] { down.x, down.y, down.z } };
        }
        var data = new PublishedGameConfig
        {
            SchemaVersion = 1,
            Catalog = catalog, PoolEntries = pools.ToArray(), RarityWeights = rarities.ToArray(),
            AllCards = allRows.Skip(1).Select(row => row.Length == 2
                ? new AllCardEntry { CardId = row[0], SourcePool = row[1] } : throw new FormatException("全卡 CSV 列数无效")).ToArray(),
            CardTable = File.ReadAllBytes("TableData/Generated/Cards.bytes"),
            TranslationTable = File.ReadAllBytes("TableData/Generated/Translations.bytes"),
            WallpaperOffsets = offsets
        };
        data.Validate();
        var settings = AddressableAssetSettingsDefaultObject.Settings ?? throw new InvalidOperationException("缺少 Addressables 设置");
        var addresses = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
        var spriteCatalog = new SerializedObject(AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/AddressableCatalogs/SpriteCatalog.asset"));
        var spriteEntries = spriteCatalog.FindProperty("m_entries");
        for (int i = 0; i < spriteEntries.arraySize; i++)
            addresses.Add(spriteEntries.GetArrayElementAtIndex(i).FindPropertyRelative("assetName").stringValue);
        foreach (string key in catalog.Avatars.Concat(catalog.Wallpapers).Select(x => x.ResourceKey)
            .Concat(catalog.Wallpapers.SelectMany(x => new[] { $"w_{x.Id:D2}_Down", $"w_{x.Id:D2}_Sprite" }))
            .Concat(catalog.CardPacks.Select(x => x.CoverResourceKey)))
            if (!addresses.Contains(key)) throw new FormatException("配置引用缺少 Addressables 资源: " + key);
        var localization = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(SettingsPath);
        if (localization == null || localization.chineseFont == null || localization.englishFont == null)
            throw new FormatException("语言字体映射缺失");
        // 卡图仍放在原资源组, 发布前确认配置引用的资源确实存在.
        var folders = new System.Collections.Generic.Dictionary<string, string>
        { ["Card01"] = "CardBag01_BlueEyes", ["Card02"] = "CardBag02_Hero", ["Card03"] = "CardBag03_SkyStriker" };
        foreach (var card in data.AllCards.Select(x => (x.CardId, x.SourcePool))
            .Concat(data.PoolEntries.Select(x => (x.CardId, x.PoolKey))))
        {
            if (!folders.TryGetValue(card.Item2, out string folder)
                || !File.Exists("Assets/UI/Card/" + folder + "/" + card.Item1 + ".jpg"))
                throw new FormatException("卡图引用无效: " + card.Item2 + "/" + card.Item1);
        }
        Directory.CreateDirectory(Root);
        File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(data, Formatting.Indented,
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }), new System.Text.UTF8Encoding(false));
        AssetDatabase.ImportAsset(ConfigPath);
        var group = settings.FindGroup("Remote_GameConfig") ?? settings.CreateGroup("Remote_GameConfig", false, false, false, null,
            typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
        var schema = group.GetSchema<BundledAssetGroupSchema>();
        schema.BuildPath.SetVariableByName(settings, "Remote.BuildPath");
        schema.LoadPath.SetVariableByName(settings, "Remote.LoadPath");
        schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
        group.GetSchema<ContentUpdateGroupSchema>().StaticContent = false;
        foreach (var pair in new[] { (ConfigPath, "GameConfig/Data"), (SettingsPath, "GameConfig/LocalizationSettings") })
            settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(pair.Item1), group).SetAddress(pair.Item2);
        EditorUtility.SetDirty(group); EditorUtility.SetDirty(settings); AssetDatabase.SaveAssets();
        ALog.Log("统一配置生成完成. Group=Remote_GameConfig; Result=Success", ALogCategories.Default);
    }

    static void Export(string table)
    {
        // Unity 启动环境的 PATH 可能不含 Windows PowerShell, 使用系统绝对路径.
        string powershell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
            "WindowsPowerShell", "v1.0", "powershell.exe");
        if (!File.Exists(powershell)) throw new FileNotFoundException("未找到 Windows PowerShell 导表工具", powershell);
        var start = new ProcessStartInfo(powershell, "-NoProfile -ExecutionPolicy Bypass -File Tools/UnityExcel2BytesCs/Excel2CsBytesTool.ps1 -SchemaPath Tools/UnityExcel2BytesCs/" + table + ".schema.json")
        { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true, WorkingDirectory = Directory.GetCurrentDirectory() };
        using (var process = Process.Start(start))
        {
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            process.WaitForExit();
            if (process.ExitCode != 0) throw new InvalidOperationException("配置导出失败: " + output.Result + error.Result);
        }
    }
}
#endif
