using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Spine;
using Spine.Unity;
using Spine.Unity.Editor;
using UnityEditor;
using UnityEngine;

public sealed class SpineRecoveryImporterWindow : EditorWindow
{
    const string PrefSource = "AChen.SpineRecovery.SourceFolder";
    const string PrefOutput = "AChen.SpineRecovery.OutputRoot";

    string m_SourceFolder = "";
    string m_OutputRoot = "Assets/Art/Spine";
    string m_Status = "选择含 json / atlas / png 的文件夹,或含多套子目录的父文件夹.";
    Vector2 m_StatusScroll;
    bool m_Importing;

    [MenuItem("Tools/AChen/导入 Spine 恢复资源")]
    static void Open()
    {
        var window = GetWindow<SpineRecoveryImporterWindow>("导入 Spine");
        window.minSize = new Vector2(520f, 260f);
    }

    void OnEnable()
    {
        m_SourceFolder = EditorPrefs.GetString(PrefSource, "");
        m_OutputRoot = EditorPrefs.GetString(PrefOutput, "Assets/Art/Spine");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("将恢复出的 Spine 目录导入为可预览资源", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUI.BeginChangeCheck();
            m_SourceFolder = EditorGUILayout.TextField("源文件夹", m_SourceFolder);
            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetString(PrefSource, m_SourceFolder);

            if (GUILayout.Button("浏览", GUILayout.Width(72f)))
            {
                string start = Directory.Exists(m_SourceFolder) ? m_SourceFolder : Application.dataPath;
                string picked = EditorUtility.OpenFolderPanel("选择 Spine 恢复目录", start, "");
                if (!string.IsNullOrEmpty(picked))
                {
                    m_SourceFolder = picked.Replace('\\', '/');
                    EditorPrefs.SetString(PrefSource, m_SourceFolder);
                }
            }
        }

        EditorGUI.BeginChangeCheck();
        m_OutputRoot = EditorGUILayout.TextField("输出根目录", m_OutputRoot);
        if (EditorGUI.EndChangeCheck())
            EditorPrefs.SetString(PrefOutput, m_OutputRoot);

        EditorGUILayout.Space();
        using (new EditorGUI.DisabledScope(m_Importing || string.IsNullOrWhiteSpace(m_SourceFolder)))
        {
            if (GUILayout.Button(m_Importing ? "导入中..." : "导入", GUILayout.Height(32f)))
            {
                // 导入过程会弹模态框, 反复刷新 AssetDatabase 并重建资源实例, 在 OnGUI 事件内执行会与
                // IMGUI 重入,结束后编辑器会一直卡到重启.这里延后到下一帧的非 GUI 上下文执行.
                m_Importing = true;
                m_Status = "导入中...";
                EditorApplication.delayCall += RunImport;
            }
        }

        EditorGUILayout.Space();
        m_StatusScroll = EditorGUILayout.BeginScrollView(m_StatusScroll);
        EditorGUILayout.HelpBox(m_Status, MessageType.None);
        EditorGUILayout.EndScrollView();
    }

    void RunImport()
    {
        try
        {
            IReadOnlyList<string> outputs = SpineRecoveryImporter.Import(m_SourceFolder, m_OutputRoot);
            m_Status = outputs.Count == 0
                ? "未找到可导入的 Spine 套."
                : "已导入:\n" + string.Join("\n", outputs);
        }
        catch (Exception exception)
        {
            m_Status = "导入失败: " + exception.Message;
            Debug.LogError("[SpineRecovery] " + exception, this);
        }
        finally
        {
            m_Importing = false;
            Repaint();
        }
    }
}

public static class SpineRecoveryImporter
{
    const string LogPrefix = "[SpineRecovery] ";

