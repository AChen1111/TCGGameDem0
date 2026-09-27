using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>头像和头像框共用透明商品卡，购买点击独立于旧头像图片节点。</summary>
public static class FixShopPortraitCards
{
    static T Ref<T>(SerializedObject so, string name) where T : Object => (T)so.FindProperty(name).objectReferenceValue;
    static void Place(RectTransform rect, Transform parent, Vector2 position, Vector2 size)
    {
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = position; rect.sizeDelta = size; rect.localScale = Vector3.one;
    }
    public static string Main()
    {
        const string path = "Assets/UI/Prefab/Hall/Shop/AvatarShopItemPrefab.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            // 标题和价格原本是嵌套 Prefab，解包本卡片实例后才能移动内部节点。
            foreach (string name in new[] { "Header", "BuyBottom" })
                PrefabUtility.UnpackPrefabInstance(root.transform.Find(name).gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            var item = Resources.FindObjectsOfTypeAll<ShopOwnedItem>().Single(x => x.gameObject == root);
            var so = new SerializedObject(item);
            var oldButton = Ref<Button>(so, "m_BtnAll");
            var oldImage = Ref<Image>(so, "m_ImgMain");
            var owner = Ref<GameObject>(so, "m_GoOwned");
            var portrait = Ref<AvatarPortraitView>(so, "m_Portrait");
            var title = Ref<TextMeshProUGUI>(so, "m_TxtTitle");
            var time = Ref<TextMeshProUGUI>(so, "m_TxtRemainTime");
            var value = Ref<TextMeshProUGUI>(so, "m_TxtValue");
            var allImages = Resources.FindObjectsOfTypeAll<Image>().Where(x => x.transform.IsChildOf(root.transform)).ToArray();
            var currency = allImages.Single(x => x.name == "icon");
            var ownedCheck = allImages.Single(x => x.name == "OwenImg");

            // 灰白板不是头像框素材；隐藏它，留下真正的装备框和装饰。
            foreach (var image in allImages)
            {
                image.raycastTarget = false;
                if (image.name == "bg") image.enabled = false;
            }
            foreach (var text in Resources.FindObjectsOfTypeAll<TMP_Text>().Where(x => x.transform.IsChildOf(root.transform)))
                text.raycastTarget = false;

            Place((RectTransform)portrait.transform, root.transform, Vector2.zero, new Vector2(216, 216));
            Place(title.rectTransform, root.transform, new Vector2(0, 136), new Vector2(280, 28));
            title.fontSize = 22; title.alignment = TextAlignmentOptions.Center;
            Place(time.rectTransform, root.transform, new Vector2(0, 116), new Vector2(280, 16));
            time.fontSize = 14; time.alignment = TextAlignmentOptions.Center;
            Place(value.rectTransform, root.transform, new Vector2(22, -131), new Vector2(156, 34));
            value.fontSize = 30; value.alignment = TextAlignmentOptions.Center;
            Place(currency.rectTransform, root.transform, new Vector2(-60, -131), new Vector2(60, 30));
            currency.preserveAspect = true;
            Place((RectTransform)owner.transform, root.transform, new Vector2(-108, 94), new Vector2(42, 42));
            Place(ownedCheck.rectTransform, owner.transform, Vector2.zero, new Vector2(42, 42));
            ownedCheck.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Sprite/ProfileCustomization/Common/GUI_CommonButtonPlof_Check_On.png");
            ownedCheck.color = Color.white; ownedCheck.preserveAspect = true;
            owner.transform.SetAsLastSibling();
            oldImage.gameObject.SetActive(false);
            root.transform.Find("Header").gameObject.SetActive(false);
            root.transform.Find("BuyBottom").gameObject.SetActive(false);

            // 原按钮与 m_ImgMain 在同一物体上，会被 SetData 隐藏；新点击层始终留在格子根节点。
            var hitArea = root.AddComponent<Image>(); hitArea.color = Color.clear; hitArea.raycastTarget = true;
            var button = root.AddComponent<Button>(); button.targetGraphic = hitArea; button.transition = Selectable.Transition.None;
            so.FindProperty("m_BtnAll").objectReferenceValue = button;
            so.ApplyModifiedPropertiesWithoutUndo();
            Object.DestroyImmediate(oldButton);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            return "透明商品卡已保存；头像和头像框共用根节点购买按钮，已拥有勾选移出旧图片节点。";
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
