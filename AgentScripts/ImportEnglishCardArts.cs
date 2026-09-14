using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class ImportEnglishCardArts
{
    const string SourceRoot = @"d:\Game\MDPro3\Picture\CardGenerated";
    const string CardRoot = "Assets/UI/Card";
    static readonly string[] ImageExts = { ".jpg", ".jpeg", ".png", ".webp" };

    static readonly (string SourceFolder, string BagFolder)[] Bags =
    {
        ("card01_en", "CardBag01_BlueEyes"),
        ("card02_en", "CardBag02_Hero"),
        ("card03_en", "CardBag03_SkyStriker")
    };

    public static string Run()
    {
        var report = new StringBuilder();
        int copied = 0;
        int missingEn = 0;
        int extraEn = 0;
        int missingSrc = 0;
        var copiedPaths = new List<(string dest, string template)>();

        for (int i = 0; i < Bags.Length; i++)
        {
            string sourceDir = Path.Combine(SourceRoot, Bags[i].SourceFolder);
            string bagAsset = CardRoot + "/" + Bags[i].BagFolder;
            string enAsset = bagAsset + "/en";
            if (!Directory.Exists(sourceDir))
            {
                missingSrc++;
                report.Append("missing-src=").Append(sourceDir).Append(';');
                ALog.LogWarning($"英文卡图源目录不存在. Path={sourceDir}", ALogCategories.Localization);
                continue;
            }

            if (!AssetDatabase.IsValidFolder(bagAsset))
            {
                missingSrc++;
                report.Append("missing-bag=").Append(bagAsset).Append(';');
                ALog.LogWarning($"中文卡包目录不存在. Path={bagAsset}", ALogCategories.Localization);
                continue;
            }

            Dictionary<long, string> cnStems = CollectChineseStems(bagAsset, out string templatePath);
            var matchedCn = new HashSet<long>();
            if (!AssetDatabase.IsValidFolder(enAsset))
            {
                AssetDatabase.CreateFolder(bagAsset, "en");
            }

            string enAbs = ToAbsolute(enAsset);
            Directory.CreateDirectory(enAbs);

            string[] sources = Directory.GetFiles(sourceDir);
            for (int s = 0; s < sources.Length; s++)
            {
                string srcPath = sources[s];
                string ext = Path.GetExtension(srcPath);
                if (!IsImageExt(ext))
                {
                    continue;
                }

                if (!TryParseCardId(Path.GetFileNameWithoutExtension(srcPath), out long id))
                {
                    ALog.LogWarning($"英文卡图文件名无法解析卡号. File={Path.GetFileName(srcPath)}", ALogCategories.Localization);
                    continue;
                }

                string stem;
                bool extra;
                if (cnStems.TryGetValue(id, out string cnStem))
                {
                    stem = cnStem;
                    extra = false;
                    matchedCn.Add(id);
                }
                else
                {
                    stem = id.ToString("D8");
                    extra = true;
                    extraEn++;
                }

                string destAsset = enAsset + "/" + stem + ext.ToLowerInvariant();
                File.Copy(srcPath, ToAbsolute(destAsset), true);
                copied++;
                copiedPaths.Add((destAsset, cnStems.TryGetValue(id, out string matched) ? bagAsset + "/" + matched + ".jpg" : templatePath));
                if (extra)
                {
                    ALog.Log($"英文卡图无对应中文卡, 仍已拷贝. Bag={Bags[i].BagFolder}; CardId={stem}", ALogCategories.Localization);
                }
            }

            foreach (KeyValuePair<long, string> pair in cnStems)
            {
                if (matchedCn.Contains(pair.Key))
                {
                    continue;
                }

                missingEn++;
                ALog.LogWarning($"中文卡缺英文图. Bag={Bags[i].BagFolder}; CardId={pair.Value}", ALogCategories.Localization);
            }
        }

        AssetDatabase.Refresh();
        for (int i = 0; i < copiedPaths.Count; i++)
        {
            CopyImporter(copiedPaths[i].template, copiedPaths[i].dest);
        }

        AddressableCatalogSetup.MarkUiArtFolders();
        AssetDatabase.SaveAssets();
        ALog.Log($"英文卡图导入完成. Copied={copied}; MissingEn={missingEn}; ExtraEn={extraEn}; MissingSrc={missingSrc}", ALogCategories.Localization);
        report.Append("copied=").Append(copied)
            .Append(";missingEn=").Append(missingEn)
            .Append(";extraEn=").Append(extraEn)
            .Append(";missingSrc=").Append(missingSrc);
        return report.ToString();
    }

    static Dictionary<long, string> CollectChineseStems(string bagAsset, out string templatePath)
    {
        var map = new Dictionary<long, string>();
        templatePath = null;
        string abs = ToAbsolute(bagAsset);
        string[] files = Directory.GetFiles(abs, "*.jpg", SearchOption.TopDirectoryOnly);
        for (int i = 0; i < files.Length; i++)
        {
            string stem = Path.GetFileNameWithoutExtension(files[i]);
            if (!TryParseCardId(stem, out long id))
            {
                continue;
            }

            map[id] = stem;
            if (templatePath == null)
            {
                templatePath = bagAsset + "/" + Path.GetFileName(files[i]);
            }
        }

        return map;
    }

    static bool TryParseCardId(string stem, out long id)
    {
        id = 0;
        if (string.IsNullOrEmpty(stem))
        {
            return false;
        }

        for (int i = 0; i < stem.Length; i++)
        {
            if (!char.IsDigit(stem[i]))
            {
                return false;
            }
        }

        return long.TryParse(stem, out id);
    }

    static bool IsImageExt(string ext)
    {
        for (int i = 0; i < ImageExts.Length; i++)
        {
            if (string.Equals(ext, ImageExts[i], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    static void CopyImporter(string templateAsset, string destAsset)
    {
        var dest = AssetImporter.GetAtPath(destAsset) as TextureImporter;
        if (dest == null)
        {
            return;
        }

        var src = string.IsNullOrEmpty(templateAsset)
            ? null
            : AssetImporter.GetAtPath(templateAsset) as TextureImporter;
        if (src == null)
        {
            return;
        }

        var settings = new TextureImporterSettings();
        src.ReadTextureSettings(settings);
        dest.SetTextureSettings(settings);
        dest.SetPlatformTextureSettings(src.GetDefaultPlatformTextureSettings());
        dest.SaveAndReimport();
    }

    static string ToAbsolute(string assetPath)
    {
        string root = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(root, assetPath.Replace('/', Path.DirectorySeparatorChar));
    }
}