    public static IReadOnlyList<string> Import(string sourceFolder, string outputRoot)
    {
        if (string.IsNullOrWhiteSpace(sourceFolder) || !Directory.Exists(sourceFolder))
            throw new DirectoryNotFoundException("源文件夹不存在: " + sourceFolder);

        outputRoot = NormalizeAssetPath(outputRoot);
        if (!outputRoot.StartsWith("Assets/", StringComparison.Ordinal))
            throw new ArgumentException("输出根目录必须在 Assets/ 下.");

        List<string> setFolders = DiscoverSetFolders(sourceFolder);
        if (setFolders.Count == 0)
            throw new InvalidOperationException("目录中没有同时包含 json、atlas、png 的 Spine 套.");

        EnsureAssetFolder(outputRoot);

        var outputs = new List<string>();
        try
        {
            for (int i = 0; i < setFolders.Count; i++)
            {
                string setFolder = setFolders[i];
                string setName = Path.GetFileName(setFolder.TrimEnd('/', '\\'));
                EditorUtility.DisplayProgressBar("导入 Spine", setName, (float)i / setFolders.Count);
                outputs.Add(ImportOne(setFolder, outputRoot + "/" + setName));
            }

            AssetDatabase.SaveAssets();
        }
        finally
        {
            // 取消覆盖或中途报错时进度条会一直挂着,编辑器看起来就是卡死,必须无条件清掉.
            EditorUtility.ClearProgressBar();
            // 导入期间整张图集会被解码进托管内存,不立刻回收编辑器会一直带着这些大块内存变卡.
            EditorUtility.UnloadUnusedAssetsImmediate();
            GC.Collect();
        }

        Debug.Log(LogPrefix + "导入完成. Count=" + outputs.Count + ", OutputRoot=" + outputRoot);
        return outputs;
    }

