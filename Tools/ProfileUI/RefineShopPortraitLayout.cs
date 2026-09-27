using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>清理旧文本边距和行实例的价格定位覆盖，统一继承透明商品卡布局。</summary>
public static class RefineShopPortraitLayout
{
    public static string Main()
    {
        const string cardPath = "Assets/UI/Prefab/Hall/Shop/AvatarShopItemPrefab.prefab";
        var card = PrefabUtility.LoadPrefabContents(cardPath);
        try
        {
            foreach (var text in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>().Where(x => x.transform.parent == card.transform))
            {
                text.margin = Vector4.zero;
                text.enableAutoSizing = false;
                text.fontSize = text.name == "Txt_Value" ? 30 : text.name == "Txt_Titile" ? 22 : 14;
            }
            PrefabUtility.SaveAsPrefabAsset(card, cardPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(card); }

        var source = AssetDatabase.LoadAssetAtPath<GameObject>(cardPath);
        var price = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>().Single(x => x.transform.IsChildOf(source.transform) && x.name == "Txt_Value").rectTransform;
        var currency = Resources.FindObjectsOfTypeAll<Image>().Single(x => x.transform.IsChildOf(source.transform) && x.name == "icon").rectTransform;
        const string rowPath = "Assets/UI/Prefab/Hall/Shop/AvatarShopItemRowPrefab.prefab";
        var row = PrefabUtility.LoadPrefabContents(rowPath);
        try
        {
            foreach (var item in Resources.FindObjectsOfTypeAll<ShopOwnedItem>().Where(x => x.transform.IsChildOf(row.transform)))
            {
                var modifications = PrefabUtility.GetPropertyModifications(item.gameObject);
                PrefabUtility.SetPropertyModifications(item.gameObject, modifications.Where(x => x.target != price && x.target != currency).ToArray());
            }
            PrefabUtility.SaveAsPrefabAsset(row, rowPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(row); }
        return "商品标题与价格边距已清理，五格行使用商品卡的价格与图标定位。";
    }
}
