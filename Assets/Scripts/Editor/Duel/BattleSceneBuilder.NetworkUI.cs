using System;
using System.Linq;
using AChen.Duel.Presentation;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEditor;
using UnityEngine;

public static partial class BattleSceneBuilder
{
    public static string UpgradeNetworkBattleUI()
    {
        // 使用已有 HUD 的字体，不重建其他显示资源。
        var panel = PrefabUtility.LoadPrefabContents(Prefabs + "BattleSelectionPanel.prefab");
        var screen = Root<BattleSelectionPanel>(panel); var so = new SerializedObject(screen);
        if (so.FindProperty("m_scroll").objectReferenceValue == null)
        {
        var content = (RectTransform)so.FindProperty("m_content").objectReferenceValue;
        var body = (RectTransform)content.parent;
        body.sizeDelta = new Vector2(body.sizeDelta.x, 400);
        foreach (string field in new[] { "m_confirm", "m_cancel" })
        {
            var buttonRect = (RectTransform)((Component)so.FindProperty(field).objectReferenceValue).transform;
            buttonRect.anchoredPosition = new Vector2(buttonRect.anchoredPosition.x, -337);
        }
        var scrollRoot = Rect("CardScroll", body, 20, 50, 1020, 240);
        scrollRoot.anchorMin = new Vector2(0, 1); scrollRoot.anchorMax = new Vector2(1, 1);
        scrollRoot.pivot = new Vector2(.5f, 1); scrollRoot.sizeDelta = new Vector2(-40, 240); scrollRoot.anchoredPosition = new Vector2(0, -50);
        var viewport = Rect("Viewport", scrollRoot, 0, 0, 1020, 240); Stretch(viewport);
        var viewportImage = viewport.gameObject.AddComponent<UnityEngine.UI.Image>(); viewportImage.color = new Color(0, 0, 0, .02f);
        var mask = viewport.gameObject.AddComponent<UnityEngine.UI.Mask>(); mask.showMaskGraphic = false;
        var scroll = scrollRoot.gameObject.AddComponent<UnityEngine.UI.ScrollRect>(); scroll.horizontal = true; scroll.vertical = false;
        scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
        content.SetParent(viewport, false); content.anchorMin = new Vector2(0, 0); content.anchorMax = new Vector2(0, 1);
        content.pivot = new Vector2(0, .5f); content.anchoredPosition = Vector2.zero; content.sizeDelta = new Vector2(0, 0);
        var layout = content.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>(); layout.spacing = 15;
        layout.childAlignment = TextAnchor.MiddleLeft; layout.childControlWidth = false; layout.childControlHeight = false;
        layout.childForceExpandWidth = false; layout.childForceExpandHeight = false;
        var fitter = content.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>(); fitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport = viewport; scroll.content = content; Ref(screen, "m_scroll", scroll);
        foreach (string field in new[] { "m_previous", "m_next", "m_page", "m_reset" })
            ((Component)so.FindProperty(field).objectReferenceValue).gameObject.SetActive(false);
        PrefabUtility.SaveAsPrefabAsset(panel, Prefabs + "BattleSelectionPanel.prefab");
        }
        PrefabUtility.UnloadPrefabContents(panel);
        var hud = PrefabUtility.LoadPrefabContents(Prefabs + "BattleHudPanel.prefab"); var hudScreen = Root<BattleHudPanel>(hud);
        var hudSo = new SerializedObject(hudScreen); s_font = ((TMP_Text)hudSo.FindProperty("m_TxtTurn").objectReferenceValue).font;
        if (hudSo.FindProperty("m_cutin").objectReferenceValue != null)
        {
            PrefabUtility.UnloadPrefabContents(hud);
            return "滑动底栏和召唤演出已配置";
        }
        ((Component)hudSo.FindProperty("m_BtnReset").objectReferenceValue).gameObject.SetActive(false);
        var toolbar = Rect("SessionControls", hud.transform, 0, 0, 700, 54);
        toolbar.anchorMin = toolbar.anchorMax = toolbar.pivot = new Vector2(1, 0); toolbar.anchoredPosition = new Vector2(-20, 16);
        Ref(hudScreen, "m_surrender", Button("DuelSurrender", toolbar, 540, 0, 150, 50, "投降"));
        Ref(hudScreen, "m_return", Button("DuelReturn", toolbar, 540, 0, 150, 50, "返回大厅"));
        var playback = Button("ReplayPlay", toolbar, 60, 0, 150, 50, "暂停"); Ref(hudScreen, "m_replayPlay", playback);
        Ref(hudScreen, "m_replayPlayLabel", Root<TextMeshProUGUI>(playback.transform.GetChild(0).gameObject));
        Ref(hudScreen, "m_replayView", Button("ReplayView", toolbar, 220, 0, 150, 50, "切换视角"));
        Ref(hudScreen, "m_replayExit", Button("ReplayExit", toolbar, 380, 0, 150, 50, "退出回放"));
        var cutinRoot = Rect("SummonCutin", hud.transform, 0, 0, 1706, 960); Stretch(cutinRoot);
        var blocker = cutinRoot.gameObject.AddComponent<UnityEngine.UI.Image>(); blocker.color = new Color(0, 0, 0, .5f); blocker.raycastTarget = true;
        var cutin = cutinRoot.gameObject.AddComponent<BattleCutinView>(); var graphicRect = Rect("Skeleton", cutinRoot, 0, 0, 1706, 960);
        graphicRect.anchorMin = graphicRect.anchorMax = graphicRect.pivot = new Vector2(.5f, .5f); graphicRect.anchoredPosition = Vector2.zero;
        var graphic = graphicRect.gameObject.AddComponent<SkeletonGraphic>(); graphic.allowMultipleCanvasRenderers = true;
        var uiMaterial = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/Spine/Runtime/spine-unity/Materials/UI-PMATexture/SkeletonGraphicDefault.mat"));
        uiMaterial.EnableKeyword("_STRAIGHT_ALPHA_INPUT"); uiMaterial.SetFloat("_StraightAlphaInput", 1);
        graphic.material = Store(uiMaterial, Art + "BattleCutinUI.mat");
        graphic.raycastTarget = false;
        Ref(cutin, "m_overlay", cutinRoot.gameObject); Ref(cutin, "m_viewport", cutinRoot); Ref(cutin, "m_graphic", graphic);
        var cutinSo = new SerializedObject(cutin); var entries = cutinSo.FindProperty("m_entries");
        string[] ids = { "01948619", "19324993", "22908820", "23204029", "32828466", "40044918", "50954680", "55063751", "55171412", "56733747", "58004362", "58481572", "60461804", "63288574", "63767246", "75147529", "89631139", "89943723", "90673289", "93347961" };
        entries.arraySize = ids.Length;
        for (int i = 0; i < ids.Length; i++)
        {
            string folder = "Assets/Art/Spine/BattleCutins/" + ids[i];
            var data = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:SkeletonDataAsset", new[] { folder }).Single()));
            var skeleton = new Skeleton(data.GetSkeletonData(false)); var state = new Spine.AnimationState(new AnimationStateData(data.GetSkeletonData(false)));
            var track = state.SetAnimation(0, "animation", false); var bounds = new Bounds(); bool first = true; float[] vertices = null;
            for (float time = 0; time <= track.Animation.Duration + .02f; time += 1f / 60)
            {
                state.Update(1f / 60); state.Apply(skeleton); skeleton.UpdateWorldTransform(Skeleton.Physics.Update);
                skeleton.GetBounds(out float x, out float y, out float width, out float height, ref vertices);
                if (width <= 0 || height <= 0) continue;
                var frameBounds = new Bounds(new Vector3(x + width / 2, y + height / 2, 0), new Vector3(width, height, 0));
                if (first) { bounds = frameBounds; first = false; } else bounds.Encapsulate(frameBounds);
            }
            var entry = entries.GetArrayElementAtIndex(i); entry.FindPropertyRelative("CardId").stringValue = ids[i];
            entry.FindPropertyRelative("Address").stringValue = "Spine/" + ids[i];
            entry.FindPropertyRelative("Animation").stringValue = "animation"; entry.FindPropertyRelative("SkeletonData").objectReferenceValue = data;
            entry.FindPropertyRelative("Center").vector2Value = bounds.center; entry.FindPropertyRelative("Size").vector2Value = bounds.size;
        }
        cutinSo.ApplyModifiedPropertiesWithoutUndo(); cutinRoot.gameObject.SetActive(false); Ref(hudScreen, "m_cutin", cutin);
        PrefabUtility.SaveAsPrefabAsset(hud, Prefabs + "BattleHudPanel.prefab"); PrefabUtility.UnloadPrefabContents(hud);
        AssetDatabase.SaveAssets(); return "已配置滑动底栏、召唤演出及对战/回放控制";
    }
}