    static string ImportOne(string sourceFolder, string destAssetDir)
    {
        sourceFolder = Path.GetFullPath(sourceFolder);
        destAssetDir = NormalizeAssetPath(destAssetDir);
        string setName = Path.GetFileName(destAssetDir);

        if (!TryFindSetFiles(sourceFolder, out string jsonSrc, out string atlasSrc, out List<string> pngSrcs))
            throw new InvalidOperationException("不是完整 Spine 套: " + sourceFolder);

        if (AssetDatabase.IsValidFolder(destAssetDir))
        {
            if (!EditorUtility.DisplayDialog("覆盖目录", "已存在 " + destAssetDir + ", 要覆盖吗?", "覆盖", "取消"))
                throw new OperationCanceledException("用户取消覆盖 " + destAssetDir);

            // 走 AssetDatabase 删除,让旧的 _Atlas / _SkeletonData 实例从内存里正常卸载.
            AssetDatabase.DeleteAsset(destAssetDir);
        }

        EnsureAssetFolder(destAssetDir);

        string atlasText = File.ReadAllText(atlasSrc);
        bool sourceIsPma = IsPmaAtlas(atlasText);
        if (sourceIsPma)
            atlasText = ForceStraightAlphaAtlas(atlasText);

        // 先只落盘贴图并让贴图设置生效,之后才落盘 json / atlas.
        // Spine 的 OnPostprocessAllAssets 会在 atlas 进库的那次刷新里自动摄取并生成
        // _Atlas / _Material / _SkeletonData;此时贴图已是最终状态,不会因为随后再改贴图设置
        // 触发第二次摄取,从而避免留下"已销毁但仍被引用"的 ScriptableObject 实例.
        var pngDests = new List<string>();
        foreach (string pngSrc in pngSrcs)
        {
            string pngDest = destAssetDir + "/" + Path.GetFileName(pngSrc);
            File.Copy(pngSrc, ToAbsolute(pngDest), true);
            if (sourceIsPma)
                ConvertPmaToStraightAlpha(ToAbsolute(pngDest));
            pngDests.Add(pngDest);
        }

        AssetDatabase.Refresh();

        foreach (string pngDest in pngDests)
            ApplyTextureImportSettings(pngDest);

        string jsonDest = destAssetDir + "/" + Path.GetFileName(jsonSrc);
        File.Copy(jsonSrc, ToAbsolute(jsonDest), true);

        string atlasDest = destAssetDir + "/" + Path.GetFileNameWithoutExtension(atlasSrc.Replace(".atlas.txt", ".atlas")) + ".atlas.txt";
        File.WriteAllText(ToAbsolute(atlasDest), atlasText);

        AssetDatabase.Refresh();

        ApplyAtlasMaterialSettings(destAssetDir, false);

        SkeletonDataAsset skeletonData = EnsureSkeletonData(destAssetDir, jsonDest, atlasDest);
        if (skeletonData == null)
            throw new InvalidOperationException("未能生成 SkeletonDataAsset: " + destAssetDir);

        string animationName = FirstAnimationName(skeletonData);
        if (string.IsNullOrEmpty(animationName))
            throw new InvalidOperationException("SkeletonData 中没有动画: " + destAssetDir);

        string prefabPath = destAssetDir + "/" + setName + ".prefab";
        CreatePrefab(skeletonData, prefabPath, setName, animationName);
        AddressableCatalogSetup.AddSpinePrefab(prefabPath);

        Debug.Log(LogPrefix + "已导入并加入 Addressables " + setName + " -> " + destAssetDir
            + ", Address=Spine/" + setName + ", Anim=" + animationName);
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(destAssetDir));
        return destAssetDir;
    }

    static void CreatePrefab(
        SkeletonDataAsset skeletonData,
        string prefabPath,
        string objectName,
        string animationName)
    {
        // useObjectFactory: false 避免这个临时对象进入撤销栈并弄脏当前打开的场景.
        SkeletonAnimation anim = EditorInstantiation.InstantiateSkeletonAnimation(
            skeletonData, "default", destroyInvalid: true, useObjectFactory: false);
        if (anim == null)
            throw new InvalidOperationException("InstantiateSkeletonAnimation 失败: " + objectName);

        GameObject go = anim.gameObject;
        try
        {
            go.name = objectName;
            go.hideFlags = HideFlags.HideAndDontSave;
            anim.loop = true;
            anim.AnimationName = animationName;
            if (anim.state != null)
            {
                anim.state.SetAnimation(0, animationName, true);
                anim.state.Update(0);
                anim.state.Apply(anim.skeleton);
                anim.skeleton.UpdateWorldTransform(Skeleton.Physics.Update);
            }

            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            }

            go.hideFlags = HideFlags.None;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            if (prefab == null)
                throw new InvalidOperationException("保存预制体失败: " + prefabPath);
        }
        finally
        {
            // 任何一步失败都不能把这个带 ExecuteInEditMode 的骨骼对象留在场景里.
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    static SkeletonDataAsset EnsureSkeletonData(string destAssetDir, string jsonAssetPath, string atlasTxtPath)
    {
        foreach (string path in FindAssets<SkeletonDataAsset>(destAssetDir))
        {
            SkeletonDataAsset existing = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(path);
            if (existing != null)
                return existing;
        }

        TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonAssetPath);
        string atlasAssetPath = destAssetDir + "/" +
            Path.GetFileNameWithoutExtension(atlasTxtPath.Replace(".atlas.txt", ".atlas")) +
            AssetUtility.AtlasSuffix + ".asset";
        SpineAtlasAsset atlas = AssetDatabase.LoadAssetAtPath<SpineAtlasAsset>(atlasAssetPath);
        if (json == null || atlas == null)
            return null;

        SkeletonDataAsset data = ScriptableObject.CreateInstance<SkeletonDataAsset>();
        data.atlasAssets = new AtlasAssetBase[] { atlas };
        data.skeletonJSON = json;
        data.scale = SpineEditorUtilities.Preferences.defaultScale;
        data.defaultMix = SpineEditorUtilities.Preferences.defaultMix;
        string assetPath = destAssetDir + "/" + Path.GetFileNameWithoutExtension(jsonAssetPath) +
            AssetUtility.SkeletonDataSuffix + ".asset";
        AssetDatabase.CreateAsset(data, assetPath);
        data.GetSkeletonData(true);
        return data;
    }

    static void ApplyAtlasMaterialSettings(string destAssetDir, bool sourceIsPma)
    {
        string absDir = ToAbsolute(destAssetDir);
        if (!Directory.Exists(absDir))
            return;

        foreach (string matFile in Directory.GetFiles(absDir, "*.mat"))
        {
            string assetPath = destAssetDir + "/" + Path.GetFileName(matFile);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material == null)
                continue;

            MaterialChecks.EnablePMATextureAtMaterial(material, sourceIsPma);
            EditorUtility.SetDirty(material);
        }
    }

    static void ApplyTextureImportSettings(string pngAssetPath)
    {
        var importer = AssetImporter.GetAtPath(pngAssetPath) as TextureImporter;
        if (importer == null)
            return;

        int maxSize = TextureMaxSize(ToAbsolute(pngAssetPath));
        importer.sRGBTexture = true;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = false;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spriteImportMode = SpriteImportMode.None;
        importer.maxTextureSize = maxSize;
        importer.SaveAndReimport();
    }

    // 在线性空间反预乘,透明像素 RGB 清零,避免 Gamma 除法造成白边和偏色.
    static void ConvertPmaToStraightAlpha(string absolutePath)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        try
        {
            if (!texture.LoadImage(File.ReadAllBytes(absolutePath)))
                throw new InvalidOperationException("无法解码 PNG: " + absolutePath);

            Color32[] pixels = texture.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
            {
                float alpha = pixels[i].a / 255f;
                if (alpha <= 1e-6f)
                {
                    pixels[i].r = 0;
                    pixels[i].g = 0;
                    pixels[i].b = 0;
                    pixels[i].a = 0;
                    continue;
                }

                if (pixels[i].a == 255)
                    continue;

                float r = SrgbToLinear(pixels[i].r / 255f) / alpha;
                float g = SrgbToLinear(pixels[i].g / 255f) / alpha;
                float b = SrgbToLinear(pixels[i].b / 255f) / alpha;
                pixels[i].r = (byte)Mathf.Clamp(Mathf.RoundToInt(LinearToSrgb(r) * 255f), 0, 255);
                pixels[i].g = (byte)Mathf.Clamp(Mathf.RoundToInt(LinearToSrgb(g) * 255f), 0, 255);
                pixels[i].b = (byte)Mathf.Clamp(Mathf.RoundToInt(LinearToSrgb(b) * 255f), 0, 255);
            }

            texture.SetPixels32(pixels);
            File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }

    static float SrgbToLinear(float value)
    {
        return value <= 0.04045f ? value / 12.92f : Mathf.Pow((value + 0.055f) / 1.055f, 2.4f);
    }

    static float LinearToSrgb(float value)
    {
        value = Mathf.Clamp01(value);
        return value <= 0.0031308f ? value * 12.92f : 1.055f * Mathf.Pow(value, 1f / 2.4f) - 0.055f;
    }

    static string ForceStraightAlphaAtlas(string atlasText)
    {
        if (Regex.IsMatch(atlasText, @"pma:\s*(true|false)", RegexOptions.IgnoreCase))
            return Regex.Replace(atlasText, @"pma:\s*(true|false)", "pma:false", RegexOptions.IgnoreCase);

        return Regex.Replace(
            atlasText,
            @"(filter:\s*Linear,Linear\r?\n)",
            "$1pma:false\n",
            RegexOptions.IgnoreCase);
    }

    // Spine 默认按 2048 导入,4096 图集会错位,按实际边长提升上限.
    // 只读 PNG 的 IHDR 头拿宽高,避免为了取尺寸把整张图集再解码一遍进内存.
    static int TextureMaxSize(string absolutePngPath)
    {
        int max = 0;
        var header = new byte[24];
        using (FileStream stream = File.OpenRead(absolutePngPath))
        {
            if (stream.Read(header, 0, header.Length) == header.Length)
            {
                int width = (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
                int height = (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];
                max = Mathf.Max(width, height);
            }
        }

        // 头部异常时直接放到最大,宁可不压缩也不能让图集被缩放导致 UV 错位.
        if (max <= 0)
            return 8192;
        if (max <= 1024)
            return 1024;
        if (max <= 2048)
            return 2048;
        if (max <= 4096)
            return 4096;
        return 8192;
    }

    static bool IsPmaAtlas(string atlasText)
    {
        if (Regex.IsMatch(atlasText, @"pma:\s*false", RegexOptions.IgnoreCase))
            return false;
        return true;
    }

    static List<string> DiscoverSetFolders(string sourceFolder)
    {
        var folders = new List<string>();
        if (TryFindSetFiles(sourceFolder, out _, out _, out _))
        {
            folders.Add(sourceFolder);
            return folders;
        }

        foreach (string child in Directory.GetDirectories(sourceFolder))
        {
            if (TryFindSetFiles(child, out _, out _, out _))
                folders.Add(child);
        }

        folders.Sort(StringComparer.OrdinalIgnoreCase);
        return folders;
    }

    static bool TryFindSetFiles(string folder, out string jsonPath, out string atlasPath, out List<string> pngPaths)
    {
        jsonPath = null;
        atlasPath = null;
        pngPaths = new List<string>();

        string[] jsons = Directory.GetFiles(folder, "*.json");
        foreach (string json in jsons)
        {
            if (json.EndsWith("JS.json", StringComparison.OrdinalIgnoreCase) ||
                File.ReadAllText(json).IndexOf("\"spine\"", StringComparison.Ordinal) >= 0)
            {
                jsonPath = json;
                break;
            }
        }

        string[] atlasTxts = Directory.GetFiles(folder, "*.atlas.txt");
        if (atlasTxts.Length > 0)
            atlasPath = atlasTxts[0];
        else
        {
            string[] atlases = Directory.GetFiles(folder, "*.atlas");
            if (atlases.Length > 0)
                atlasPath = atlases[0];
        }

        if (jsonPath == null || atlasPath == null)
            return false;

        foreach (string pageName in AtlasPageNames(File.ReadAllText(atlasPath)))
        {
            string png = Path.Combine(folder, pageName);
            if (!File.Exists(png))
                return false;
            pngPaths.Add(png);
        }

        return pngPaths.Count > 0;
    }

    static List<string> AtlasPageNames(string atlasText)
    {
        var names = new List<string>();
        string[] lines = atlasText.Replace("\r\n", "\n").Split('\n');
        bool expectPage = true;
        foreach (string raw in lines)
        {
            string line = raw.Trim();
            if (line.Length == 0)
            {
                expectPage = true;
                continue;
            }

            if (expectPage && line.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                names.Add(line);
                expectPage = false;
            }
        }

        return names;
    }

    static string FirstAnimationName(SkeletonDataAsset skeletonData)
    {
        SkeletonData data = skeletonData.GetSkeletonData(false);
        if (data == null || data.Animations == null || data.Animations.Count == 0)
            return null;
        return data.Animations.Items[0].Name;
    }

    static IEnumerable<string> FindAssets<T>(string folder) where T : UnityEngine.Object
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folder });
        foreach (string guid in guids)
            yield return AssetDatabase.GUIDToAssetPath(guid);
    }

    static void EnsureAssetFolder(string assetPath)
    {
        assetPath = NormalizeAssetPath(assetPath);
        if (AssetDatabase.IsValidFolder(assetPath))
            return;

        string parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
        string name = Path.GetFileName(assetPath);
        if (string.IsNullOrEmpty(parent) || parent == assetPath)
            throw new InvalidOperationException("无法创建目录: " + assetPath);

        EnsureAssetFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    static string NormalizeAssetPath(string path)
    {
        return string.IsNullOrWhiteSpace(path) ? path : path.Replace('\\', '/').TrimEnd('/');
    }

    static string ToAbsolute(string assetPath)
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
    }
}
