using Spine.Unity;
using Spine.Unity.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CreateSpinePreviewPrefab
{
    public static string CreateP12325()
    {
        return Run(
            "Assets/Art/Spine/P12325JS/P12325JS_SkeletonData.asset",
            "Assets/Art/Spine/P12325JS/P12325JS.prefab",
            "Assets/Art/Spine/P12325JS/P12325JSPreview.unity",
            "P12325JS",
            "animation");
    }

    public static string CreateP16528()
    {
        return Run(
            "Assets/Art/Spine/P16528JS/P16528JS_SkeletonData.asset",
            "Assets/Art/Spine/P16528JS/P16528JS.prefab",
            "Assets/Art/Spine/P16528JS/P16528JSPreview.unity",
            "P16528JS",
            "animation");
    }

    public static string Run(string dataPath, string prefabPath, string scenePath, string objectName, string animationName)
    {
        SkeletonDataAsset data = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(dataPath);
        if (data == null)
            throw new System.Exception("找不到 SkeletonDataAsset: " + dataPath);

        Scene scene = default;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene opened = SceneManager.GetSceneAt(i);
            if (opened.path == scenePath)
            {
                scene = opened;
                break;
            }
        }

        if (!scene.IsValid())
        {
            if (System.IO.File.Exists(scenePath))
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                EditorSceneManager.SaveScene(scene, scenePath);
            }
        }
        else
        {
            EditorSceneManager.SetActiveScene(scene);
        }

        foreach (GameObject root in scene.GetRootGameObjects())
            Object.DestroyImmediate(root);

        SkeletonAnimation anim = EditorInstantiation.InstantiateSkeletonAnimation(data, "default");
        if (anim == null)
            throw new System.Exception("InstantiateSkeletonAnimation 失败");

        anim.gameObject.name = objectName;
        anim.loop = true;
        anim.AnimationName = animationName;
        if (anim.state != null)
        {
            anim.state.SetAnimation(0, animationName, true);
            anim.state.Update(0);
            anim.state.Apply(anim.skeleton);
            anim.skeleton.UpdateWorldTransform(Spine.Skeleton.Physics.Update);
        }

        EditorSceneManager.MoveGameObjectToScene(anim.gameObject, scene);

        GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
            anim.gameObject, prefabPath, InteractionMode.AutomatedAction);
        if (prefab == null)
            throw new System.Exception("保存预制体失败: " + prefabPath);

        GameObject camGo = new GameObject("PreviewCamera");
        Camera cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 16f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.12f, 0.14f, 1f);
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        EditorSceneManager.MoveGameObjectToScene(camGo, scene);

        EditorSceneManager.SaveScene(scene, scenePath);
        Selection.activeGameObject = anim.gameObject;
        EditorGUIUtility.PingObject(prefab);
        return prefabPath;
    }
}
